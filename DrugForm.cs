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

namespace Pasionat
{
    public partial class DrugForm : Form
    {
        private DatabaseHelperDrug dbHelper;
        public DrugForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperDrug();
            LoadMedicinesData();
            ClearFields();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMedicineName.Text))
            {
                MessageBox.Show("Введите название препарата!");
                return;
            }

            try
            {
                // Проверяем, существует ли уже препарат с таким названием
                if (dbHelper.IsMedicineNameExists(txtMedicineName.Text))
                {
                    MessageBox.Show("Препарат с таким названием уже существует!");
                    return;
                }

                dbHelper.AddMedicine(
                    medicineName: txtMedicineName.Text,
                    dosage: txtDosage.Text
                );

                LoadMedicinesData();
                ClearFields();
                MessageBox.Show("Препарат успешно добавлен!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Препарат с таким названием уже существует");
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
                if (dataGridViewMedicines.SelectedRows.Count == 0 ||
                    dataGridViewMedicines.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите препарат для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewMedicines.SelectedRows[0];
                string selectedMedicineName = selectedRow.Cells["Название_препарата"].Value.ToString();
                string dosage = selectedRow.Cells["Дозировка_в_мг"].Value?.ToString() ?? "";

                DialogResult result = MessageBox.Show(
                    $"Удалить препарат '{selectedMedicineName}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteMedicine(selectedMedicineName);
                LoadMedicinesData();
                ClearFields();
                MessageBox.Show("Препарат удален!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить препарат: существуют связанные записи в назначениях");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
        private void buttonReport_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем данные ДО создания отчета
                DataTable medications = dbHelper.LoadMedicines();
                int totalCount = medications.Rows.Count;

                if (medications.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для формирования отчета!");
                    return;
                }

                using (Report report = new Report())
                {
                    string reportPath = Path.Combine(Application.StartupPath, @"Reports\DrugReport.frx");

                    if (!File.Exists(reportPath))
                    {
                        MessageBox.Show("Файл шаблона отчёта по препаратам не найден!");
                        return;
                    }

                    report.Load(reportPath);

                    // Регистрируем данные
                    report.Dictionary.Clear();
                    report.Dictionary.RegisterData(medications, "Препараты", true);

                    // Принудительно активируем все источники данных
                    foreach (FastReport.Data.DataSourceBase dataSource in report.Dictionary.DataSources)
                    {
                        dataSource.Enabled = true;
                    }

                    // Устанавливаем параметры
                    report.SetParameterValue("TotalCount", totalCount);
                    report.SetParameterValue("ReportDate", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

                    // Проверяем доступность данных
                    if (report.Dictionary.DataSources.Count == 0)
                    {
                        MessageBox.Show("Источники данных не зарегистрированы в отчете");
                        return;
                    }

                    report.Prepare();
                    report.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}");
            }
        }
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewMedicines.SelectedRows.Count == 0 ||
                    dataGridViewMedicines.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите препарат для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewMedicines.SelectedRows[0];

                txtMedicineName.Text = selectedRow.Cells["Название_препарата"].Value.ToString();
                txtDosage.Text = selectedRow.Cells["Дозировка_в_мг"].Value?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных препарата: " + ex.Message);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewMedicines.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите препарат для сохранения!");
                    return;
                }

                if (string.IsNullOrEmpty(txtMedicineName.Text))
                {
                    MessageBox.Show("Введите название препарата!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewMedicines.SelectedRows[0];
                string originalMedicineName = selectedRow.Cells["Название_препарата"].Value.ToString();
                string newMedicineName = txtMedicineName.Text;

                if (newMedicineName != originalMedicineName)
                {
                    if (dbHelper.IsMedicineNameExists(newMedicineName))
                    {
                        MessageBox.Show("Ошибка: Препарат с таким названием уже существует");
                        return;
                    }
                }

                dbHelper.UpdateMedicine(
                    originalMedicineName,
                    newMedicineName,
                    txtDosage.Text
                );

                LoadMedicinesData();
                ClearFields();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }




        private void LoadMedicinesData()
        {
            try
            {
                DataTable medicines = dbHelper.LoadMedicines();
                dataGridViewMedicines.DataSource = medicines;
                dataGridViewMedicines.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки препаратов: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            txtMedicineName.Text = "";
            txtDosage.Text = "";
        }


        
    }
    public class DatabaseHelperDrug
    {
        String connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;";

        public DataTable LoadMedicines()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Препарат ORDER BY Название_препарата";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        // Удалите этот метод или сделайте его идентичным LoadMedicines()
        // public DataTable LoadMedications() ...

        public void AddMedicine(string medicineName, string dosage)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO Препарат 
                (Название_препарата, Дозировка_в_мг) 
            VALUES 
                (@MedicineName, @Dosage)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MedicineName", medicineName);
                cmd.Parameters.AddWithValue("@Dosage", string.IsNullOrEmpty(dosage) ? (object)DBNull.Value : dosage);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteMedicine(string medicineName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Препарат WHERE Название_препарата = @MedicineName";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MedicineName", medicineName);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateMedicine(string currentMedicineName, string newMedicineName, string dosage)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            UPDATE Препарат 
            SET 
                Название_препарата = @NewMedicineName,
                Дозировка_в_мг = @Dosage
            WHERE 
                Название_препарата = @CurrentMedicineName";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewMedicineName", newMedicineName);
                cmd.Parameters.AddWithValue("@Dosage", string.IsNullOrEmpty(dosage) ? (object)DBNull.Value : dosage);
                cmd.Parameters.AddWithValue("@CurrentMedicineName", currentMedicineName);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsMedicineNameExists(string medicineName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Препарат WHERE Название_препарата = @MedicineName";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MedicineName", medicineName);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}
