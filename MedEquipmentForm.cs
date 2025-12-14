using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastReport;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using FastReport;
using FastReport.Data;
using System.IO;
using Pasionat;
using FastReport.Preview;
using System.Xml.Linq;

namespace Pasionat
{
    public partial class MedEquipmentForm : Form
    {
        public MedEquipmentForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelperM();
            LoadMedicalEquipmentData();
        }
        private DatabaseHelperM dbHelper;


        private void LoadMedicalEquipmentData()
        {
            try
            {
                DataTable medicalEquipment = dbHelper.LoadMedicalEquipment();
                dataGridViewMedicalEquipment.DataSource = medicalEquipment;
                dataGridViewMedicalEquipment.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void buttonAdd_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название оборудования!");
                return;
            }
            if (!int.TryParse(txtInventoryNumber.Text, out int inventoryNumber))
            {
                MessageBox.Show("Инвентарный номер должен быть целым числом!");
                return;
            }

            try
            {
                // Проверка существования инвентарного номера
                if (dbHelper.IsMedicalEquipmentNumberExists(inventoryNumber))
                {
                    MessageBox.Show("Оборудование с таким инвентарным номером уже существует!");
                    return;
                }


                dbHelper.AddMedicalEquipment(inventoryNumber, txtName.Text);
                LoadMedicalEquipmentData();
                MessageBox.Show("Оборудование успешно добавлено!");
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("Ошибка: Оборудование с таким номером уже существует");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Ошибка: Указан несуществующий номер личного дела воспитанника");
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
                if (dataGridViewMedicalEquipment.SelectedRows.Count == 0 ||
                    dataGridViewMedicalEquipment.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите корректную запись для удаления!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewMedicalEquipment.SelectedRows[0];
                int selectedInventoryNumber = Convert.ToInt32(selectedRow.Cells["Инвентарный_номер_оборудования"].Value);

                DialogResult result = MessageBox.Show(
                    $"Удалить оборудование с инвентарным номером {selectedInventoryNumber}?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;

                dbHelper.DeleteMedicalEquipment(selectedInventoryNumber);
                LoadMedicalEquipmentData();
                MessageBox.Show("Запись удалена!");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нельзя удалить оборудование: существуют связанные записи");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void buttonEdit_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewMedicalEquipment.SelectedRows.Count == 0 ||
                    dataGridViewMedicalEquipment.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Выберите запись для редактирования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewMedicalEquipment.SelectedRows[0];
                txtInventoryNumber.Text = selectedRow.Cells["Инвентарный_номер_оборудования"].Value.ToString();
                txtName.Text = selectedRow.Cells["Название"].Value.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void buttonSave_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewMedicalEquipment.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите запись для сохранения!");
                    return;
                }

                if (string.IsNullOrEmpty(txtName.Text))
                {
                    MessageBox.Show("Заполните название оборудования!");
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewMedicalEquipment.SelectedRows[0];
                int originalInventoryNumber = Convert.ToInt32(selectedRow.Cells["Инвентарный_номер_оборудования"].Value);
                int newInventoryNumber = Convert.ToInt32(txtInventoryNumber.Text);

                // Проверка изменения инвентарного номера
                if (newInventoryNumber != originalInventoryNumber)
                {
                    if (dbHelper.IsMedicalEquipmentNumberExists(newInventoryNumber))
                    {
                        MessageBox.Show("Оборудование с таким инвентарным номером уже существует!");
                        return;
                    }
                }

                LoadMedicalEquipmentData();
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        public class DatabaseHelperM
        {
            private String connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;";

            // Методы для медицинского оборудования
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

            public void AddMedicalEquipment(int inventoryNumber, string name)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                INSERT INTO Мед_оборудование 
                    (Инвентарный_номер_оборудования, Название) 
                VALUES 
                    (@InventoryNumber, @Name)";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@InventoryNumber", inventoryNumber);
                    cmd.Parameters.AddWithValue("@Name", name);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            public void DeleteMedicalEquipment(int inventoryNumber)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Мед_оборудование WHERE Инвентарный_номер_оборудования = @InventoryNumber";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@InventoryNumber", inventoryNumber);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            public void UpdateMedicalEquipment(int originalInventoryNumber, string name, int newInventoryNumber)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                UPDATE Мед_оборудование 
                SET 
                    Инвентарный_номер_оборудования = @NewInventoryNumber,
                    Название = @Name,
                WHERE 
                    Инвентарный_номер_оборудования = @OriginalInventoryNumber";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@NewInventoryNumber", newInventoryNumber);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@OriginalInventoryNumber", originalInventoryNumber);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            public bool IsMedicalEquipmentNumberExists(int inventoryNumber)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Мед_оборудование WHERE Инвентарный_номер_оборудования = @InventoryNumber";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@InventoryNumber", inventoryNumber);

                    connection.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

      
    }
}
