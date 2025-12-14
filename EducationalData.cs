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
using System.Diagnostics;
using System.Configuration;

namespace Pasionat
{
    public partial class EducationalData : Form
    {
        private EducationDatabaseHelper dbHelper;
        private DataTable studentsData;
        private DataTable teachersData;
        private DataTable programsData;
        private DataTable educationData;
        private DataTable subjectsData;

        private int _currentStudentCode = -1;
        private int _selectedStudentId = -1;
        private string _selectedStudentName = "";

        public EducationalData()
        {
            InitializeComponent();
            dbHelper = new EducationDatabaseHelper();

            // Настройка DataGridView
            ConfigureDataGridView();
        }

        private void ConfigureDataGridView()
        {
            dataGridViewEducation.AutoGenerateColumns = true;
            dataGridViewEducation.ReadOnly = true;
            dataGridViewEducation.AllowUserToAddRows = false;
            dataGridViewEducation.AllowUserToDeleteRows = false;
            dataGridViewEducation.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void EducationalData_Load(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("=== ЗАГРУЗКА ФОРМЫ EducationalData ===");

                // Загружаем данные последовательно
                LoadComboBoxData();
                LoadEducationData();
                LoadSubjectsData();
                ClearFields();

                Debug.WriteLine("=== ФОРМА УСПЕШНО ЗАГРУЖЕНА ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки формы: {ex.Message}");
                MessageBox.Show($"Ошибка при загрузке формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                Debug.WriteLine("Загрузка данных в ComboBox...");

                LoadStudentsComboBox();
                Application.DoEvents();

                LoadTeachersComboBox();
                Application.DoEvents();

                LoadProgramsComboBox();
                Application.DoEvents();

                LoadYearsComboBox();

                Debug.WriteLine("ComboBox данные успешно загружены");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки ComboBox: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных для ComboBox: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSubjectsData()
        {
            try
            {
                Debug.WriteLine("Загрузка данных предметов...");
                subjectsData = dbHelper.LoadSubjects();

                if (subjectsData != null && subjectsData.Rows.Count > 0)
                {
                    comboBox1.DataSource = subjectsData;
                    comboBox1.DisplayMember = "Название_учебного_предмета";
                    comboBox1.ValueMember = "Название_учебного_предмета";
                    comboBox1.SelectedIndex = -1;

                    Debug.WriteLine($"Загружено предметов: {subjectsData.Rows.Count}");
                }
                else
                {
                    Debug.WriteLine("ВНИМАНИЕ: Нет данных о предметах");
                    comboBox1.DataSource = null;
                    comboBox1.Items.Clear();
                    comboBox1.Text = "Нет данных о предметах";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки предметов: {ex.Message}");
                comboBox1.DataSource = null;
                comboBox1.Items.Clear();
                comboBox1.Text = "Ошибка загрузки";
            }
        }

        private void LoadStudentsComboBox()
        {
            try
            {
                Debug.WriteLine("Начало загрузки студентов...");
                studentsData = dbHelper.LoadStudents();

                if (studentsData != null && studentsData.Rows.Count > 0)
                {
                    Debug.WriteLine($"Загружено студентов: {studentsData.Rows.Count}");

                    // Очищаем и перезагружаем комбобокс
                    comboBoxStudent.BeginUpdate();
                    comboBoxStudent.DataSource = null;
                    comboBoxStudent.Items.Clear();

                    comboBoxStudent.DataSource = studentsData;
                    comboBoxStudent.DisplayMember = "ФИО";
                    comboBoxStudent.ValueMember = "Номер_личного_дела_воспитанника";
                    comboBoxStudent.SelectedIndex = -1;
                    comboBoxStudent.EndUpdate();

                    Debug.WriteLine($"ComboBoxStudent items: {comboBoxStudent.Items.Count}");

                    // Проверяем привязку данных
                    if (comboBoxStudent.Items.Count > 0)
                    {
                        Debug.WriteLine("Данные студентов успешно загружены в комбобокс");
                    }
                }
                else
                {
                    Debug.WriteLine("ВНИМАНИЕ: Нет данных о воспитанниках");
                    comboBoxStudent.DataSource = null;
                    comboBoxStudent.Items.Clear();
                    comboBoxStudent.Text = "Нет данных о воспитанниках";
                    MessageBox.Show("В базе данных нет информации о воспитанниках.", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки воспитанников: {ex.Message}");
                comboBoxStudent.DataSource = null;
                comboBoxStudent.Items.Clear();
                comboBoxStudent.Text = "Ошибка загрузки";
                MessageBox.Show($"Ошибка загрузки списка воспитанников: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTeachersComboBox()
        {
            try
            {
                Debug.WriteLine("Начало загрузки педагогов...");
                teachersData = dbHelper.LoadTeachers();

                if (teachersData != null && teachersData.Rows.Count > 0)
                {
                    Debug.WriteLine($"Загружено педагогов: {teachersData.Rows.Count}");

                    comboBoxTeacher.BeginUpdate();
                    comboBoxTeacher.DataSource = null;
                    comboBoxTeacher.Items.Clear();

                    comboBoxTeacher.DataSource = teachersData;
                    comboBoxTeacher.DisplayMember = "ФИО_педагога";
                    comboBoxTeacher.ValueMember = "ФИО_педагога";
                    comboBoxTeacher.SelectedIndex = -1;
                    comboBoxTeacher.EndUpdate();

                    Debug.WriteLine($"ComboBoxTeacher items: {comboBoxTeacher.Items.Count}");
                }
                else
                {
                    Debug.WriteLine("ВНИМАНИЕ: Нет данных о педагогах");
                    comboBoxTeacher.DataSource = null;
                    comboBoxTeacher.Items.Clear();
                    comboBoxTeacher.Text = "Нет данных о педагогах";
                    MessageBox.Show("В базе данных нет информации о педагогах.", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки педагогов: {ex.Message}");
                comboBoxTeacher.DataSource = null;
                comboBoxTeacher.Items.Clear();
                comboBoxTeacher.Text = "Ошибка загрузки";
                MessageBox.Show($"Ошибка загрузки списка педагогов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProgramsComboBox()
        {
            try
            {
                Debug.WriteLine("Начало загрузки программ...");
                programsData = dbHelper.LoadPrograms();

                if (programsData != null && programsData.Rows.Count > 0)
                {
                    Debug.WriteLine($"Загружено программ: {programsData.Rows.Count}");

                    comboBoxProgram.BeginUpdate();
                    comboBoxProgram.DataSource = null;
                    comboBoxProgram.Items.Clear();

                    comboBoxProgram.DataSource = programsData;
                    comboBoxProgram.DisplayMember = "Название";
                    comboBoxProgram.ValueMember = "Код_программы";
                    comboBoxProgram.SelectedIndex = -1;
                    comboBoxProgram.EndUpdate();

                    Debug.WriteLine($"ComboBoxProgram items: {comboBoxProgram.Items.Count}");
                }
                else
                {
                    Debug.WriteLine("ВНИМАНИЕ: Нет данных о программах");
                    comboBoxProgram.DataSource = null;
                    comboBoxProgram.Items.Clear();
                    comboBoxProgram.Text = "Нет данных о программах";
                    MessageBox.Show("В базе данных нет информации об образовательных программах.", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки программ: {ex.Message}");
                comboBoxProgram.DataSource = null;
                comboBoxProgram.Items.Clear();
                comboBoxProgram.Text = "Ошибка загрузки";
                MessageBox.Show($"Ошибка загрузки списка программ: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadYearsComboBox()
        {
            try
            {
                comboBoxYear.BeginUpdate();
                comboBoxYear.Items.Clear();

                int currentYear = DateTime.Now.Year;
                for (int year = currentYear - 5; year <= currentYear + 5; year++)
                {
                    comboBoxYear.Items.Add(year);
                }
                comboBoxYear.SelectedIndex = -1;
                comboBoxYear.EndUpdate();

                Debug.WriteLine($"Добавлено годов в комбобокс: {comboBoxYear.Items.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки годов: {ex.Message}");
            }
        }

        private void LoadEducationData()
        {
            try
            {
                Debug.WriteLine("Начало загрузки данных в DataGridView...");

                educationData = dbHelper.LoadEducationData();

                if (educationData != null && educationData.Rows.Count > 0)
                {
                    // Очищаем и перезагружаем DataGridView
                    dataGridViewEducation.BeginInvoke(new Action(() =>
                    {
                        dataGridViewEducation.DataSource = null;
                        dataGridViewEducation.DataSource = educationData;
                        dataGridViewEducation.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                        dataGridViewEducation.Refresh();
                    }));

                    Debug.WriteLine($"=== ДАННЫЕ DataGridView ЗАГРУЖЕНЫ ===");
                    Debug.WriteLine($"Количество записей: {educationData.Rows.Count}");
                    Debug.WriteLine($"Количество столбцов: {educationData.Columns.Count}");

                    // Логируем названия столбцов
                    string columns = "Столбцы: " + string.Join(", ",
                        educationData.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                    Debug.WriteLine(columns);

                    if (educationData.Rows.Count > 0)
                    {
                        Debug.WriteLine("Первая запись: " +
                            $"Код: {educationData.Rows[0]["Код_обучамого_в_определенном_году"]}, " +
                            $"Воспитанник: {educationData.Rows[0]["ФИО_воспитанника"]}");
                    }
                }
                else
                {
                    Debug.WriteLine("ВНИМАНИЕ: Нет данных об обучении");
                    dataGridViewEducation.DataSource = null;
                    MessageBox.Show("В базе данных нет записей об обучении воспитанников.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки DataGridView: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных об обучении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                Debug.WriteLine($"Проверка выбранных значений:");
                Debug.WriteLine($"comboBoxStudent.SelectedValue: {comboBoxStudent.SelectedValue}");
                Debug.WriteLine($"comboBoxTeacher.SelectedValue: {comboBoxTeacher.SelectedValue}");
                Debug.WriteLine($"comboBoxProgram.SelectedValue: {comboBoxProgram.SelectedValue}");
                Debug.WriteLine($"comboBoxYear.SelectedItem: {comboBoxYear.SelectedItem}");

                if (comboBoxStudent.SelectedValue == null)
                {
                    MessageBox.Show("Ошибка: не выбран воспитанник!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int studentId = (int)comboBoxStudent.SelectedValue;
                string teacherName = comboBoxTeacher.SelectedValue?.ToString();
                int programCode = (int)comboBoxProgram.SelectedValue;
                short studyYear = short.Parse(comboBoxYear.SelectedItem.ToString());

                Debug.WriteLine($"Добавление записи: StudentId={studentId}, Teacher={teacherName}, Program={programCode}, Year={studyYear}");

                // Проверяем, нет ли уже такой записи
                if (IsEducationRecordExists(studentId, programCode, studyYear))
                {
                    MessageBox.Show("Запись об обучении для этого воспитанника по выбранной программе и году уже существует!", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Автогенерация кода обучаемого
                int newStudentCode = dbHelper.GetNextStudentCode();
                Debug.WriteLine($"Сгенерирован код обучаемого: {newStudentCode}");

                dbHelper.AddEducationRecord(
                    studentCode: newStudentCode,
                    studyYear: studyYear,
                    programCode: programCode,
                    teacherName: teacherName,
                    studentId: studentId
                );

                Debug.WriteLine("Запись успешно добавлена в БД");

                // Обновляем данные
                LoadEducationData();
                ClearFields();

                MessageBox.Show($"Запись об обучении успешно добавлена! Код обучаемого: {newStudentCode}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА добавления записи: {ex.Message}");
                MessageBox.Show($"Ошибка добавления записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsEducationRecordExists(int studentId, int programCode, short studyYear)
        {
            try
            {
                if (educationData == null) return false;

                foreach (DataRow row in educationData.Rows)
                {
                    if (row.Field<int>("Номер_личного_дела_воспитанника") == studentId &&
                        row.Field<int>("Код_программы") == programCode &&
                        row.Field<short>("Год_обучения") == studyYear)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка проверки дублирования: {ex.Message}");
                return false;
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewEducation.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewEducation.SelectedRows[0];
                int studentCode = Convert.ToInt32(selectedRow.Cells["Код_обучамого_в_определенном_году"].Value);
                string studentName = selectedRow.Cells["ФИО_воспитанника"].Value?.ToString() ?? "неизвестный воспитанник";
                string programName = selectedRow.Cells["Название_программы"].Value?.ToString() ?? "неизвестная программа";
                short studyYear = Convert.ToInt16(selectedRow.Cells["Год_обучения"].Value);

                Debug.WriteLine($"Удаление записи: Code={studentCode}, Student={studentName}, Program={programName}, Year={studyYear}");

                DialogResult result = MessageBox.Show(
                    $"Удалить запись об обучении?\n" +
                    $"Воспитанник: {studentName}\n" +
                    $"Программа: {programName}\n" +
                    $"Год: {studyYear}",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    dbHelper.DeleteEducationRecord(studentCode);
                    Debug.WriteLine("Запись удалена из БД");
                    LoadEducationData();
                    ClearFields();
                    MessageBox.Show("Запись об обучении удалена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА удаления записи: {ex.Message}");
                MessageBox.Show($"Ошибка удаления записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewEducation.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewEducation.SelectedRows[0];

                // Сохраняем код обучаемого для обновления
                _currentStudentCode = Convert.ToInt32(selectedRow.Cells["Код_обучамого_в_определенном_году"].Value);

                // Заполняем поля формы данными из выбранной строки
                int studentId = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);
                string teacherName = selectedRow.Cells["ФИО_педагога"].Value?.ToString();
                int programCode = Convert.ToInt32(selectedRow.Cells["Код_программы"].Value);
                short studyYear = Convert.ToInt16(selectedRow.Cells["Год_обучения"].Value);

                Debug.WriteLine($"Редактирование записи: CurrentCode={_currentStudentCode}, StudentId={studentId}, Teacher={teacherName}, Program={programCode}, Year={studyYear}");

                // Устанавливаем значения в комбобоксы
                comboBoxStudent.SelectedValue = studentId;
                comboBoxTeacher.SelectedValue = teacherName;
                comboBoxProgram.SelectedValue = programCode;
                comboBoxYear.SelectedItem = studyYear;

                // Активируем кнопку сохранения
                buttonSave.Enabled = true;
                buttonAdd.Enabled = false;

                Debug.WriteLine("Данные для редактирования загружены в форму");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА загрузки данных для редактирования: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных для редактирования: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            if (_currentStudentCode == -1)
            {
                MessageBox.Show("Сначала выберите запись для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int studentId = (int)comboBoxStudent.SelectedValue;
                string teacherName = comboBoxTeacher.SelectedValue?.ToString();
                int programCode = (int)comboBoxProgram.SelectedValue;
                short studyYear = short.Parse(comboBoxYear.SelectedItem.ToString());

                Debug.WriteLine($"Сохранение изменений: Code={_currentStudentCode}, StudentId={studentId}, Teacher={teacherName}, Program={programCode}, Year={studyYear}");

                dbHelper.UpdateEducationRecord(
                    studentCode: _currentStudentCode,
                    studyYear: studyYear,
                    programCode: programCode,
                    teacherName: teacherName,
                    studentId: studentId
                );

                Debug.WriteLine("Изменения успешно сохранены в БД");
                LoadEducationData();
                ClearFields();
                MessageBox.Show("Запись об обучении обновлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА обновления записи: {ex.Message}");
                MessageBox.Show($"Ошибка обновления записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Обновление данных...");
                LoadComboBoxData();
                LoadEducationData();
                ClearFields();
                MessageBox.Show("Данные обновлены!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА обновления: {ex.Message}");
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = textBoxSearch.Text.Trim();
                Debug.WriteLine($"Поиск по тексту: '{searchText}'");

                if (string.IsNullOrEmpty(searchText))
                {
                    Debug.WriteLine("Текст поиска пуст - загрузка всех данных");
                    LoadEducationData();
                    return;
                }

                if (educationData == null)
                {
                    MessageBox.Show("Нет данных для поиска", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataTable filteredData = educationData.Clone();

                var filteredRows = educationData.AsEnumerable()
                    .Where(row =>
                        (row.Field<string>("ФИО_воспитанника")?.ToLower().Contains(searchText.ToLower()) ?? false) ||
                        (row.Field<string>("ФИО_педагога")?.ToLower().Contains(searchText.ToLower()) ?? false) ||
                        (row.Field<string>("Название_программы")?.ToLower().Contains(searchText.ToLower()) ?? false) ||
                        (row.Field<short>("Год_обучения").ToString().Contains(searchText)));

                foreach (var row in filteredRows)
                {
                    filteredData.ImportRow(row);
                }

                dataGridViewEducation.DataSource = filteredData;
                Debug.WriteLine($"Найдено записей: {filteredData.Rows.Count}");

                if (filteredData.Rows.Count == 0)
                {
                    MessageBox.Show("Записи не найдены", "Результат поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА поиска: {ex.Message}");
                MessageBox.Show("Ошибка поиска: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonPick_Click(object sender, EventArgs e)
        {
            if (dataGridViewEducation.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись об обучении из таблицы!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = dataGridViewEducation.SelectedRows[0];
                _selectedStudentId = Convert.ToInt32(selectedRow.Cells["Номер_личного_дела_воспитанника"].Value);
                _selectedStudentName = selectedRow.Cells["ФИО_воспитанника"].Value?.ToString() ?? "";
                _currentStudentCode = Convert.ToInt32(selectedRow.Cells["Код_обучамого_в_определенном_году"].Value);

                // Обновляем метку на второй вкладке
                labelFIO.Text = _selectedStudentName;

                // Показываем информацию о выборе
                string programName = selectedRow.Cells["Название_программы"].Value?.ToString() ?? "";
                short studyYear = Convert.ToInt16(selectedRow.Cells["Год_обучения"].Value);

                Debug.WriteLine($"Выбран воспитанник: ID={_selectedStudentId}, ФИО={_selectedStudentName}, Код={_currentStudentCode}");

                MessageBox.Show($"Выбран воспитанник: {_selectedStudentName}\nПрограмма: {programName}\nГод: {studyYear}",
                    "Выбор воспитанника", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА выбора воспитанника: {ex.Message}");
                MessageBox.Show($"Ошибка выбора воспитанника: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_selectedStudentId == -1 || string.IsNullOrEmpty(_selectedStudentName))
            {
                MessageBox.Show("Сначала выберите воспитанника на первой вкладке (кнопка 'Выбрать')!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите учебный предмет!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string goalDescription = textBox2.Text.Trim();
            string resultDescription = textBox3.Text.Trim();

            if (string.IsNullOrEmpty(goalDescription))
            {
                MessageBox.Show("Введите описание цели обучения!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus();
                return;
            }

            if (string.IsNullOrEmpty(resultDescription))
            {
                MessageBox.Show("Введите описание результатов обучения!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox3.Focus();
                return;
            }

            try
            {
                string subjectName = comboBox1.SelectedValue?.ToString();
                string subjectAreaCode = "";

                // Получаем код предметной области из выбранного предмета
                if (subjectsData != null && comboBox1.SelectedIndex >= 0)
                {
                    var selectedRow = subjectsData.Rows[comboBox1.SelectedIndex];
                    subjectAreaCode = selectedRow["Код_предметной_области"]?.ToString();
                }

                Debug.WriteLine($"Сохранение результатов: Студент={_selectedStudentName}, Предмет={subjectName}, Код обучаемого={_currentStudentCode}");

                // Сохраняем результаты изучения предмета
                dbHelper.SaveSubjectResults(
                    studentCode: _currentStudentCode,
                    subjectName: subjectName,
                    subjectAreaCode: subjectAreaCode,
                    goalDescription: goalDescription,
                    resultDescription: resultDescription
                );

                MessageBox.Show("Результаты изучения учебного предмета успешно сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Очищаем поля после сохранения
                ClearSubjectFields();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА сохранения результатов: {ex.Message}");
                MessageBox.Show($"Ошибка сохранения результатов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearSubjectFields()
        {
            textBox2.Clear();
            textBox3.Clear();
            comboBox1.SelectedIndex = -1;
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // При переключении на вторую вкладку обновляем ФИО выбранного воспитанника
            if (tabControl2.SelectedTab == tabPage2)
            {
                if (!string.IsNullOrEmpty(_selectedStudentName))
                {
                    labelFIO.Text = _selectedStudentName;
                }
                else
                {
                    labelFIO.Text = "Воспитанник не выбран";
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Очистка полей формы");
            ClearFields();
        }

        private bool ValidateInputs()
        {
            if (comboBoxStudent.SelectedIndex == -1 || comboBoxStudent.SelectedValue == null)
            {
                Debug.WriteLine("Валидация: не выбран воспитанник");
                MessageBox.Show("Выберите воспитанника!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxStudent.Focus();
                return false;
            }

            if (comboBoxTeacher.SelectedIndex == -1 || comboBoxTeacher.SelectedValue == null)
            {
                Debug.WriteLine("Валидация: не выбран педагог");
                MessageBox.Show("Выберите педагога!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxTeacher.Focus();
                return false;
            }

            if (comboBoxProgram.SelectedIndex == -1 || comboBoxProgram.SelectedValue == null)
            {
                Debug.WriteLine("Валидация: не выбрана программа");
                MessageBox.Show("Выберите программу!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxProgram.Focus();
                return false;
            }

            if (comboBoxYear.SelectedIndex == -1)
            {
                Debug.WriteLine("Валидация: не выбран год обучения");
                MessageBox.Show("Выберите год обучения!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxYear.Focus();
                return false;
            }

            Debug.WriteLine("Валидация: все поля заполнены корректно");
            return true;
        }

        private void ClearFields()
        {
            comboBoxStudent.SelectedIndex = -1;
            comboBoxTeacher.SelectedIndex = -1;
            comboBoxProgram.SelectedIndex = -1;
            comboBoxYear.SelectedIndex = -1;
            textBoxSearch.Clear();

            // Сбрасываем состояние кнопок
            buttonAdd.Enabled = true;
            buttonSave.Enabled = false;
            _currentStudentCode = -1;

            Debug.WriteLine("Поля формы очищены");
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
            // Обработчик клика по вкладке
        }

        private void dataGridViewEducation_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Автоматически подгоняем размер столбцов после привязки данных
            if (dataGridViewEducation.Columns.Count > 0)
            {
                dataGridViewEducation.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }
    }

    public class EducationDatabaseHelper
    {
        private string connectionString;

        public EducationDatabaseHelper()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PansionatDB"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=MSI;Database=PansionatDB;User Id=admin;Password=test1;Integrated Security=False;";
            }
        }
        public DataTable LoadSubjects()
        {
            DataTable dt = new DataTable();
            try
            {
                Debug.WriteLine("Выполнение запроса для предметов...");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            уп.Название_учебного_предмета,
                            уп.Код_предметной_области,
                            по.Название_предметной_области,
                            уп.Объем_в_часах
                        FROM Учебный_предмет уп
                        INNER JOIN Предметная_область по ON уп.Код_предметной_области = по.Код_предметной_области
                        ORDER BY по.Название_предметной_области, уп.Название_учебного_предмета";

                    Debug.WriteLine($"SQL для предметов: {query}");

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    connection.Open();
                    adapter.Fill(dt);

                    Debug.WriteLine($"Загружено предметов: {dt.Rows.Count}");
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА загрузки предметов: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА загрузки предметов: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
            return dt;
        }

        // ДОБАВЛЕН МЕТОД ДЛЯ СОХРАНЕНИЯ РЕЗУЛЬТАТОВ ИЗУЧЕНИЯ ПРЕДМЕТА
        public void SaveSubjectResults(int studentCode, string subjectName, string subjectAreaCode, string goalDescription, string resultDescription)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO Результат_изучения_учебных_предметов_воспитанника 
                        (Название_учебного_предмета, Код_предметной_области, Код_обучамого_в_определенном_году, Описание_цели_обучения, Описание_результатов_обучения) 
                        VALUES 
                        (@SubjectName, @SubjectAreaCode, @StudentCode, @GoalDescription, @ResultDescription)";

                    Debug.WriteLine($"Сохранение результатов предмета: Subject={subjectName}, AreaCode={subjectAreaCode}, StudentCode={studentCode}");

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@SubjectName", subjectName);
                    cmd.Parameters.AddWithValue("@SubjectAreaCode", subjectAreaCode);
                    cmd.Parameters.AddWithValue("@StudentCode", studentCode);
                    cmd.Parameters.AddWithValue("@GoalDescription", goalDescription);
                    cmd.Parameters.AddWithValue("@ResultDescription", resultDescription);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    Debug.WriteLine($"Сохранено записей результатов: {rowsAffected}");
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА сохранения результатов предмета: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);

                if (sqlEx.Number == 547) // Ошибка внешнего ключа
                {
                    errorMsg += "\n\nПроверьте:\n" +
                               "1. Существует ли выбранный учебный предмет\n" +
                               "2. Корректность кода предметной области\n" +
                               "3. Существование записи об обучении воспитанника";
                }

                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА сохранения результатов предмета: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
        }

        // ДОБАВЛЕН МЕТОД ДЛЯ ПРОВЕРКИ И СОЗДАНИЯ ИНДИВИДУАЛЬНОЙ ПРОГРАММЫ
        private bool EnsureIndividualProgramExists(int programCode, short studyYear, SqlConnection connection, SqlTransaction transaction = null)
        {
            try
            {
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM Индивидуальная_программа 
                    WHERE Код_программы = @ProgramCode AND Год_обучения = @StudyYear";

                SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                if (transaction != null) checkCmd.Transaction = transaction;

                checkCmd.Parameters.AddWithValue("@ProgramCode", programCode);
                checkCmd.Parameters.AddWithValue("@StudyYear", studyYear);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count == 0)
                {
                    string insertQuery = @"
                        INSERT INTO Индивидуальная_программа (Код_программы, Год_обучения) 
                        VALUES (@ProgramCode, @StudyYear)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                    if (transaction != null) insertCmd.Transaction = transaction;

                    insertCmd.Parameters.AddWithValue("@ProgramCode", programCode);
                    insertCmd.Parameters.AddWithValue("@StudyYear", studyYear);

                    int rowsAffected = insertCmd.ExecuteNonQuery();
                    Debug.WriteLine($"Создана индивидуальная программа: Program={programCode}, Year={studyYear}, Rows={rowsAffected}");

                    return rowsAffected > 0;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке/создании индивидуальной программы: {ex.Message}");
                throw new Exception($"Ошибка работы с индивидуальной программой: {ex.Message}", ex);
            }
        }

        // ОБНОВЛЕННЫЙ МЕТОД ДОБАВЛЕНИЯ ЗАПИСИ
        public void AddEducationRecord(int studentCode, short studyYear, int programCode, string teacherName, int studentId)
        {
            SqlTransaction transaction = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    try
                    {
                        if (!EnsureIndividualProgramExists(programCode, studyYear, connection, transaction))
                        {
                            throw new Exception("Не удалось создать индивидуальную программу");
                        }

                        string query = @"
                            INSERT INTO Обучение_воспитанника 
                            (Код_обучамого_в_определенном_году, Год_обучения, Код_программы, ФИО_педагога, Номер_личного_дела_воспитанника) 
                            VALUES 
                            (@StudentCode, @StudyYear, @ProgramCode, @TeacherName, @StudentId)";

                        Debug.WriteLine($"Добавление записи: Code={studentCode}, Year={studyYear}, Program={programCode}, Teacher={teacherName}, StudentId={studentId}");

                        SqlCommand cmd = new SqlCommand(query, connection, transaction);
                        cmd.Parameters.AddWithValue("@StudentCode", studentCode);
                        cmd.Parameters.AddWithValue("@StudyYear", studyYear);
                        cmd.Parameters.AddWithValue("@ProgramCode", programCode);
                        cmd.Parameters.AddWithValue("@TeacherName", teacherName);
                        cmd.Parameters.AddWithValue("@StudentId", studentId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        transaction.Commit();

                        Debug.WriteLine($"Добавлено записей: {rowsAffected}");
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        Debug.WriteLine($"ОШИБКА в транзакции: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА добавления записи: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);

                if (sqlEx.Number == 547)
                {
                    errorMsg += "\n\nПроверьте:\n" +
                               "1. Существует ли программа с указанным кодом\n" +
                               "2. Существует ли воспитанник с указанным номером дела\n" +
                               "3. Существует ли педагог с указанным ФИО";
                }

                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА добавления записи: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
        }

        // ОБНОВЛЕННЫЙ МЕТОД РЕДАКТИРОВАНИЯ ЗАПИСИ
        public void UpdateEducationRecord(int studentCode, short studyYear, int programCode, string teacherName, int studentId)
        {
            SqlTransaction transaction = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    try
                    {
                        // 1. Проверяем и создаем индивидуальную программу если нужно
                        if (!EnsureIndividualProgramExists(programCode, studyYear, connection, transaction))
                        {
                            throw new Exception("Не удалось создать индивидуальную программу");
                        }

                        // 2. Обновляем запись об обучении
                        string query = @"
                        UPDATE Обучение_воспитанника 
                        SET 
                            Год_обучения = @StudyYear,
                            Код_программы = @ProgramCode,
                            ФИО_педагога = @TeacherName,
                            Номер_личного_дела_воспитанника = @StudentId
                        WHERE Код_обучамого_в_определенном_году = @StudentCode";

                        Debug.WriteLine($"Обновление записи: Code={studentCode}, Year={studyYear}, Program={programCode}, Teacher={teacherName}, StudentId={studentId}");

                        SqlCommand cmd = new SqlCommand(query, connection, transaction);
                        cmd.Parameters.AddWithValue("@StudentCode", studentCode);
                        cmd.Parameters.AddWithValue("@StudyYear", studyYear);
                        cmd.Parameters.AddWithValue("@ProgramCode", programCode);
                        cmd.Parameters.AddWithValue("@TeacherName", teacherName);
                        cmd.Parameters.AddWithValue("@StudentId", studentId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        transaction.Commit();

                        Debug.WriteLine($"Обновлено записей: {rowsAffected}");

                        if (rowsAffected == 0)
                        {
                            throw new Exception("Запись не найдена или не была изменена");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        Debug.WriteLine($"ОШИБКА в транзакции обновления: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА обновления записи: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);

                if (sqlEx.Number == 547)
                {
                    errorMsg += "\n\nПроверьте корректность данных:\n" +
                               "- Программа должна существовать\n" +
                               "- Воспитанник должен существовать\n" +
                               "- Педагог должен существовать";
                }

                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА обновления записи: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
        }

        public DataTable LoadStudents()
        {
            DataTable dt = new DataTable();
            try
            {
                Debug.WriteLine("Выполнение запроса для студентов...");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Номер_личного_дела_воспитанника, ФИО FROM Воспитанник ORDER BY ФИО";
                    Debug.WriteLine($"SQL для студентов: {query}");

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    Debug.WriteLine("Открытие подключения для студентов...");
                    connection.Open();

                    Debug.WriteLine("Заполнение DataTable студентов...");
                    adapter.Fill(dt);

                    Debug.WriteLine($"Загружено студентов: {dt.Rows.Count}");

                    if (dt.Rows.Count > 0)
                    {
                        Debug.WriteLine($"Колонки студентов: {string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА загрузки студентов: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА загрузки студентов: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
            return dt;
        }

        public DataTable LoadTeachers()
        {
            DataTable dt = new DataTable();
            try
            {
                Debug.WriteLine("Выполнение запроса для педагогов...");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT ФИО_педагога FROM Педагог ORDER BY ФИО_педагога";
                    Debug.WriteLine($"SQL для педагогов: {query}");

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    connection.Open();
                    adapter.Fill(dt);

                    Debug.WriteLine($"Загружено педагогов: {dt.Rows.Count}");
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА загрузки педагогов: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА загрузки педагогов: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
            return dt;
        }

        public DataTable LoadPrograms()
        {
            DataTable dt = new DataTable();
            try
            {
                Debug.WriteLine("Выполнение запроса для программ...");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Код_программы, Название FROM Программа ORDER BY Название";
                    Debug.WriteLine($"SQL для программ: {query}");

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    connection.Open();
                    adapter.Fill(dt);

                    Debug.WriteLine($"Загружено программ: {dt.Rows.Count}");
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА загрузки программ: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА загрузки программ: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
            return dt;
        }

        public DataTable LoadEducationData()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            ob.Код_обучамого_в_определенном_году,
                            ob.Год_обучения,
                            ob.Код_программы,
                            ob.ФИО_педагога,
                            ob.Номер_личного_дела_воспитанника,
                            v.ФИО AS ФИО_воспитанника,
                            p.Название AS Название_программы
                        FROM Обучение_воспитанника ob
                        INNER JOIN Воспитанник v ON ob.Номер_личного_дела_воспитанника = v.Номер_личного_дела_воспитанника
                        INNER JOIN Программа p ON ob.Код_программы = p.Код_программы
                        ORDER BY ob.Год_обучения DESC, v.ФИО";

                    Debug.WriteLine("SQL запрос для загрузки данных обучения:");
                    Debug.WriteLine(query);

                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    connection.Open();
                    adapter.Fill(dt);

                    Debug.WriteLine($"Загружено записей обучения: {dt.Rows.Count}");
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА загрузки данных обучения: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА загрузки данных обучения: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
            return dt;
        }
        public int GetNextStudentCode()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT ISNULL(MAX(Код_обучамого_в_определенном_году), 0) + 1 FROM Обучение_воспитанника";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    connection.Open();
                    int result = Convert.ToInt32(cmd.ExecuteScalar());
                    Debug.WriteLine($"Сгенерирован следующий код обучаемого: {result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА генерации кода обучаемого: {ex.Message}");
                throw new Exception($"Ошибка генерации кода обучаемого: {ex.Message}", ex);
            }
        }

        public void DeleteEducationRecord(int studentCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Обучение_воспитанника WHERE Код_обучамого_в_определенном_году = @StudentCode";
                    Debug.WriteLine($"Удаление записи с кодом: {studentCode}");

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@StudentCode", studentCode);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    Debug.WriteLine($"Удалено записей: {rowsAffected}");
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMsg = $"SQL ОШИБКА удаления записи: {sqlEx.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMsg = $"ОШИБКА удаления записи: {ex.Message}";
                Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg, ex);
            }
        }
    }
}