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
   
    public partial class RealTimeDataForm : Form
    {
        private DatabaseHelper dbHelper;
        public RealTimeDataForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadStudentsData();
            InitReportTemplate();

        }
        public SqlConnection GetConnection()
        {
            return new SqlConnection("Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;");
        }
        private void InitReportTemplate()
        {
            
        }
        private void LoadStudentsData()
        {
            try
            {
                DataTable students = dbHelper.LoadStudents();
                dataGridViewStudents.DataSource = students;
                dataGridViewStudents.Refresh(); // Обновление отображения
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }


        

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void RealTimeDataForm_Load(object sender, EventArgs e)
        {

            //this.reportViewer1.RefreshReport();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
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
            if (string.IsNullOrEmpty(txtDiagnosis.Text))
            {
                MessageBox.Show("Введите Диагноз!");
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
                    diagnosis: txtDiagnosis.Text
                );

                LoadStudentsData();
                MessageBox.Show("Воспитанник успешно добавлен!");
            }
            catch (SqlException ex) when (ex.Number == 2627) 
            {
                MessageBox.Show("Ошибка: Воспитанник с таким номером дела уже существует");
            }
            catch (SqlException ex) when (ex.Number == 547) 
            {
                MessageBox.Show("Ошибка: Указан несуществующий диагноз");
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {GetFriendlyErrorMessage(ex)}");
            }
        
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
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
        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                
                if (dataGridViewStudents.SelectedRows.Count == 0 ||
                    dataGridViewStudents.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите корректную запись для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];
                if (selectedRow.IsNewRow)
                {
                    MessageBox.Show("Невозможно удалить новую (несохраненную) строку!");
                    return;
                }

                // Проверяем существование столбца
                if (!dataGridViewStudents.Columns.Contains("Номер_личного_дела_воспитанника"))
                {
                    MessageBox.Show("Ошибка конфигурации: столбец 'Номер_личного_дела' не найден.");
                    return;
                }

                // Получаем номер личного дела
                int selectedNumber = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Удалить запись №{selectedNumber}?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                // Удаление и обновление данных
                dbHelper.DeleteStudent(selectedNumber);
                LoadStudentsData();
                MessageBox.Show("Запись удалена!");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Ошибка: Некорректный выбор строки. " + ex.Message);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить воспитанника: существуют связанные записи");
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка удаления: {GetFriendlyErrorMessage(ex)}");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка: {ex.Message}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка выбранной строки
                if (dataGridViewStudents.SelectedRows.Count == 0 || dataGridViewStudents.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите запись для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];

                // Извлечение данных из строки
                txtNumb.Text = selectedRow.Cells["Номер_личного_дела_воспитанника"].Value.ToString();
                txtFIO.Text = selectedRow.Cells["ФИО"].Value.ToString();
                dateTimePickerBirthDate.Value = Convert.ToDateTime(selectedRow.Cells["Дата_рождения"].Value);
                comboBoxGender.SelectedItem = selectedRow.Cells["Пол"].Value.ToString();
                txtDiagnosis.Text = selectedRow.Cells["Основной_диагноз"].Value.ToString();
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Конфликт уникальности при обновлении");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        /*private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка выбранной строки
                if (dataGridViewStudents.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите запись для сохранения!");
                    return;
                }

                // Проверка обязательных полей
                if (string.IsNullOrEmpty(txtFIO.Text) || comboBoxGender.SelectedItem == null)
                {
                    MessageBox.Show("Заполните обязательные поля (ФИО, Пол)!");
                    return;
                }

                // Получение номера дела из выделенной строки
                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];
                int currentNumber = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);
                
                // Обновление данных в БД
                dbHelper.UpdateStudent(
                    currentNumber,
                    txtFIO.Text,
                    dateTimePickerBirthDate.Value,
                    comboBoxGender.SelectedItem.ToString(),
                    txtDiagnosis.Text
                );
                LoadStudentsData();
                MessageBox.Show("Изменения сохранены!");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
