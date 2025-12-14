using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using FastReport;
using FastReport.Data;
using System.IO;
using Pasionat;
using FastReport.Preview;

namespace Pansionat
{
    public partial class _2_RealTimeDataForm : Form
    {
        private DatabaseHelper dbHelper;
        private DataTable diagnosesData;
        private DataTable guardiansData;
        private DataTable equipmentData;

        public _2_RealTimeDataForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadStudentsData();
            LoadComboBoxData();
            InitReportTemplate();
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Загрузка диагнозов
                diagnosesData = dbHelper.LoadDiagnoses();
                comboBoxDiagnosis.DataSource = diagnosesData;
                comboBoxDiagnosis.DisplayMember = "Название";
                comboBoxDiagnosis.ValueMember = "Код_заболевания";
                comboBoxDiagnosis.SelectedIndex = -1;

                // Загрузка опекунов
                guardiansData = dbHelper.LoadGuardians();
                comboBoxGuardian.DataSource = guardiansData;
                comboBoxGuardian.DisplayMember = "ФИО_опекуна";
                comboBoxGuardian.ValueMember = "ФИО_опекуна";
                comboBoxGuardian.SelectedIndex = -1;

                // Загрузка мед оборудования
                equipmentData = dbHelper.LoadMedicalEquipment();
                comboBoxEquipment.DataSource = equipmentData;
                comboBoxEquipment.DisplayMember = "Название";
                comboBoxEquipment.ValueMember = "Инвентарный_номер_оборудования";
                comboBoxEquipment.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных для ComboBox: " + ex.Message);
            }
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection("Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;");
        }

        private void InitReportTemplate()
        {
            // Инициализация шаблона отчета
        }

        private void LoadStudentsData()
        {
            try
            {
                DataTable students = dbHelper.LoadStudents();
                dataGridViewStudents.DataSource = students;
                dataGridViewStudents.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void RealTimeDataForm_Load(object sender, EventArgs e)
        {
            // Загрузка формы
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFIO.Text))
            {
                MessageBox.Show("Введите ФИО!");
                return;
            }
            if (comboBoxGender.SelectedItem == null)
            {
                MessageBox.Show("Выберите пол!");
                return;
            }
            if (comboBoxDiagnosis.SelectedValue == null)
            {
                MessageBox.Show("Выберите диагноз!");
                return;
            }
            if (comboBoxGuardian.SelectedValue == null)
            {
                MessageBox.Show("Выберите опекуна!");
                return;
            }
            if (comboBoxEquipment.SelectedValue == null)
            {
                MessageBox.Show("Выберите мед оборудование!");
                return;
            }
            if (!int.TryParse(txtNumb.Text, out int number))
            {
                MessageBox.Show("Номер должен быть целым числом!");
                return;
            }

            try
            {
                dbHelper.AddStudent(
                    number: number,
                    fio: txtFIO.Text,
                    birthDate: dateTimePickerBirthDate.Value,
                    gender: comboBoxGender.SelectedItem.ToString(),
                    diagnosisId: comboBoxDiagnosis.SelectedValue.ToString(), // Изменено на string
                    guardianName: comboBoxGuardian.SelectedValue.ToString(),
                    equipmentInventoryNumber: (int)comboBoxEquipment.SelectedValue
                );

                LoadStudentsData();
                ClearForm();
                MessageBox.Show("Воспитанник успешно добавлен!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Воспитанник с таким номером дела уже существует");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Ошибка: Указан несуществующий диагноз, опекун или оборудование");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            txtNumb.Clear();
            txtFIO.Clear();
            dateTimePickerBirthDate.Value = DateTime.Now;
            comboBoxGender.SelectedIndex = -1;
            comboBoxDiagnosis.SelectedIndex = -1;
            comboBoxGuardian.SelectedIndex = -1;
            comboBoxEquipment.SelectedIndex = -1;
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewStudents.SelectedRows.Count == 0 ||
                    dataGridViewStudents.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите запись для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];

                // Заполняем основные поля
                txtNumb.Text = selectedRow.Cells["Номер_личного_дела_воспитанника"].Value.ToString();
                txtFIO.Text = selectedRow.Cells["ФИО"].Value.ToString();
                dateTimePickerBirthDate.Value = Convert.ToDateTime(selectedRow.Cells["Дата_рождения"].Value);
                comboBoxGender.SelectedItem = selectedRow.Cells["Пол"].Value.ToString();

                // Устанавливаем значения ComboBox
                string diagnosisName = selectedRow.Cells["Основной_диагноз"].Value?.ToString();
                string guardianName = selectedRow.Cells["ФИО_опекуна"].Value?.ToString();
                string equipmentName = selectedRow.Cells["Мед_оборудование"].Value?.ToString();

                // Находим соответствующие значения в ComboBox
                if (!string.IsNullOrEmpty(diagnosisName))
                {
                    var diagnosisRow = diagnosesData.Select($"Название = '{diagnosisName.Replace("'", "''")}'").FirstOrDefault();
                    if (diagnosisRow != null)
                        comboBoxDiagnosis.SelectedValue = diagnosisRow["Код_заболевания"];
                    else
                        comboBoxDiagnosis.SelectedIndex = -1;
                }
                else
                {
                    comboBoxDiagnosis.SelectedIndex = -1;
                }

                if (!string.IsNullOrEmpty(guardianName))
                {
                    comboBoxGuardian.SelectedValue = guardianName;
                }
                else
                {
                    comboBoxGuardian.SelectedIndex = -1;
                }

                if (!string.IsNullOrEmpty(equipmentName))
                {
                    var equipmentRow = equipmentData.Select($"Название = '{equipmentName.Replace("'", "''")}'").FirstOrDefault();
                    if (equipmentRow != null)
                        comboBoxEquipment.SelectedValue = equipmentRow["Инвентарный_номер_оборудования"];
                    else
                        comboBoxEquipment.SelectedIndex = -1;
                }
                else
                {
                    comboBoxEquipment.SelectedIndex = -1;
                }

                // Переключаемся в режим редактирования
                buttonAdd.Enabled = false;
                buttonSave.Enabled = true;
                buttonDelete.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewStudents.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите запись для сохранения!");
                    return;
                }

                // Проверка обязательных полей
                if (string.IsNullOrEmpty(txtFIO.Text))
                {
                    MessageBox.Show("Заполните поле ФИО!");
                    return;
                }

                if (comboBoxGender.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пол!");
                    return;
                }

                if (!int.TryParse(txtNumb.Text, out int newNumber))
                {
                    MessageBox.Show("Номер должен быть целым числом!");
                    return;
                }

                if (comboBoxDiagnosis.SelectedValue == null)
                {
                    MessageBox.Show("Выберите диагноз!");
                    return;
                }

                if (comboBoxGuardian.SelectedValue == null)
                {
                    MessageBox.Show("Выберите опекуна!");
                    return;
                }

                if (comboBoxEquipment.SelectedValue == null)
                {
                    MessageBox.Show("Выберите мед оборудование!");
                    return;
                }

                // Получаем оригинальный номер из выбранной строки
                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];
                int originalNumber = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);

                // Проверка изменения номера
                if (newNumber != originalNumber)
                {
                    if (dbHelper.IsStudentNumberExists(newNumber))
                    {
                        MessageBox.Show("Ошибка: Воспитанник с таким номером дела уже существует");
                        return;
                    }
                }

                // Обновление данных в БД
                dbHelper.UpdateStudent(
                    originalNumber: originalNumber,
                    newNumber: newNumber,
                    fio: txtFIO.Text,
                    birthDate: dateTimePickerBirthDate.Value,
                    gender: comboBoxGender.SelectedItem.ToString(),
                    diagnosisId: comboBoxDiagnosis.SelectedValue.ToString(), // Изменено на string
                    guardianName: comboBoxGuardian.SelectedValue.ToString(),
                    equipmentInventoryNumber: (int)comboBoxEquipment.SelectedValue
                );

                LoadStudentsData();
                ClearForm();

                // Возвращаем кнопки в исходное состояние
                buttonAdd.Enabled = true;
                buttonSave.Enabled = false;
                buttonDelete.Enabled = true;

                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            buttonAdd.Enabled = true;
            buttonSave.Enabled = false;
            buttonDelete.Enabled = true;
        }

        private string GetFriendlyErrorMessage(SqlException ex)
        {
            switch (ex.Number)
            {
                case 18456:
                    return "Неверные учетные данные базы данных";
                case 4060:
                    return "Ошибка подключения к базе данных";
                case 208:
                    return "Несуществующая таблица";
                default:
                    return ex.Message;
            }
        }
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewStudents.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите запись для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];

                // Проверяем, что строка не является новой пустой строкой
                if (selectedRow.IsNewRow)
                {
                    MessageBox.Show("Выберите существующую запись для удаления!");
                    return;
                }

                // Проверяем, что номер дела не пустой
                if (selectedRow.Cells["Номер_личного_дела_воспитанника"].Value == null)
                {
                    MessageBox.Show("Не удалось получить номер дела выбранной записи!");
                    return;
                }

                int selectedNumber = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);

                DialogResult result = MessageBox.Show(
                    $"Удалить воспитанника с номером дела {selectedNumber}?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteStudent(selectedNumber);
                LoadStudentsData();
                ClearForm();
                MessageBox.Show("Запись успешно удалена!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить воспитанника: существуют связанные записи в других таблицах");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }

        private void buttonReport_Click(object sender, EventArgs e)
        {
            try
            {
                using (Report report = new Report())
                {
                    string reportPath = Path.Combine(Application.StartupPath, @"Reports\StudentsReport.frx");

                    if (!File.Exists(reportPath))
                    {
                        MessageBox.Show("Файл шаблона отчёта не найден!");
                        return;
                    }

                    report.Load(reportPath);

                    DataTable students = dbHelper.LoadStudents();
                    int totalCount = students.Rows.Count;

                    report.RegisterData(students, "Воспитанник");
                    report.GetDataSource("Воспитанник").Enabled = true;

                    report.SetParameterValue("TotalCount", totalCount);

                    report.Prepare();
                    report.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}");
            }
        }

        // Остальные методы меню остаются без изменений
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = true;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DiagnosisForm diagnosisForm = new DiagnosisForm();
            groupBox2.Visible = false;
            diagnosisForm.ShowDialog();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            DrugForm drugForm = new DrugForm();
            groupBox2.Visible = false;
            drugForm.ShowDialog();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            GuardianForm gForm = new GuardianForm();
            groupBox2.Visible = false;
            gForm.ShowDialog();
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            SubjAreaForm sForm = new SubjAreaForm();
            groupBox2.Visible = false;
            sForm.ShowDialog();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            MedEquipmentForm mForm = new MedEquipmentForm();
            groupBox2.Visible = false;
            mForm.ShowDialog();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            TeacherForm sForm = new TeacherForm();
            groupBox2.Visible = false;
            sForm.ShowDialog();
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            CurriculumForm sForm = new CurriculumForm();
            groupBox2.Visible = false;
            sForm.ShowDialog();
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            GuardianForm sForm = new GuardianForm();
            groupBox2.Visible = false;
            sForm.ShowDialog();
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            LessonForm sForm = new LessonForm();
            groupBox2.Visible = false;
            sForm.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void progToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void информацияОбОбученииToolStripMenuItem_Click(object sender, EventArgs e) {
            EducationalData sForm = new EducationalData();
            groupBox2.Visible = false;
            sForm.ShowDialog();

        }

        private void инфорацияОЛеченииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HealingFormcs sForm = new HealingFormcs();
            groupBox2.Visible = false;
            sForm.ShowDialog();
        }

       
    }

    public class DatabaseHelper
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PansionatDBConnection"].ConnectionString;

        public void AddStudent(int number, string fio, DateTime birthDate, string gender,
                         string diagnosisId, string guardianName, int equipmentInventoryNumber) // Изменено на string diagnosisId
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Добавляем воспитанника
                    string query = @"
                    INSERT INTO Воспитанник 
                        (Номер_личного_дела_воспитанника, ФИО, Дата_рождения, Пол, Основной_диагноз) 
                    VALUES 
                        (@Numb, @FIO, @BirthDate, @Gender, 
                         (SELECT Название FROM Диагноз WHERE Код_заболевания = @DiagnosisId))";

                    SqlCommand cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Numb", number);
                    cmd.Parameters.AddWithValue("@FIO", fio);
                    cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@DiagnosisId", diagnosisId); // Теперь string
                    cmd.ExecuteNonQuery();

                    // 2. Добавляем связь с диагнозом
                    query = @"
                    INSERT INTO Диагноз_воспитанника 
                        (Код_заболевания, Номер_личного_дела_воспитанника) 
                    VALUES 
                        (@DiagnosisId, @Numb)";

                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@DiagnosisId", diagnosisId); // Теперь string
                    cmd.Parameters.AddWithValue("@Numb", number);
                    cmd.ExecuteNonQuery();

                    // 3. Добавляем связь с опекуном
                    query = @"
                    INSERT INTO Опекун_воспитанника 
                        (ФИО_опекуна, Номер_личного_дела_воспитанника) 
                    VALUES 
                        (@GuardianName, @Numb)";

                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@GuardianName", guardianName);
                    cmd.Parameters.AddWithValue("@Numb", number);
                    cmd.ExecuteNonQuery();

                    // 4. Обновляем мед оборудование
                    query = @"
                    UPDATE Мед_оборудование 
                    SET Номер_личного_дела_воспитанника = @Numb
                    WHERE Инвентарный_номер_оборудования = @EquipmentInventoryNumber";

                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Numb", number);
                    cmd.Parameters.AddWithValue("@EquipmentInventoryNumber", equipmentInventoryNumber);
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public DataTable LoadStudents()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    в.Номер_личного_дела_воспитанника,
                    в.ФИО,
                    в.Дата_рождения,
                    в.Пол,
                    в.Основной_диагноз,
                    ов.ФИО_опекуна,
                    м.Название AS Мед_оборудование
                FROM Воспитанник в
                LEFT JOIN Опекун_воспитанника ов ON в.Номер_личного_дела_воспитанника = ов.Номер_личного_дела_воспитанника
                LEFT JOIN Мед_оборудование м ON в.Номер_личного_дела_воспитанника = м.Номер_личного_дела_воспитанника";

                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadDiagnoses()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Код_заболевания, Название FROM Диагноз";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadGuardians()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ФИО_опекуна FROM Опекун";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadMedicalEquipment()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Инвентарный_номер_оборудования, Название FROM Мед_оборудование";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void DeleteStudent(int number)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Сначала обновляем мед оборудование (убираем связь)
                    string query = "UPDATE Мед_оборудование SET Номер_личного_дела_воспитанника = NULL WHERE Номер_личного_дела_воспитанника = @Number";
                    SqlCommand cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Number", number);
                    cmd.ExecuteNonQuery();

                    // 2. Удаляем связи с диагнозами
                    query = "DELETE FROM Диагноз_воспитанника WHERE Номер_личного_дела_воспитанника = @Number";
                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Number", number);
                    cmd.ExecuteNonQuery();

                    // 3. Удаляем связи с опекунами
                    query = "DELETE FROM Опекун_воспитанника WHERE Номер_личного_дела_воспитанника = @Number";
                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Number", number);
                    cmd.ExecuteNonQuery();

                    // 4. Удаляем воспитанника
                    query = "DELETE FROM Воспитанник WHERE Номер_личного_дела_воспитанника = @Number";
                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Number", number);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Запись не найдена в базе данных");
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateStudent(int originalNumber, int newNumber, string fio, DateTime birthDate,
                        string gender, string diagnosisId, string guardianName, int equipmentInventoryNumber) // Изменено на string diagnosisId
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Обновляем основные данные воспитанника
                    string query = @"
                    UPDATE Воспитанник 
                    SET 
                        Номер_личного_дела_воспитанника = @NewNumber,
                        ФИО = @FIO,
                        Дата_рождения = @BirthDate,
                        Пол = @Gender,
                        Основной_диагноз = (SELECT Название FROM Диагноз WHERE Код_заболевания = @DiagnosisId)
                    WHERE Номер_личного_дела_воспитанника = @OriginalNumber";

                    SqlCommand cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                    cmd.Parameters.AddWithValue("@FIO", fio);
                    cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@DiagnosisId", diagnosisId); // Теперь string
                    cmd.Parameters.AddWithValue("@OriginalNumber", originalNumber);
                    cmd.ExecuteNonQuery();

                    // 2. Обновляем связь с диагнозом
                    query = @"
                    UPDATE Диагноз_воспитанника 
                    SET Код_заболевания = @DiagnosisId
                    WHERE Номер_личного_дела_воспитанника = @NewNumber";

                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@DiagnosisId", diagnosisId); // Теперь string
                    cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Если связи не было, создаем новую
                    if (rowsAffected == 0)
                    {
                        query = @"
                        INSERT INTO Диагноз_воспитанника (Код_заболевания, Номер_личного_дела_воспитанника)
                        VALUES (@DiagnosisId, @NewNumber)";
                        cmd = new SqlCommand(query, connection, transaction);
                        cmd.Parameters.AddWithValue("@DiagnosisId", diagnosisId); // Теперь string
                        cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Обновляем связь с опекуном
                    query = @"
                    UPDATE Опекун_воспитанника 
                    SET ФИО_опекуна = @GuardianName
                    WHERE Номер_личного_дела_воспитанника = @NewNumber";

                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@GuardianName", guardianName);
                    cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                    rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        query = @"
                        INSERT INTO Опекун_воспитанника (ФИО_опекуна, Номер_личного_дела_воспитанника)
                        VALUES (@GuardianName, @NewNumber)";
                        cmd = new SqlCommand(query, connection, transaction);
                        cmd.Parameters.AddWithValue("@GuardianName", guardianName);
                        cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                        cmd.ExecuteNonQuery();
                    }

                    // 4. Обновляем мед оборудование
                    // Сначала освобождаем текущее оборудование
                    query = @"
                    UPDATE Мед_оборудование 
                    SET Номер_личного_дела_воспитанника = NULL 
                    WHERE Номер_личного_дела_воспитанника = @NewNumber";

                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                    cmd.ExecuteNonQuery();

                    // Затем привязываем новое оборудование
                    query = @"
                    UPDATE Мед_оборудование 
                    SET Номер_личного_дела_воспитанника = @NewNumber
                    WHERE Инвентарный_номер_оборудования = @EquipmentInventoryNumber";

                    cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                    cmd.Parameters.AddWithValue("@EquipmentInventoryNumber", equipmentInventoryNumber);
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public bool IsStudentNumberExists(int number)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Воспитанник WHERE Номер_личного_дела_воспитанника = @Number";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Number", number);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private string GetFriendlyErrorMessage(SqlException ex)
        {
            switch (ex.Number)
            {
                case 18456:
                    return "Неверные учетные данные базы данных";
                case 4060:
                    return "Ошибка подключения к базе данных";
                case 208:
                    return "Несуществующая таблица";
                default:
                    return ex.Message;
            }
        }
    }
}