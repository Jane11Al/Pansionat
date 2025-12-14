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
using System.Configuration;

namespace Pansionat
{
    public partial class TeacherForm : Form
    {
        private TeacherDatabaseHelper dbHelper;
        private bool isEditing = false;
        private string currentTeacherFIO = "";

        public TeacherForm()
        {
            InitializeComponent();
            dbHelper = new TeacherDatabaseHelper();
        }

        private void TeacherForm_Load(object sender, EventArgs e)
        {
            LoadTeachersData();
        }

        private void LoadTeachersData()
        {
            try
            {
                DataTable teachers = dbHelper.LoadTeachers();
                dataGridViewTeachers.DataSource = teachers;
                dataGridViewTeachers.AutoResizeColumns();
                dataGridViewTeachers.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxFIO.Text))
            {
                MessageBox.Show("Введите ФИО педагога!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPosition.Text))
            {
                MessageBox.Show("Введите должность!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxContact.Text))
            {
                MessageBox.Show("Введите контактную информацию!");
                return;
            }

            if (dateTimePickerBirthDate.Value > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем!");
                return;
            }

            try
            {
                dbHelper.AddTeacher(
                    fio: textBoxFIO.Text,
                    birthDate: dateTimePickerBirthDate.Value,
                    contactInfo: textBoxContact.Text,
                    position: textBoxPosition.Text
                );

                LoadTeachersData();
                ClearForm();
                MessageBox.Show("Педагог успешно добавлен!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Педагог с таким ФИО уже существует");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewTeachers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите педагога для редактирования!");
                return;
            }

            DataGridViewRow selectedRow = dataGridViewTeachers.SelectedRows[0];

            textBoxFIO.Text = selectedRow.Cells["ФИО_педагога"].Value.ToString();
            textBoxContact.Text = selectedRow.Cells["Контактная_информация"].Value?.ToString() ?? "";
            textBoxPosition.Text = selectedRow.Cells["Должность"].Value?.ToString() ?? "";

            if (selectedRow.Cells["Дата_рождения"].Value != DBNull.Value)
            {
                dateTimePickerBirthDate.Value = Convert.ToDateTime(selectedRow.Cells["Дата_рождения"].Value);
            }

            currentTeacherFIO = textBoxFIO.Text;
            isEditing = true;

            // Блокируем поле ФИО при редактировании, так как оно является ключом
            textBoxFIO.Enabled = false;

            buttonAdd.Enabled = false;
            buttonSave.Enabled = true;
            buttonDelete.Enabled = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!isEditing)
            {
                MessageBox.Show("Сначала выберите педагога для редактирования!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxPosition.Text))
            {
                MessageBox.Show("Введите должность!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxContact.Text))
            {
                MessageBox.Show("Введите контактную информацию!");
                return;
            }

            try
            {
                dbHelper.UpdateTeacher(
                    originalFIO: currentTeacherFIO,
                    newFIO: textBoxFIO.Text,
                    birthDate: dateTimePickerBirthDate.Value,
                    contactInfo: textBoxContact.Text,
                    position: textBoxPosition.Text
                );

                LoadTeachersData();
                ClearForm();
                ResetFormState();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewTeachers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите педагога для удаления!");
                return;
            }

            DataGridViewRow selectedRow = dataGridViewTeachers.SelectedRows[0];
            string teacherFIO = selectedRow.Cells["ФИО_педагога"].Value.ToString();

            DialogResult result = MessageBox.Show(
                $"Удалить педагога: {teacherFIO}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    dbHelper.DeleteTeacher(teacherFIO);
                    LoadTeachersData();
                    MessageBox.Show("Педагог удален!");
                }
                catch (SqlException ex) when (ex.Number == 547)
                {
                    MessageBox.Show("Нельзя удалить педагога: существуют связанные записи в обучении");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
        }

        private void ClearForm()
        {
            textBoxFIO.Clear();
            textBoxContact.Clear();
            textBoxPosition.Clear();
            dateTimePickerBirthDate.Value = DateTime.Now.AddYears(-30);
        }

        private void ResetFormState()
        {
            isEditing = false;
            currentTeacherFIO = "";
            textBoxFIO.Enabled = true;
            buttonAdd.Enabled = true;
            buttonSave.Enabled = false;
            buttonDelete.Enabled = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            ResetFormState();
        }

        // Остальные методы для событий UI
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e) { }
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void button8_Click(object sender, EventArgs e) { }
    }

    public class TeacherDatabaseHelper
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PansionatDBConnection"].ConnectionString;

        public DataTable LoadTeachers()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ФИО_педагога, Дата_рождения, Контактная_информация, Должность FROM Педагог";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void AddTeacher(string fio, DateTime birthDate, string contactInfo, string position)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Педагог 
                    (ФИО_педагога, Дата_рождения, Контактная_информация, Должность) 
                VALUES 
                    (@FIO, @BirthDate, @ContactInfo, @Position)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@FIO", fio);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@ContactInfo", contactInfo);
                cmd.Parameters.AddWithValue("@Position", position);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateTeacher(string originalFIO, string newFIO, DateTime birthDate, string contactInfo, string position)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Педагог 
                SET 
                    ФИО_педагога = @NewFIO,
                    Дата_рождения = @BirthDate,
                    Контактная_информация = @ContactInfo,
                    Должность = @Position
                WHERE 
                    ФИО_педагога = @OriginalFIO";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewFIO", newFIO);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@ContactInfo", contactInfo);
                cmd.Parameters.AddWithValue("@Position", position);
                cmd.Parameters.AddWithValue("@OriginalFIO", originalFIO);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTeacher(string fio)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Педагог WHERE ФИО_педагога = @FIO";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@FIO", fio);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsTeacherExists(string fio)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Педагог WHERE ФИО_педагога = @FIO";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@FIO", fio);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}