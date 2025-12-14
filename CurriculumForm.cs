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
using Pansionat;

namespace Pasionat
{
    public partial class CurriculumForm : Form
    {
        public CurriculumForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperCur();
            LoadProgramsData();
            ClearFields();
        }
        private DatabaseHelperCur dbHelper;
        private void LoadProgramsData()
        {
            try
            {
                DataTable programs = dbHelper.LoadPrograms();
                dataGridViewPrograms.DataSource = programs;
                dataGridViewPrograms.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки программ: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            txtProgramCode.Text = "";
            txtProgramName.Text = "";

            // Автоматическая генерация следующего кода
            int nextCode = dbHelper.GetMaxProgramCode() + 1;
            txtProgramCode.Text = nextCode.ToString();
        }

        private void buttonAddProgram_Click(object sender, EventArgs e)
        {

            
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

        }


        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void buttonAdd_Click(object sender, EventArgs e)
        {

        }

        private void buttonReport_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем данные
                DataTable programs = dbHelper.LoadPrograms();
                int totalCount = programs.Rows.Count;

                if (programs.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчета");
                    return;
                }

                using (Report report = new Report())
                {
                    string reportPath = Path.Combine(Application.StartupPath, @"Reports\ProgramsReport.frx");

                    if (!File.Exists(reportPath))
                    {
                        // Создаем простой отчет программно, если файл не найден
                        CreateSimpleProgramReport(report, programs, totalCount);
                    }
                    else
                    {
                        report.Load(reportPath);

                        // Регистрируем данные
                        report.RegisterData(programs, "Программы");

                        // Активируем источник данных
                        foreach (FastReport.Data.DataSourceBase dataSource in report.Dictionary.DataSources)
                        {
                            if (dataSource.Name == "Программы")
                            {
                                dataSource.Enabled = true;
                                break;
                            }
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
                MessageBox.Show($"Ошибка формирования отчёта по программам: {ex.Message}");
            }
            
        }
        private void CreateSimpleProgramReport(Report report, DataTable programs, int totalCount)
        {
            // Создаем страницу
            ReportPage page = new ReportPage();
            report.Pages.Add(page);

            // Заголовок отчета
            TextObject title = new TextObject();
            title.Bounds = new System.Drawing.RectangleF(0, 0, page.PaperWidth, 30);
            title.Text = "СПРАВОЧНИК ОБРАЗОВАТЕЛЬНЫХ ПРОГРАММ";
            title.HorzAlign = HorzAlign.Center;
            title.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            page.ReportTitle.Objects.Add(title); // Исправлено: Objects.Add

            // Заголовки столбцов
            TextObject headerCode = new TextObject();
            headerCode.Bounds = new System.Drawing.RectangleF(0, 40, 80, 20);
            headerCode.Text = "Код";
            headerCode.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            headerCode.Border.Lines = BorderLines.All;
            page.ReportTitle.Objects.Add(headerCode); // Исправлено: Objects.Add

            TextObject headerName = new TextObject();
            headerName.Bounds = new System.Drawing.RectangleF(80, 40, page.PaperWidth - 80, 20);
            headerName.Text = "Название программы";
            headerName.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            headerName.Border.Lines = BorderLines.All;
            page.ReportTitle.Objects.Add(headerName); // Исправлено: Objects.Add

            // Данные
            DataBand dataBand = new DataBand();
            dataBand.Bounds = new System.Drawing.RectangleF(0, 0, page.PaperWidth, 20);
            dataBand.Height = 20;

            // Колонка с кодом
            TextObject codeText = new TextObject();
            codeText.Bounds = new System.Drawing.RectangleF(0, 0, 80, 20);
            codeText.Text = "[Программы.Код_программы]";
            codeText.Border.Lines = BorderLines.All;
            dataBand.Objects.Add(codeText); // Исправлено: Objects.Add

            // Колонка с названием
            TextObject nameText = new TextObject();
            nameText.Bounds = new System.Drawing.RectangleF(80, 0, page.PaperWidth - 80, 20);
            nameText.Text = "[Программы.Название]";
            nameText.Border.Lines = BorderLines.All;
            dataBand.Objects.Add(nameText); // Исправлено: Objects.Add

            page.Bands.Add(dataBand);

            // Итоговая информация
            TextObject summary = new TextObject();
            summary.Bounds = new System.Drawing.RectangleF(0, 10, page.PaperWidth, 20);
            summary.Text = $"Всего программ: {totalCount}";
            summary.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            page.ReportSummary.Objects.Add(summary); // Исправлено: Objects.Add

            // Регистрируем данные
            report.RegisterData(programs, "Программы");
        }
        private void buttonAdd_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProgramName.Text))
            {
                MessageBox.Show("Введите название программы!");
                return;
            }

