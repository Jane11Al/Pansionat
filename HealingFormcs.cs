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
using System.Diagnostics;
using FastReport.DevComponents.DotNetBar;

namespace Pasionat
{
    public partial class HealingFormcs : Form
    {
        private HealingDatabaseHelper dbHelper;
        private DataTable studentsData;
        private DataTable appointmentsData;
        private DataTable equipmentData;
        private DataTable diagnosesData;

        private string currentStudentName = "";
        private int currentStudentId = -1;
        private int currentAppointmentId = -1;

        public HealingFormcs()
        {
            InitializeComponent();

            // Исправляем привязку событий
            this.buttonDeleteDiagnosis.Click -= this.buttonAddDiagnosis_Click;
            this.buttonDeleteDiagnosis.Click += this.buttonDeleteDiagnosis_Click;

            // Привязываем события для оборудования
            this.buttonAttachEquipment.Click += new System.EventHandler(this.buttonAttachEquipment_Click);
            this.buttonDetachEquipment.Click += new System.EventHandler(this.buttonDetachEquipment_Click);

            dbHelper = new HealingDatabaseHelper();
        }

        private void HealingFormcs_Load(object sender, EventArgs e)
        {
            try
            {
                LoadComboBoxData();
                LoadTab1Data();
                LoadTab2Data();
                LoadTab3Data();
                LoadTab4Data();
                UpdateStudentLabels();
                ConfigureDataGridViews();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке формы: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridViews()
        {
            // Скрываем столбец с кодом назначения в dataGridViewDrugs
            if (dataGridViewDrugs.Columns.Contains("Код_назначения"))
            {
                dataGridViewDrugs.Columns["Код_назначения"].Visible = false;
            }
        }

        private void UpdateStudentLabels()
        {
            labelCurrentStudent1.Text = currentStudentName;
            labelCurrentStudent2.Text = currentStudentName;
            labelCurrentStudent3.Text = currentStudentName;

            bool hasSelection = !string.IsNullOrEmpty(currentStudentName);
            labelCurrentStudent1.Visible = hasSelection;
            labelCurrentStudent2.Visible = hasSelection;
            labelCurrentStudent3.Visible = hasSelection;

            buttonAttachEquipment.Enabled = hasSelection;
            buttonDetachEquipment.Enabled = hasSelection;

            if (!hasSelection)
            {
                dataGridViewEquipment.DataSource = null;
            }
        }

        private void LoadTab1Data()
        {
            try
            {
                studentsData = dbHelper.LoadStudents();

                if (studentsData == null)
                {
                    MessageBox.Show("Данные воспитанников не загружены", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (studentsData.Rows.Count > 0)
                {
                    dataGridViewStudents.DataSource = studentsData;
                    dataGridViewStudents.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных воспитанников: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                diagnosesData = dbHelper.LoadDiagnoses();

                if (diagnosesData != null && diagnosesData.Rows.Count > 0)
                {
                    comboBoxDiagnosisStudent.DataSource = diagnosesData;
                    comboBoxDiagnosisStudent.DisplayMember = "Название";
                    comboBoxDiagnosisStudent.ValueMember = "Код_заболевания";
                    comboBoxDiagnosisStudent.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Нет данных о диагнозах в базе данных", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                DataTable drugs = dbHelper.LoadDrugs();
                if (drugs != null && drugs.Rows.Count > 0)
                {
                    comboBoxAppointmentDrug.DataSource = drugs;
                    comboBoxAppointmentDrug.DisplayMember = "Название_препарата";
                    comboBoxAppointmentDrug.ValueMember = "Название_препарата";
                    comboBoxAppointmentDrug.SelectedIndex = -1;
                }

                LoadEquipmentComboBox();
                dateTimePickerAppointmentDate.Value = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных для ComboBox: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadEquipmentComboBox()
        {
            try
            {
                DataTable freeEquipment = dbHelper.LoadFreeMedicalEquipment();

                if (freeEquipment != null && freeEquipment.Rows.Count > 0)
                {
                    comboBoxEquipment.DataSource = freeEquipment;
                    comboBoxEquipment.DisplayMember = "Название";
                    comboBoxEquipment.ValueMember = "Инвентарный_номер_оборудования";
                    comboBoxEquipment.SelectedIndex = -1;
                }
                else
                {
                    comboBoxEquipment.DataSource = null;
                    comboBoxEquipment.Items.Clear();
                    comboBoxEquipment.Text = "Нет свободного оборудования";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTab2Data()
        {
            try
            {
                DataTable studentDiagnoses;
                if (currentStudentId != -1)
                {
                    studentDiagnoses = dbHelper.LoadStudentDiagnosesByStudent(currentStudentId);
                }
                else
                {
                    studentDiagnoses = new DataTable();
                }

                if (studentDiagnoses != null && studentDiagnoses.Rows.Count > 0)
                {
                    dataGridViewStudentDiagnoses.DataSource = studentDiagnoses;
                    dataGridViewStudentDiagnoses.Refresh();
                }
                else
                {
                    dataGridViewStudentDiagnoses.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки диагнозов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTab3Data()
        {
            try
            {
                DataTable studentEquipment;
                if (currentStudentId != -1)
                {
                    studentEquipment = dbHelper.LoadMedicalEquipmentByStudent(currentStudentId);

                    DataTable filteredEquipment = studentEquipment.Clone();
                    foreach (DataRow row in studentEquipment.Rows)
                    {
                        if (row["Номер_личного_дела_воспитанника"] != DBNull.Value &&
                            Convert.ToInt32(row["Номер_личного_дела_воспитанника"]) == currentStudentId)
                        {
                            filteredEquipment.ImportRow(row);
                        }
                    }
                    studentEquipment = filteredEquipment;
                }
                else
                {
                    studentEquipment = new DataTable();
                }

                if (studentEquipment != null)
                {
                    dataGridViewEquipment.DataSource = studentEquipment;
                    dataGridViewEquipment.Refresh();
                }
                else
                {
                    dataGridViewEquipment.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSave1_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите воспитанника из таблицы!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewStudents.SelectedRows[0];
                currentStudentName = selectedRow.Cells["ФИО"].Value.ToString();
                currentStudentId = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);

                UpdateStudentLabels();
                LoadTab2Data();
                LoadTab3Data();
                LoadTab4Data();

                MessageBox.Show($"Выбран воспитанник: {currentStudentName}", "Сохранение выбора", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выборе воспитанника: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonAddDiagnosis_Click(object sender, EventArgs e)
        {
            if (currentStudentId == -1)
            {
                MessageBox.Show("Сначала выберите воспитанника на вкладке 'Общие сведения' и нажмите 'Сохранить выбор'!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxDiagnosisStudent.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите диагноз!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string diseaseCode;
                string diagnosisName;

                if (comboBoxDiagnosisStudent.SelectedValue != null)
                {
                    diseaseCode = comboBoxDiagnosisStudent.SelectedValue.ToString();
                    diagnosisName = comboBoxDiagnosisStudent.Text;
                }
                else if (comboBoxDiagnosisStudent.SelectedItem is DataRowView selectedRow)
                {
                    diseaseCode = selectedRow["Код_заболевания"]?.ToString() ?? "";
                    diagnosisName = selectedRow["Название"]?.ToString() ?? "";
                }
                else
                {
                    int selectedIndex = comboBoxDiagnosisStudent.SelectedIndex;
                    if (selectedIndex >= 0 && selectedIndex < diagnosesData.Rows.Count)
                    {
                        DataRow row = diagnosesData.Rows[selectedIndex];
                        diseaseCode = row["Код_заболевания"]?.ToString() ?? "";
                        diagnosisName = row["Название"]?.ToString() ?? "";
                    }
                    else
                    {
                        MessageBox.Show("Не удалось получить данные выбранного диагноза", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (string.IsNullOrEmpty(diseaseCode))
                {
                    MessageBox.Show("Не удалось получить код диагноза", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (IsDiagnosisAlreadyAssigned(currentStudentId, diseaseCode))
                {
                    MessageBox.Show($"Диагноз '{diagnosisName}' уже назначен воспитаннику {currentStudentName}!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dbHelper.AddStudentDiagnosis(currentStudentId, diseaseCode);
                LoadTab2Data();

                MessageBox.Show($"Диагноз '{diagnosisName}' успешно назначен воспитаннику {currentStudentName}!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                comboBoxDiagnosisStudent.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка назначения диагноза: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDeleteDiagnosis_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudentDiagnoses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите диагноз для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewStudentDiagnoses.SelectedRows[0];

                int studentId = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);
                string diseaseCode = selectedRow.Cells["Код_заболевания"].Value?.ToString();
                string studentName = selectedRow.Cells["ФИО_воспитанника"].Value?.ToString() ?? "неизвестный воспитанник";
                string diagnosisName = selectedRow.Cells["Название_диагноза"].Value?.ToString() ?? "неизвестный диагноз";

                if (string.IsNullOrEmpty(diseaseCode))
                {
                    MessageBox.Show("Не удалось получить код заболевания для удаления", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"Удалить диагноз '{diagnosisName}' (код: {diseaseCode}) у воспитанника {studentName}?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool success = dbHelper.DeleteStudentDiagnosis(studentId, diseaseCode);

                    if (success)
                    {
                        LoadTab2Data();
                        MessageBox.Show("Диагноз успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить диагноз. Возможно, запись уже была удалена или произошла ошибка базы данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления диагноза: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDiagnosisAlreadyAssigned(int studentId, string diseaseCode)
        {
            try
            {
                DataTable studentDiagnoses = dbHelper.LoadStudentDiagnosesByStudent(studentId);
                if (studentDiagnoses == null) return false;

                foreach (DataRow row in studentDiagnoses.Rows)
                {
                    string existingCode = row["Код_заболевания"]?.ToString();
                    if (!string.IsNullOrEmpty(existingCode) && existingCode == diseaseCode)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке диагноза: {ex.Message}");
                return false;
            }
        }

        private void buttonAttachEquipment_Click(object sender, EventArgs e)
        {
            if (comboBoxEquipment.SelectedValue == null)
            {
                MessageBox.Show("Выберите оборудование из списка!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentStudentId == -1)
            {
                MessageBox.Show("Сначала выберите воспитанника на вкладке 'Общие сведения' и нажмите 'Сохранить выбор'!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int inventoryNumber = (int)comboBoxEquipment.SelectedValue;
                string equipmentName = comboBoxEquipment.Text;

                DialogResult result = MessageBox.Show(
                    $"Привязать оборудование '{equipmentName}' к воспитаннику {currentStudentName}?",
                    "Подтверждение привязки",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool success = dbHelper.AttachEquipmentToStudent(inventoryNumber, currentStudentId);

                    if (success)
                    {
                        LoadTab3Data();
                        LoadEquipmentComboBox();
                        MessageBox.Show("Оборудование успешно привязано к воспитаннику!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось привязать оборудование.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка привязки оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDetachEquipment_Click(object sender, EventArgs e)
        {
            if (dataGridViewEquipment.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите оборудование для отвязки!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewEquipment.SelectedRows[0];

                int inventoryNumber = Convert.ToInt32(selectedRow.Cells["Инвентарный_номер_оборудования"].Value);
                string equipmentName = selectedRow.Cells["Название"].Value?.ToString() ?? "неизвестное оборудование";

                DialogResult result = MessageBox.Show(
                    $"Отвязать оборудование '{equipmentName}' от воспитанника {currentStudentName}?",
                    "Подтверждение отвязки",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool success = dbHelper.DetachEquipment(inventoryNumber);

                    if (success)
                    {
                        LoadTab3Data();
                        LoadEquipmentComboBox();
                        MessageBox.Show("Оборудование успешно отвязано от воспитанника!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось отвязать оборудование.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отвязки оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxSearchStudent_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = textBoxSearchStudent.Text.Trim();
                if (string.IsNullOrEmpty(searchText))
                {
                    dataGridViewStudents.DataSource = studentsData;
                    return;
                }

                DataTable filteredData = studentsData.Clone();
                var filteredRows = studentsData.AsEnumerable()
                    .Where(row => row.Field<string>("ФИО").ToLower().Contains(searchText.ToLower()) ||
                                 row.Field<string>("Основной_диагноз").ToLower().Contains(searchText.ToLower()) ||
                                 row.Field<int>("Номер_личного_дела_воспитанника").ToString().Contains(searchText));

                foreach (var row in filteredRows)
                {
                    filteredData.ImportRow(row);
                }

                dataGridViewStudents.DataSource = filteredData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ИСПРАВЛЕННЫЙ МЕТОД ДОБАВЛЕНИЯ/ОБНОВЛЕНИЯ НАЗНАЧЕНИЯ
        private void buttonAddAppointment_Click(object sender, EventArgs e)
        {
            if (!ValidateAppointmentData())
            {
                return;
            }

            try
            {
                string drugName = comboBoxAppointmentDrug.SelectedValue.ToString();
                DateTime appointmentDate = dateTimePickerAppointmentDate.Value;

                DialogResult result = MessageBox.Show(
                    currentAppointmentId == -1 ? "Добавить назначение?" : "Обновить назначение?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (currentAppointmentId == -1)
                    {
                        // Режим добавления - используем перегруженный метод без appointmentCode
                        dbHelper.AddAppointment(
                            drugName: drugName,
                            studentId: currentStudentId,
                            appointmentDate: appointmentDate
                        );
                        MessageBox.Show("Назначение успешно добавлено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Режим обновления - используем правильное имя параметра
                        dbHelper.UpdateAppointment(
                            appointmentCode: currentAppointmentId, // Исправлено с appointmentId на appointmentCode
                            drugName: drugName,
                            studentId: currentStudentId,
                            appointmentDate: appointmentDate
                        );
                        MessageBox.Show("Назначение успешно обновлено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    LoadTab4Data();
                    ClearAppointmentFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с назначением: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateAppointmentData()
        {
            if (comboBoxAppointmentDrug.SelectedValue == null)
            {
                MessageBox.Show("Выберите препарат!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (currentStudentId == -1)
            {
                MessageBox.Show("Сначала выберите воспитанника на вкладке 'Общие сведения' и нажмите 'Сохранить выбор'!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (dateTimePickerAppointmentDate.Value > DateTime.Now)
            {
                MessageBox.Show("Дата назначения не может быть в будущем!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void buttonEditAppointment_Click(object sender, EventArgs e)
        {
            if (dataGridViewDrugs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите назначение для редактирования!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewDrugs.SelectedRows[0];

                int appointmentStudentId = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);
                if (currentStudentId != -1 && appointmentStudentId != currentStudentId)
                {
                    MessageBox.Show("Выбранное назначение принадлежит другому воспитаннику!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                currentAppointmentId = Convert.ToInt32(selectedRow.Cells["Код_назначения"].Value);
                comboBoxAppointmentDrug.SelectedValue = selectedRow.Cells["Название_препарата"].Value.ToString();
                dateTimePickerAppointmentDate.Value = Convert.ToDateTime(selectedRow.Cells["Дата_назначения"].Value);

                buttonAddAppointment.Text = "Обновить";
                tabControl2.SelectedTab = tabPage4;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных назначения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDeleteAppointment_Click(object sender, EventArgs e)
        {
            if (dataGridViewDrugs.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите назначение для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewDrugs.SelectedRows[0];
                int appointmentCode = Convert.ToInt32(selectedRow.Cells["Код_назначения"].Value);
                string drugName = selectedRow.Cells["Название_препарата"].Value?.ToString() ?? "неизвестный препарат";
                string studentName = selectedRow.Cells["ФИО_воспитанника"].Value?.ToString() ?? "неизвестный воспитанник";

                int appointmentStudentId = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);
                if (currentStudentId != -1 && appointmentStudentId != currentStudentId)
                {
                    MessageBox.Show("Выбранное назначение принадлежит другому воспитаннику!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"Удалить назначение препарата '{drugName}' для воспитанника {studentName}?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    dbHelper.DeleteAppointment(appointmentCode);
                    LoadTab4Data();
                    ClearAppointmentFields();
                    MessageBox.Show("Назначение успешно удалено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления назначения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearAppointmentFields()
        {
            currentAppointmentId = -1;
            comboBoxAppointmentDrug.SelectedIndex = -1;
            dateTimePickerAppointmentDate.Value = DateTime.Now;
            buttonAddAppointment.Text = "Добавить";
        }

        private void LoadTab4Data()
        {
            try
            {
                DataTable appointments;
                if (currentStudentId != -1)
                {
                    appointments = dbHelper.LoadAppointmentsByStudent(currentStudentId);
                }
                else
                {
                    appointments = dbHelper.LoadAppointments();
                }

                if (appointments != null)
                {
                    dataGridViewDrugs.DataSource = appointments;
                    dataGridViewDrugs.Refresh();
                    ConfigureDataGridViews();
                }
                else
                {
                    dataGridViewDrugs.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки назначений: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancelAppointment_Click(object sender, EventArgs e)
        {
            ClearAppointmentFields();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl2.SelectedTab != tabPage4)
            {
                ClearAppointmentFields();
            }
        }
    }
}