*/
        private void button3_Click(object sender, EventArgs e)
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

                // Получаем данные из интерфейса
                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];
                int originalNumber = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);
                int newNumber = Convert.ToInt32(txtNumb.Text);

                // Проверка изменения номера
                if (newNumber != originalNumber)
                {
                    // Проверяем существование нового номера в БД
                    if (dbHelper.IsStudentNumberExists(newNumber))
                    {
                        MessageBox.Show("Ошибка: Воспитанник с таким номером дела уже существует");
                        return;
                    }
                }

                // Обновление данных в БД
                dbHelper.UpdateStudent(
                    originalNumber,
                    txtFIO.Text,
                    dateTimePickerBirthDate.Value,
                    comboBoxGender.SelectedItem.ToString(),
                    txtDiagnosis.Text,
                    newNumber // Передаем новый номер, если он изменился
                );

                LoadStudentsData();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        private void buttonBack_Click(object sender, EventArgs e)
        {

        }
        private void buttonReport_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Загрузка шаблона
                using (Report report = new Report()) 
                {
                    string reportPath = Path.Combine(Application.StartupPath, @"Reports\StudentsReport.frx");

                    if (!File.Exists(reportPath))
                    {
                        MessageBox.Show("Файл шаблона отчёта не найден!");
                        return;
                    }

                    report.Load(reportPath);

                    // 2. Получение данных
                    DataTable students = dbHelper.LoadStudents();
                    int totalCount = students.Rows.Count;

                    // 3. Регистрация данных
                    report.RegisterData(students, "Воспитанник");
                    report.GetDataSource("Воспитанник").Enabled = true;

                    // 4. Установка параметров
                    report.SetParameterValue("TotalCount", totalCount);

                    // 5. Подготовка и показ отчёта
                    report.Prepare();
                    report.Show();

                    /*using (PreviewForm preview = new PreviewForm())
                    {
                        preview.Report = report; // ← Используем существующий report
                        preview.ShowDialog();

                    }*/
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}");
            }
        }
    }
    public class DatabaseHelper
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PansionatDBConnection"].ConnectionString;
        
        public DataTable LoadStudents()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Воспитанник";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }
        public void AddStudent(int number, string fio, DateTime birthDate, string gender, string diagnosis)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO Воспитанник 
                (Номер_личного_дела_воспитанника, ФИО, Дата_рождения, Пол, Основной_диагноз) 
            VALUES 
                (@Numb, @FIO, @BirthDate, @Gender, @Diagnosis)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Numb", number);
                cmd.Parameters.AddWithValue("@FIO", fio);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteStudent(int number)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Воспитанник WHERE Номер_личного_дела_воспитанника = @Number";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Number", number);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public int GetMaxStudentNumber()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT MAX(Номер_личного_дела) FROM Воспитанник";
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        public void UpdateStudent(int currentNumber, string fio, DateTime birthDate, string gender, string diagnosis)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            UPDATE Воспитанник 
            SET 
                ФИО = @FIO,
                Дата_рождения = @BirthDate,
                Пол = @Gender,
                Основной_диагноз = @Diagnosis
            WHERE 
                Номер_личного_дела_воспитанника = @CurrentNumber";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@FIO", fio);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);
                cmd.Parameters.AddWithValue("@CurrentNumber", currentNumber);

                connection.Open();
                cmd.ExecuteNonQuery();
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

        public void UpdateStudent(int originalNumber, string fio, DateTime birthDate, string gender, string diagnosis, int newNumber)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Воспитанник 
                SET 
                    Номер_личного_дела_воспитанника = @NewNumber,
                    ФИО = @FIO,
                    Дата_рождения = @BirthDate,
                    Пол = @Gender,
                    Основной_диагноз = @Diagnosis
                WHERE 
                    Номер_личного_дела_воспитанника = @OriginalNumber";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                cmd.Parameters.AddWithValue("@FIO", fio);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);
                cmd.Parameters.AddWithValue("@OriginalNumber", originalNumber);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

}
