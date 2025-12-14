using FastReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using Pasionat;
using FastReport.Preview;
namespace Pasionat
{
    public partial class GuardianForm : Form
    {
        public GuardianForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperG();
            LoadGuardiansData();
            ClearFields();
        }
        private DatabaseHelperG dbHelper;

        private void LoadGuardiansData()
        {
            try
            {
                DataTable guardians = dbHelper.LoadGuardians();
                dataGridViewGuardians.DataSource = guardians;
                dataGridViewGuardians.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки опекунов: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            txtFullName.Text = "";
            txtContactInfo.Text = "";
            txtAddress.Text = "";
            cmbGender.SelectedIndex = -1;
            dateTimePickerBirthDate.Value = DateTime.Now.AddYears(-30); // Устанавливаем возраст по умолчанию 30 лет
        }

/*        private void InitializeComboBoxes()
        {
            cmbGender.Items.Clear();
            cmbGender.Items.Add("М");
            cmbGender.Items.Add("Ж");
        }*/
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewGuardians.SelectedRows.Count == 0 ||
                    dataGridViewGuardians.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите опекуна для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewGuardians.SelectedRows[0];
                string selectedFullName = selectedRow.Cells["ФИО_опекуна"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Удалить опекуна '{selectedFullName}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteGuardian(selectedFullName);
                LoadGuardiansData();
                ClearFields();
                MessageBox.Show("Опекун удален!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить опекуна: существуют связанные записи с воспитанниками");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
           
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО опекуна!");
                return false;
            }

            if (cmbGender.SelectedItem == null)
            {
                MessageBox.Show("Выберите пол опекуна!");
                return false;
            }

            // Проверка возраста (опекуну должно быть больше 18 лет)
            if (dateTimePickerBirthDate.Checked && dateTimePickerBirthDate.Value > DateTime.Now.AddYears(-18))
            {
                MessageBox.Show("Опекун должен быть старше 18 лет!");
                return false;
            }

            return true;
        }

        private void buttonReport_Click(object sender, EventArgs e)
        {

        }

        private float GetColumnPosition(float[] columnWidths, int index)
        {
            float position = 0;
            for (int i = 0; i < index; i++)
            {
                position += columnWidths[i];
            }
            return position;
        }

        private void buttonAdd_Click_1(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Проверяем, существует ли уже опекун с таким ФИО
                if (dbHelper.IsGuardianExists(txtFullName.Text))
                {
                    MessageBox.Show("Опекун с таким ФИО уже существует!");
                    return;
                }

                dbHelper.AddGuardian(
                    fullName: txtFullName.Text,
                    contactInfo: txtContactInfo.Text,
                    address: txtAddress.Text,
                    gender: cmbGender.SelectedItem?.ToString(),
                    birthDate: dateTimePickerBirthDate.Checked ? dateTimePickerBirthDate.Value : (DateTime?)null
                );

                LoadGuardiansData();
                ClearFields();
                MessageBox.Show("Опекун успешно добавлен!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Опекун с таким ФИО уже существует");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }


        private void buttonEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewGuardians.SelectedRows.Count == 0 ||
                    dataGridViewGuardians.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите опекуна для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewGuardians.SelectedRows[0];

                txtFullName.Text = selectedRow.Cells["ФИО_опекуна"].Value.ToString();
                txtContactInfo.Text = selectedRow.Cells["Контактная_информация"].Value?.ToString() ?? "";
                txtAddress.Text = selectedRow.Cells["Адрес"].Value?.ToString() ?? "";

                // Устанавливаем пол
                string gender = selectedRow.Cells["Пол"].Value?.ToString() ?? "";
                cmbGender.SelectedItem = gender;

