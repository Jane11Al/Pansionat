using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text; // Важно добавить!
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Pasionat
{
    public partial class SubjAreaForm : Form
    {
        public SubjAreaForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperSubjectArea();
            LoadSubjectAreasData();
            ClearFields();
        }

        private DatabaseHelperSubjectArea dbHelper;

        private void LoadSubjectAreasData()
        {
            try
            {
                DataTable subjectAreas = dbHelper.LoadSubjectAreas();
                dataGridViewSubjectAreas.DataSource = subjectAreas;
                dataGridViewSubjectAreas.Refresh();

                // Убедимся, что DataGridView настроен правильно
                dataGridViewSubjectAreas.AutoGenerateColumns = true;
                dataGridViewSubjectAreas.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки предметных областей: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            textBoxAreaCode.Clear();
            textBoxAreaName.Clear();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxAreaName.Text))
            {
                MessageBox.Show("Введите название предметной области!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxAreaCode.Text))
            {
                MessageBox.Show("Введите код предметной области!");
                return;
            }

            try
            {
                if (!int.TryParse(textBoxAreaCode.Text, out int areaCode))
                {
                    MessageBox.Show("Код предметной области должен быть числом!");
                    return;
                }

                // Проверка существования кода перед добавлением
                if (dbHelper.IsSubjectAreaExists(areaCode))
                {
                    MessageBox.Show("Предметная область с таким кодом уже существует!");
                    return;
                }

                dbHelper.AddSubjectArea(
                    areaCode: areaCode,
                    areaName: textBoxAreaName.Text
                );

                LoadSubjectAreasData();
                ClearFields();
                MessageBox.Show("Предметная область успешно добавлена!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Предметная область с таким кодом уже существует");
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
                if (dataGridViewSubjectAreas.SelectedRows.Count == 0 ||
                    dataGridViewSubjectAreas.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите предметную область для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewSubjectAreas.SelectedRows[0];

                // Проверка на null значений
                if (selectedRow.Cells["Код_предметной_области"].Value == null ||
                    selectedRow.Cells["Название_предметной_области"].Value == null)
                {
                    MessageBox.Show("Ошибка: Не удалось получить данные выбранной записи!");
                    return;
                }

                int selectedAreaCode = Convert.ToInt32(selectedRow.Cells["Код_предметной_области"].Value);
                string areaName = selectedRow.Cells["Название_предметной_области"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Удалить предметную область '{areaName}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteSubjectArea(selectedAreaCode);
                LoadSubjectAreasData();
                ClearFields();
                MessageBox.Show("Предметная область удалена!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить предметную область: существуют связанные записи в учебных предметах");
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
                if (dataGridViewSubjectAreas.SelectedRows.Count == 0 ||
                    dataGridViewSubjectAreas.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите предметную область для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewSubjectAreas.SelectedRows[0];

                // Проверка на null значений
                if (selectedRow.Cells["Код_предметной_области"].Value == null ||
                    selectedRow.Cells["Название_предметной_области"].Value == null)
                {
                    MessageBox.Show("Ошибка: Не удалось получить данные выбранной записи!");
                    return;
                }

                textBoxAreaCode.Text = selectedRow.Cells["Код_предметной_области"].Value.ToString();
                textBoxAreaName.Text = selectedRow.Cells["Название_предметной_области"].Value.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных предметной области: " + ex.Message);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSubjectAreas.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите предметную область для сохранения!");
                    return;
                }

                if (string.IsNullOrEmpty(textBoxAreaName.Text))
                {
                    MessageBox.Show("Введите название предметной области!");
                    return;
                }

                if (string.IsNullOrEmpty(textBoxAreaCode.Text))
                {
                    MessageBox.Show("Введите код предметной области!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewSubjectAreas.SelectedRows[0];
                int originalAreaCode = Convert.ToInt32(selectedRow.Cells["Код_предметной_области"].Value);

                if (!int.TryParse(textBoxAreaCode.Text, out int newAreaCode))
                {
                    MessageBox.Show("Код предметной области должен быть числом!");
                    return;
                }

                // Проверка: если код изменился, проверяем не существует ли уже такого кода
                if (newAreaCode != originalAreaCode)
                {
                    if (dbHelper.IsSubjectAreaExists(newAreaCode))
                    {
                        MessageBox.Show("Ошибка: Предметная область с таким кодом уже существует");
                        return;
                    }
                }

                dbHelper.UpdateSubjectArea(
                    originalAreaCode: originalAreaCode,
                    newAreaCode: newAreaCode,
                    areaName: textBoxAreaName.Text
                );

                LoadSubjectAreasData();
                ClearFields();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Предметная область с таким кодом уже существует");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void buttonReport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable subjectAreas = dbHelper.LoadSubjectAreas();

                if (subjectAreas.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчета");
                    return;
                }

                StringBuilder report = new StringBuilder();
                report.AppendLine("ПРЕДМЕТНЫЕ ОБЛАСТИ");
                report.AppendLine("==================");
                report.AppendLine($"Всего предметных областей: {subjectAreas.Rows.Count}");
                report.AppendLine();

                foreach (DataRow row in subjectAreas.Rows)
                {
                    report.AppendLine($"{row["Код_предметной_области"]} - {row["Название_предметной_области"]}");
                }

                MessageBox.Show(report.ToString(), "Отчет по предметным областям");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}");
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            LoadSubjectAreasData();
            ClearFields();
        }
    }

    public class DatabaseHelperSubjectArea
    {
        string connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;";

        public DataTable LoadSubjectAreas()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        Код_предметной_области,
                        Название_предметной_области
                    FROM Предметная_область 
                    ORDER BY Код_предметной_области";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void AddSubjectArea(int areaCode, string areaName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Предметная_область 
                    (Код_предметной_области, Название_предметной_области) 
                VALUES 
                    (@AreaCode, @AreaName)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@AreaCode", areaCode);
                cmd.Parameters.AddWithValue("@AreaName", areaName);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteSubjectArea(int areaCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Предметная_область WHERE Код_предметной_области = @AreaCode";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@AreaCode", areaCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateSubjectArea(int originalAreaCode, int newAreaCode, string areaName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Предметная_область 
                SET 
                    Код_предметной_области = @NewAreaCode,
                    Название_предметной_области = @AreaName
                WHERE 
                    Код_предметной_области = @OriginalAreaCode";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewAreaCode", newAreaCode);
                cmd.Parameters.AddWithValue("@AreaName", areaName);
                cmd.Parameters.AddWithValue("@OriginalAreaCode", originalAreaCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsSubjectAreaExists(int areaCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Предметная_область WHERE Код_предметной_области = @AreaCode";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@AreaCode", areaCode);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}