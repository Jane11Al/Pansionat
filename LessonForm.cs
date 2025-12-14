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
    public partial class LessonForm : Form
    {
        public LessonForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperL();
            CheckDatabaseConnection();

            LoadSubjectsData();
            LoadSubjectAreasComboBox();
            ClearFields();
        }

        private DatabaseHelperL dbHelper;
        private string currentSubjectName = ""; // Для хранения названия редактируемой записи

        private void CheckDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbHelper.connectionString))
                {
                    connection.Open();
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
                DataTable subjects = dbHelper.LoadSubjects();
                dataGridViewSubjects.DataSource = null;
                dataGridViewSubjects.DataSource = subjects;
                dataGridViewSubjects.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки учебных предметов: {ex.Message}", "Ошибка");
            }
        }

        private void LoadSubjectAreasComboBox()
        {
            try
            {
                DataTable subjectAreas = dbHelper.LoadSubjectAreasForComboBox();
                comboBoxSubjectArea.DataSource = subjectAreas;
                comboBoxSubjectArea.DisplayMember = "Название_предметной_области";
                comboBoxSubjectArea.ValueMember = "Код_предметной_области";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметных областей: {ex.Message}", "Ошибка");
            }
        }

        private void ClearFields()
        {
            txtSubjectName.Text = "";
            txtHours.Text = "";
            if (comboBoxSubjectArea.Items.Count > 0)
                comboBoxSubjectArea.SelectedIndex = 0;
            currentSubjectName = "";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSubjectName.Text))
            {
                MessageBox.Show("Введите название учебного предмета!");
                return;
            }

            if (comboBoxSubjectArea.SelectedValue == null)
            {
                MessageBox.Show("Выберите предметную область!");
                return;
            }

            try
            {
                if (!int.TryParse(txtHours.Text, out int hours))
                {
                    MessageBox.Show("Объем в часах должен быть числом!");
                    return;
                }

                string subjectName = txtSubjectName.Text;
                int subjectAreaCode = (int)comboBoxSubjectArea.SelectedValue;

                if (dbHelper.IsSubjectExists(subjectName))
                {
                    MessageBox.Show("Учебный предмет с таким названием уже существует!");
                    return;
                }

                dbHelper.AddSubject(
                    subjectName: subjectName,
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
                string subjectName = selectedRow.Cells["Название_учебного_предмета"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Удалить учебный предмет '{subjectName}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteSubject(subjectName);
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

                currentSubjectName = selectedRow.Cells["Название_учебного_предмета"].Value.ToString();
                string hours = selectedRow.Cells["Объем_в_часах"].Value.ToString();
                int subjectAreaCode = Convert.ToInt32(selectedRow.Cells["Код_предметной_области"].Value);

                txtSubjectName.Text = currentSubjectName;
                txtHours.Text = hours;
                comboBoxSubjectArea.SelectedValue = subjectAreaCode;
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
                if (string.IsNullOrEmpty(currentSubjectName))
                {
                    MessageBox.Show("Выберите учебный предмет для сохранения!");
                    return;
                }

                if (string.IsNullOrEmpty(txtSubjectName.Text))
                {
                    MessageBox.Show("Введите название учебного предмета!");
                    return;
                }

                if (comboBoxSubjectArea.SelectedValue == null)
                {
                    MessageBox.Show("Выберите предметную область!");
                    return;
                }

                if (!int.TryParse(txtHours.Text, out int hours))
                {
                    MessageBox.Show("Объем в часах должен быть числом!");
                    return;
                }

                string newSubjectName = txtSubjectName.Text;
                int subjectAreaCode = (int)comboBoxSubjectArea.SelectedValue;

                // Если название изменилось, проверяем не существует ли уже предмет с новым названием
                if (newSubjectName != currentSubjectName && dbHelper.IsSubjectExists(newSubjectName))
                {
                    MessageBox.Show("Учебный предмет с таким названием уже существует!");
                    return;
                }

                dbHelper.UpdateSubject(
                    currentSubjectName,
                    newSubjectName,
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
    }

    public class DatabaseHelperL
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
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка загрузки предметов: {ex.Message}");
                }
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
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка загрузки предметных областей: {ex.Message}");
                }
            }
            return dt;
        }

        public void AddSubject(string subjectName, int hours, int subjectAreaCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Учебный_предмет 
                    (Название_учебного_предмета, Объем_в_часах, Код_предметной_области) 
                VALUES 
                    (@SubjectName, @Hours, @SubjectAreaCode)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubjectName", subjectName);
                cmd.Parameters.AddWithValue("@Hours", hours);
                cmd.Parameters.AddWithValue("@SubjectAreaCode", subjectAreaCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteSubject(string subjectName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Учебный_предмет WHERE Название_учебного_предмета = @SubjectName";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubjectName", subjectName);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateSubject(string oldSubjectName, string newSubjectName, int hours, int subjectAreaCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Учебный_предмет 
                SET 
                    Название_учебного_предмета = @NewSubjectName,
                    Объем_в_часах = @Hours,
                    Код_предметной_области = @SubjectAreaCode
                WHERE 
                    Название_учебного_предмета = @OldSubjectName";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewSubjectName", newSubjectName);
                cmd.Parameters.AddWithValue("@Hours", hours);
                cmd.Parameters.AddWithValue("@SubjectAreaCode", subjectAreaCode);
                cmd.Parameters.AddWithValue("@OldSubjectName", oldSubjectName);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsSubjectExists(string subjectName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Учебный_предмет WHERE Название_учебного_предмета = @SubjectName";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubjectName", subjectName);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}