            if (!int.TryParse(txtProgramCode.Text, out int programCode))
            {
                MessageBox.Show("Код программы должен быть целым числом!");
                return;
            }

            try
            {
                dbHelper.AddProgram(
                    programCode: programCode,
                    name: txtProgramName.Text
                );

                LoadProgramsData();
                ClearFields();
                MessageBox.Show("Программа успешно добавлена!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Программа с таким кодом уже существует");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void txtProgramCode_TextChanged(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void buttonReport_Click_1(object sender, EventArgs e)
        {

        }

        private void txtProgramName_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonEdit_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewPrograms.SelectedRows.Count == 0 ||
                    dataGridViewPrograms.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите программу для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewPrograms.SelectedRows[0];

                txtProgramCode.Text = selectedRow.Cells["Код_программы"].Value.ToString();
                txtProgramName.Text = selectedRow.Cells["Название"].Value.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных программы: " + ex.Message);
            }
        }

        private void buttonSave_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewPrograms.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите программу для сохранения!");
                    return;
                }

                if (string.IsNullOrEmpty(txtProgramName.Text))
                {
                    MessageBox.Show("Введите название программы!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewPrograms.SelectedRows[0];
                int originalProgramCode = Convert.ToInt32(selectedRow.Cells["Код_программы"].Value);
                int newProgramCode = Convert.ToInt32(txtProgramCode.Text);

                if (newProgramCode != originalProgramCode)
                {
                    if (dbHelper.IsProgramCodeExists(newProgramCode))
                    {
                        MessageBox.Show("Ошибка: Программа с таким кодом уже существует");
                        return;
                    }
                }

                dbHelper.UpdateProgram(
                    originalProgramCode,
                    txtProgramName.Text,
                    newProgramCode
                );

                LoadProgramsData();
                ClearFields();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void buttonDelete_Click_1(object sender, EventArgs e)
        {

            try
            {
                if (dataGridViewPrograms.SelectedRows.Count == 0 ||
                    dataGridViewPrograms.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите программу для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewPrograms.SelectedRows[0];
                int selectedProgramCode = Convert.ToInt32(selectedRow.Cells["Код_программы"].Value);
                string programName = selectedRow.Cells["Название"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Удалить программу '{programName}'?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteProgram(selectedProgramCode);
                LoadProgramsData();
                ClearFields();
                MessageBox.Show("Программа удалена!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить программу: существуют связанные записи");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }
    }
    public class DatabaseHelperCur
    {
        String connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;";

        public DataTable LoadPrograms()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Программа ORDER BY Код_программы";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void AddProgram(int programCode, string name)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Программа 
                    (Код_программы, Название) 
                VALUES 
                    (@ProgramCode, @Name)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ProgramCode", programCode);
                cmd.Parameters.AddWithValue("@Name", name);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteProgram(int programCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Программа WHERE Код_программы = @ProgramCode";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ProgramCode", programCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateProgram(int currentProgramCode, string name, int newProgramCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Программа 
                SET 
                    Код_программы = @NewProgramCode,
                    Название = @Name
                WHERE 
                    Код_программы = @CurrentProgramCode";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewProgramCode", newProgramCode);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@CurrentProgramCode", currentProgramCode);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsProgramCodeExists(int programCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Программа WHERE Код_программы = @ProgramCode";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ProgramCode", programCode);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public int GetMaxProgramCode()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ISNULL(MAX(Код_программы), 0) FROM Программа";
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
