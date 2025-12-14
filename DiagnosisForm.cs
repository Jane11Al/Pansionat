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
using FastReport;
using FastReport.Data;
using Pasionat;
using FastReport.Preview;

namespace Pasionat
{
    public partial class DiagnosisForm : Form
    {
        private DatabaseHelperDiagnoses dbHelper;

        public DiagnosisForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperDiagnoses();
            LoadDiagnosesData();
            ClearFields();
        }

        private void LoadDiagnosesData()
        {
            try
            {
                DataTable diagnoses = dbHelper.LoadDiagnoses();
                dataGridViewDiagnoses.DataSource = diagnoses;
                dataGridViewDiagnoses.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки диагнозов: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            txtDiseaseCode.Text = "";
            txtDiseaseName.Text = "";
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDiseaseCode.Text))
            {
                MessageBox.Show("Введите код диагноза!");
                return;
            }

            if (string.IsNullOrEmpty(txtDiseaseName.Text))
            {
                MessageBox.Show("Введите название диагноза!");
                return;
            }

            try
            {
                dbHelper.AddDiagnosis(
                    diseaseCode: txtDiseaseCode.Text,
                    name: txtDiseaseName.Text
                );

                LoadDiagnosesData();
                ClearFields();
                MessageBox.Show("Диагноз успешно добавлен!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Диагноз с таким кодом уже существует");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnDeleteDiagnosis_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewDiagnoses.SelectedRows.Count == 0 ||
                    dataGridViewDiagnoses.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите диагноз для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewDiagnoses.SelectedRows[0];
                string selectedDiseaseCode = selectedRow.Cells["Код_заболевания"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Удалить диагноз '{selectedRow.Cells["Название"].Value}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteDiagnosis(selectedDiseaseCode);
                LoadDiagnosesData();
                ClearFields();
                MessageBox.Show("Диагноз удален!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить диагноз: существуют связанные записи с воспитанниками");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void btnDiagnosisReport_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataTable diagnoses = dbHelper.LoadDiagnoses();
                int totalCount = diagnoses.Rows.Count;

                if (diagnoses.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчета");
                    return;
                }

                using (Report report = new Report())
                {
                    string reportPath = Path.Combine(Application.StartupPath, @"Reports\DiagnosesReport.frx");

                    if (!File.Exists(reportPath))
                    {
                        MessageBox.Show("Файл шаблона отчёта по диагнозам не найден!");
                        return;
                    }

                    report.Load(reportPath);

                    report.Dictionary.Clear();
                    report.Dictionary.RegisterData(diagnoses, "Диагноз", true);

                    foreach (FastReport.Data.DataSourceBase dataSource in report.Dictionary.DataSources)
                    {
                        dataSource.Enabled = true;
                    }

                    report.SetParameterValue("TotalCount", totalCount);
                    report.SetParameterValue("ReportDate", DateTime.Now.ToString("dd.MM.yyyy"));

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

        private void btnSaveDiagnosis_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewDiagnoses.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите диагноз для сохранения!");
                    return;
                }

                if (string.IsNullOrEmpty(txtDiseaseCode.Text))
                {
                    MessageBox.Show("Введите код диагноза!");
                    return;
                }

                if (string.IsNullOrEmpty(txtDiseaseName.Text))
                {
                    MessageBox.Show("Введите название диагноза!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewDiagnoses.SelectedRows[0];
                string originalDiseaseCode = selectedRow.Cells["Код_заболевания"].Value.ToString();
                string newDiseaseCode = txtDiseaseCode.Text;

                if (newDiseaseCode != originalDiseaseCode)
                {
                    if (dbHelper.IsDiagnosisCodeExists(newDiseaseCode))
                    {
                        MessageBox.Show("Ошибка: Диагноз с таким кодом уже существует");
                        return;
                    }
                }

                dbHelper.UpdateDiagnosis(
                    originalDiseaseCode,
                    txtDiseaseName.Text,
                    newDiseaseCode
                );

                LoadDiagnosesData();
                ClearFields();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void btnEditDiagnosis_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewDiagnoses.SelectedRows.Count == 0 ||
                    dataGridViewDiagnoses.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите диагноз для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewDiagnoses.SelectedRows[0];

                txtDiseaseCode.Text = selectedRow.Cells["Код_заболевания"].Value.ToString();
                txtDiseaseName.Text = selectedRow.Cells["Название"].Value.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных диагноза: " + ex.Message);
            }
        }
    }

    public class DatabaseHelperDiagnoses
    {
        String connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;";

        // Методы для работы с диагнозами
        public DataTable LoadDiagnoses()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Диагноз ORDER BY Код_заболевания";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void AddDiagnosis(string diseaseCode, string name)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Диагноз 
                    (Код_заболевания, Название) 
                VALUES 
                    (@DiseaseCode, @Name)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DiseaseCode", diseaseCode);
                cmd.Parameters.AddWithValue("@Name", name);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteDiagnosis(string diseaseCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Диагноз WHERE Код_заболевания = @DiseaseCode";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DiseaseCode", diseaseCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateDiagnosis(string currentDiseaseCode, string name, string newDiseaseCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Диагноз 
                SET 
                    Код_заболевания = @NewDiseaseCode,
                    Название = @Name
                WHERE 
                    Код_заболевания = @CurrentDiseaseCode";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewDiseaseCode", newDiseaseCode);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@CurrentDiseaseCode", currentDiseaseCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsDiagnosisCodeExists(string diseaseCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Диагноз WHERE Код_заболевания = @DiseaseCode";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DiseaseCode", diseaseCode);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}