                // Устанавливаем дату рождения
                if (selectedRow.Cells["Дата_рождения"].Value != DBNull.Value)
                {
                    dateTimePickerBirthDate.Value = Convert.ToDateTime(selectedRow.Cells["Дата_рождения"].Value);
                    dateTimePickerBirthDate.Checked = true;
                }
                else
                {
                    dateTimePickerBirthDate.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных опекуна: " + ex.Message);
            }
        }

        private void buttonSave_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewGuardians.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите опекуна для сохранения!");
                    return;
                }

                if (!ValidateInput())
                    return;

                DataGridViewRow selectedRow = dataGridViewGuardians.SelectedRows[0];
                string originalFullName = selectedRow.Cells["ФИО_опекуна"].Value.ToString();
                string newFullName = txtFullName.Text;

                if (newFullName != originalFullName)
                {
                    if (dbHelper.IsGuardianExists(newFullName))
                    {
                        MessageBox.Show("Ошибка: Опекун с таким ФИО уже существует");
                        return;
                    }
                }

                dbHelper.UpdateGuardian(
                    originalFullName,
                    newFullName,
                    txtContactInfo.Text,
                    txtAddress.Text,
                    cmbGender.SelectedItem?.ToString(),
                    dateTimePickerBirthDate.Checked ? dateTimePickerBirthDate.Value : (DateTime?)null
                );

                LoadGuardiansData();
                ClearFields();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void buttonReport_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Получаем данные
                DataTable guardians = dbHelper.LoadGuardians();
                int totalCount = guardians.Rows.Count;

                if (guardians.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчета");
                    return;
                }

                using (Report report = new Report())
                {
                    string reportPath = Path.Combine(Application.StartupPath, @"Reports\GuardiansReport.frx");

                    if (File.Exists(reportPath))
                    {
                        // Используем шаблон, если он существует
                        report.Load(reportPath);

                        // Регистрируем данные
                        report.Dictionary.Clear();
                        report.Dictionary.RegisterData(guardians, "Опекуны", true);

                        // Принудительно активируем все источники данных
                        foreach (FastReport.Data.DataSourceBase dataSource in report.Dictionary.DataSources)
                        {
                            dataSource.Enabled = true;
                        }

                        // Устанавливаем параметры
                        report.SetParameterValue("TotalCount", totalCount);
                        report.SetParameterValue("ReportDate", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    }
                    

                    report.Prepare();
                    report.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта по опекунам: {ex.Message}");
            }
        }
    }
    public class DatabaseHelperG
{    
    String connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;";
    public DataTable LoadGuardians()
    {
        DataTable dt = new DataTable();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Опекун ORDER BY ФИО_опекуна";
            SqlCommand cmd = new SqlCommand(query, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        return dt;
    }

    public void AddGuardian(string fullName, string contactInfo, string address, string gender, DateTime? birthDate)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = @"
                INSERT INTO Опекун 
                    (ФИО_опекуна, Контактная_информация, Адрес, Пол, Дата_рождения) 
                VALUES 
                    (@FullName, @ContactInfo, @Address, @Gender, @BirthDate)";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@FullName", fullName);
            cmd.Parameters.AddWithValue("@ContactInfo", string.IsNullOrEmpty(contactInfo) ? (object)DBNull.Value : contactInfo);
            cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
            cmd.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(gender) ? (object)DBNull.Value : gender);
            cmd.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);

            connection.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteGuardian(string fullName)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "DELETE FROM Опекун WHERE ФИО_опекуна = @FullName";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@FullName", fullName);

            connection.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateGuardian(string currentFullName, string newFullName, string contactInfo, string address, string gender, DateTime? birthDate)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = @"
                UPDATE Опекун 
                SET 
                    ФИО_опекуна = @NewFullName,
                    Контактная_информация = @ContactInfo,
                    Адрес = @Address,
                    Пол = @Gender,
                    Дата_рождения = @BirthDate
                WHERE 
                    ФИО_опекуна = @CurrentFullName";

            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@NewFullName", newFullName);
            cmd.Parameters.AddWithValue("@ContactInfo", string.IsNullOrEmpty(contactInfo) ? (object)DBNull.Value : contactInfo);
            cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
            cmd.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(gender) ? (object)DBNull.Value : gender);
            cmd.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@CurrentFullName", currentFullName);

            connection.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public bool IsGuardianExists(string fullName)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT COUNT(*) FROM Опекун WHERE ФИО_опекуна = @FullName";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@FullName", fullName);

            connection.Open();
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}
}
