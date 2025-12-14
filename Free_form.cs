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

namespace Pasionat
{
    public partial class Free_form : Form
    {
        public Free_form()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperSubject();
            CheckDatabaseConnection();

            LoadSubjectsData();
            LoadSubjectNamesComboBox();
            LoadSubjectAreasComboBox();
            ClearFields();
        }

        private DatabaseHelperSubject dbHelper;

        private void CheckDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbHelper.connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Подключение к базе данных успешно!", "Диагностика");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка");
            }
        }

        private void LoadSubjectsData()
        {
            try
            {
                MessageBox.Show("Начинаем загрузку данных для таблицы...", "Диагностика");

                DataTable subjects = dbHelper.LoadSubjects();

                // Диагностика
                MessageBox.Show($"Загружено строк: {subjects.Rows.Count}", "Диагностика");
                foreach (DataColumn column in subjects.Columns)
                {
                    MessageBox.Show($"Столбец: {column.ColumnName}, Type: {column.DataType}", "Диагностика");
                }

                dataGridViewSubjects.DataSource = null; // Очищаем предыдущие данные
                dataGridViewSubjects.DataSource = subjects;
                dataGridViewSubjects.Refresh();

                if (dataGridViewSubjects.Rows.Count == 0)
                {
                    MessageBox.Show("Таблица пуста после привязки данных", "Диагностика");
                }
                else
                {
                    MessageBox.Show($"В таблице отображается {dataGridViewSubjects.Rows.Count} строк", "Диагностика");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки учебных предметов: {ex.Message}\n\nДетали: {ex.ToString()}", "Ошибка");
            }
        }

        private void LoadSubjectNamesComboBox()
        {
            try
            {
                MessageBox.Show("Начинаем загрузку данных для комбобокса названий предметов...", "Диагностика");

                DataTable subjectNames = dbHelper.LoadSubjectNames();

                MessageBox.Show($"Загружено названий предметов: {subjectNames.Rows.Count}", "Диагностика");
                foreach (DataColumn column in subjectNames.Columns)
                {
                    MessageBox.Show($"Столбец названий: {column.ColumnName}, Type: {column.DataType}", "Диагностика");
                }

                comboBoxSubjectName.DataSource = subjectNames;
                comboBoxSubjectName.DisplayMember = "Название_учебного_предмета";
                comboBoxSubjectName.ValueMember = "Код_учебного_предмета";

                MessageBox.Show($"Комбобокс названий содержит {comboBoxSubjectName.Items.Count} элементов", "Диагностика");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки названий учебных предметов: {ex.Message}", "Ошибка");
            }
        }

        private void LoadSubjectAreasComboBox()
        {
            try
            {
                MessageBox.Show("Начинаем загрузку данных для комбобокса предметных областей...", "Диагностика");

                DataTable subjectAreas = dbHelper.LoadSubjectAreasForComboBox();

                MessageBox.Show($"Загружено предметных областей: {subjectAreas.Rows.Count}", "Диагностика");
                foreach (DataColumn column in subjectAreas.Columns)
                {
                    MessageBox.Show($"Столбец областей: {column.ColumnName}, Type: {column.DataType}", "Диагностика");
                }

                comboBoxSubjectAreaCode.DataSource = subjectAreas;
                comboBoxSubjectAreaCode.DisplayMember = "Название_предметной_области";
                comboBoxSubjectAreaCode.ValueMember = "Код_предметной_области";

                MessageBox.Show($"Комбобокс областей содержит {comboBoxSubjectAreaCode.Items.Count} элементов", "Диагностика");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметных областей: {ex.Message}", "Ошибка");
            }
        }

        private void ClearFields()
        {
            if (comboBoxSubjectName.Items.Count > 0)
                comboBoxSubjectName.SelectedIndex = 0;
            txtHours.Text = "";
            if (comboBoxSubjectAreaCode.Items.Count > 0)
                comboBoxSubjectAreaCode.SelectedIndex = 0;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (comboBoxSubjectName.SelectedValue == null)
            {
                MessageBox.Show("Выберите учебный предмет!");
                return;
            }

            if (comboBoxSubjectAreaCode.SelectedValue == null)
            {
                MessageBox.Show("Выберите предметную область!");
                return;
            }

            try
            {
                // Преобразуем Text в int для часов
                if (!int.TryParse(txtHours.Text, out int hours))
                {
                    MessageBox.Show("Объем в часах должен быть числом!");
                    return;
                }

                int subjectNameId = (int)comboBoxSubjectName.SelectedValue;
                int subjectAreaCode = (int)comboBoxSubjectAreaCode.SelectedValue;

                dbHelper.AddSubject(
                    subjectNameId: subjectNameId,
                    hours: hours,
                    subjectAreaCode: subjectAreaCode
                );

                LoadSubjectsData();
                ClearFields();
                MessageBox.Show("Учебный предмет успешно добавлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSubjects.SelectedRows.Count == 0 ||
                    dataGridViewSubjects.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите учебный предмет для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewSubjects.SelectedRows[0];
                int selectedSubjectId = Convert.ToInt32(selectedRow.Cells["Код_учебного_предмета"].Value);
                string subjectName = selectedRow.Cells["Название_учебного_предмета"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Удалить учебный предмет '{subjectName}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteSubject(selectedSubjectId);
                LoadSubjectsData();
                ClearFields();
                MessageBox.Show("Учебный предмет удален!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить учебный предмет: существуют связанные записи");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSubjects.SelectedRows.Count == 0 ||
                    dataGridViewSubjects.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите учебный предмет для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewSubjects.SelectedRows[0];

                // Устанавливаем значения в комбобоксы
                int subjectNameId = Convert.ToInt32(selectedRow.Cells["Код_учебного_предмета"].Value);
                int subjectAreaCode = Convert.ToInt32(selectedRow.Cells["Код_предметной_области"].Value);

                comboBoxSubjectName.SelectedValue = subjectNameId;
                txtHours.Text = selectedRow.Cells["Объем_в_часах"].Value.ToString();
                comboBoxSubjectAreaCode.SelectedValue = subjectAreaCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных учебного предмета: " + ex.Message);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSubjects.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите учебный предмет для сохранения!");
                    return;
                }

                if (comboBoxSubjectName.SelectedValue == null)
                {
                    MessageBox.Show("Выберите учебный предмет!");
                    return;
                }

                if (comboBoxSubjectAreaCode.SelectedValue == null)
                {
                    MessageBox.Show("Выберите предметную область!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewSubjects.SelectedRows[0];
                int originalSubjectId = Convert.ToInt32(selectedRow.Cells["Код_учебного_предмета"].Value);

                // Преобразуем Text в int
                if (!int.TryParse(txtHours.Text, out int hours))
                {
                    MessageBox.Show("Объем в часах должен быть числом!");
                    return;
                }

                int subjectNameId = (int)comboBoxSubjectName.SelectedValue;
                int subjectAreaCode = (int)comboBoxSubjectAreaCode.SelectedValue;

                dbHelper.UpdateSubject(
                    originalSubjectId,
                    subjectNameId,
                    hours,
                    subjectAreaCode
                );

                LoadSubjectsData();
                ClearFields();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void buttonAdd_Click_1(object sender, EventArgs e)
        {

        }
    }

    public class DatabaseHelperSubject
    {
        public string connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;";

        public DataTable LoadSubjects()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    string query = @"
                        SELECT 
                            Код_учебного_предмета,
                            Название_учебного_предмета,
                            Объем_в_часах,
                            Код_предметной_области
                        FROM Учебный_предмет 
                        ORDER BY Название_учебного_предмета";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    connection.Open();
                    adapter.Fill(dt);
                    connection.Close();

                    MessageBox.Show($"Запрос выполнен успешно, получено {dt.Rows.Count} строк", "Диагностика БД");
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"SQL Error: {sqlEx.Message}\nNumber: {sqlEx.Number}", "Ошибка БД");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"General Error in LoadSubjects: {ex.Message}", "Ошибка БД");
                }
            }
            return dt;
        }

        public DataTable LoadSubjectNames()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT Код_учебного_предмета, Название_учебного_предмета FROM Учебный_предмет ORDER BY Название_учебного_предмета";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    connection.Open();
                    adapter.Fill(dt);
                    connection.Close();

                    MessageBox.Show($"Запрос названий выполнен успешно, получено {dt.Rows.Count} строк", "Диагностика БД");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in LoadSubjectNames: {ex.Message}", "Ошибка БД");
                }
            }
            return dt;
        }

        public DataTable LoadSubjectsWithAreaName()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        уп.Код_учебного_предмета,
                        уп.Название_учебного_предмета,
                        уп.Объем_в_часах,
                        уп.Код_предметной_области,
                        по.Название_предметной_области
                    FROM Учебный_предмет уп
                    LEFT JOIN Предметная_область по ON уп.Код_предметной_области = по.Код_предметной_области
                    ORDER BY уп.Название_учебного_предмета";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadSubjectAreasForComboBox()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT Код_предметной_области, Название_предметной_области FROM Предметная_область ORDER BY Название_предметной_области";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    connection.Open();
                    adapter.Fill(dt);
                    connection.Close();

                    MessageBox.Show($"Запрос областей выполнен успешно, получено {dt.Rows.Count} строк", "Диагностика БД");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in LoadSubjectAreasForComboBox: {ex.Message}", "Ошибка БД");
                }
            }
            return dt;
        }

        public void AddSubject(int subjectNameId, int hours, int subjectAreaCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Учебный_предмет 
                    (Код_учебного_предмета, Объем_в_часах, Код_предметной_области) 
                VALUES 
                    (@SubjectNameId, @Hours, @SubjectAreaCode)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubjectNameId", subjectNameId);
                cmd.Parameters.AddWithValue("@Hours", hours);
                cmd.Parameters.AddWithValue("@SubjectAreaCode", subjectAreaCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteSubject(int subjectId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Учебный_предмет WHERE Код_учебного_предмета = @SubjectId";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubjectId", subjectId);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateSubject(int subjectId, int subjectNameId, int hours, int subjectAreaCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Учебный_предмет 
                SET 
                    Код_учебного_предмета = @SubjectNameId,
                    Объем_в_часах = @Hours,
                    Код_предметной_области = @SubjectAreaCode
                WHERE 
                    Код_учебного_предмета = @SubjectId";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubjectNameId", subjectNameId);
                cmd.Parameters.AddWithValue("@Hours", hours);
                cmd.Parameters.AddWithValue("@SubjectAreaCode", subjectAreaCode);
                cmd.Parameters.AddWithValue("@SubjectId", subjectId);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsSubjectExists(int subjectNameId, int subjectAreaCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Учебный_предмет WHERE Код_учебного_предмета = @SubjectNameId AND Код_предметной_области = @SubjectAreaCode";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubjectNameId", subjectNameId);
                cmd.Parameters.AddWithValue("@SubjectAreaCode", subjectAreaCode);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}