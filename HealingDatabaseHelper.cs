using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;

namespace Pasionat
{
    public class HealingDatabaseHelper
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PansionatDBConnection"].ConnectionString;

        // ============ МЕТОДЫ ДЛЯ ВОСПИТАННИКОВ ============
        public string ConnectionString
        {
            get { return connectionString; }
        }

        // Альтернативно можно сделать метод:
        public string GetConnectionString()
        {
            return connectionString;
        }
        public DataTable LoadStudents()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        Номер_личного_дела_воспитанника,
                        ФИО,
                        Дата_рождения,
                        Пол,
                        Основной_диагноз
                    FROM Воспитанник 
                    ORDER BY ФИО";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadStudentNames()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT DISTINCT ФИО FROM Воспитанник ORDER BY ФИО";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadStudentById(int studentId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        Номер_личного_дела_воспитанника,
                        ФИО,
                        Дата_рождения,
                        Пол,
                        Основной_диагноз
                    FROM Воспитанник 
                    WHERE Номер_личного_дела_воспитанника = @StudentId";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }
        // МЕТОД ДЛЯ ЗАГРУЗКИ НАЗНАЧЕНИЙ КОНКРЕТНОГО ВОСПИТАННИКА
        public DataTable LoadAppointmentsByStudent(int studentId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                           н.Код_назначения,
                           н.Название_препарата,
                           н.Номер_личного_дела_воспитанника,
                           н.Дата_назначения,
                           в.ФИО as ФИО_воспитанника
                       FROM Назначение н
                       INNER JOIN Воспитанник в ON н.Номер_личного_дела_воспитанника = в.Номер_личного_дела_воспитанника
                       WHERE н.Номер_личного_дела_воспитанника = @StudentId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;

                        DataTable dataTable = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки назначений воспитанника: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // МЕТОД ДЛЯ ПРОВЕРКИ СУЩЕСТВОВАНИЯ НАЗНАЧЕНИЯ
        public bool IsAppointmentExists(int appointmentCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Назначение WHERE Код_назначения = @AppointmentCode";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@AppointmentCode", SqlDbType.Int).Value = appointmentCode;

                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка проверки назначения: {ex.Message}");
                return false;
            }
        }
        // ПЕРЕГРУЖЕННЫЙ МЕТОД ДОБАВЛЕНИЯ НАЗНАЧЕНИЯ БЕЗ ПАРАМЕТРА appointmentCode
        public void AddAppointment(string drugName, int studentId, DateTime appointmentDate)
        {
            try
            {
                // Получаем следующий доступный код назначения
                int nextAppointmentCode = GetNextAppointmentCode();

                // Вызываем основной метод с сгенерированным кодом
                AddAppointment(nextAppointmentCode, drugName, studentId, appointmentDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления назначения: {ex.Message}");
            }
        }
        // МЕТОД ДОБАВЛЕНИЯ НАЗНАЧЕНИЯ
        public void AddAppointment(int appointmentCode, string drugName, int studentId, DateTime appointmentDate)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Если передан -1, генерируем код автоматически
                    int actualAppointmentCode = appointmentCode;
                    if (appointmentCode == -1)
                    {
                        actualAppointmentCode = GetNextAppointmentCode();
                    }

                    string query = @"INSERT INTO Назначение (Код_назначения, Название_препарата, Номер_личного_дела_воспитанника, Дата_назначения) 
                           VALUES (@AppointmentCode, @DrugName, @StudentId, @AppointmentDate)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@AppointmentCode", SqlDbType.Int).Value = actualAppointmentCode;
                        command.Parameters.Add("@DrugName", SqlDbType.VarChar, 50).Value = drugName;
                        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
                        command.Parameters.Add("@AppointmentDate", SqlDbType.Date).Value = appointmentDate;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления назначения: {ex.Message}");
            }
        }


        // ИЗМЕНЕННЫЙ МЕТОД ОБНОВЛЕНИЯ НАЗНАЧЕНИЯ
        public void UpdateAppointment(int appointmentCode, string drugName, int studentId, DateTime appointmentDate)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // ПРОВЕРЯЕМ СУЩЕСТВОВАНИЕ НАЗНАЧЕНИЯ ПЕРЕД ОБНОВЛЕНИЕМ
                    if (!IsAppointmentExists(appointmentCode))
                    {
                        throw new Exception($"Назначение с кодом {appointmentCode} не найдено");
                    }

                    string query = @"UPDATE Назначение 
                   SET Название_препарата = @DrugName, 
                       Номер_личного_дела_воспитанника = @StudentId, 
                       Дата_назначения = @AppointmentDate
                   WHERE Код_назначения = @AppointmentCode";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@AppointmentCode", SqlDbType.Int).Value = appointmentCode;
                        command.Parameters.Add("@DrugName", SqlDbType.VarChar, 50).Value = drugName;
                        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
                        command.Parameters.Add("@AppointmentDate", SqlDbType.Date).Value = appointmentDate;

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception("Назначение не найдено для обновления (после проверки)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления назначения: {ex.Message}");
            }
        }
        public void DeleteAppointment(int appointmentCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Назначение WHERE Код_назначения = @AppointmentCode";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@AppointmentCode", SqlDbType.Int).Value = appointmentCode;

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception("Назначение не найдено для удаления");
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new Exception("Нельзя удалить назначение, так как на него есть ссылки в других таблицах");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка удаления назначения: {ex.Message}");
            }
        }

        // ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ПОЛУЧЕНИЯ СЛЕДУЮЩЕГО КОДА НАЗНАЧЕНИЯ
        private int GetNextAppointmentCode()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ISNULL(MAX(Код_назначения), 0) + 1 FROM Назначение";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения следующего кода назначения: {ex.Message}");
                return 1; // Возвращаем значение по умолчанию
            }
        }

        // ============ МЕТОДЫ ДЛЯ ПРЕПАРАТОВ ============

        public DataTable LoadDrugs()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        Название_препарата,
                        Дозировка_в_мг
                    FROM Препарат 
                    ORDER BY Название_препарата";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void AddDrug(string drugName, string dosage)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Препарат 
                        (Название_препарата, Дозировка_в_мг) 
                    VALUES 
                        (@DrugName, @Dosage)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DrugName", drugName);
                cmd.Parameters.AddWithValue("@Dosage", dosage);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteDrug(string drugName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Препарат WHERE Название_препарата = @DrugName";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DrugName", drugName);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ============ МЕТОДЫ ДЛЯ НАЗНАЧЕНИЙ ============

        public DataTable LoadAppointments()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        н.Код_назначения,
                        н.Название_препарата,
                        н.Номер_личного_дела_воспитанника,
                        в.ФИО,
                        н.Дата_назначения
                    FROM Назначение н
                    LEFT JOIN Воспитанник в ON н.Номер_личного_дела_воспитанника = в.Номер_личного_дела_воспитанника
                    ORDER BY н.Дата_назначения DESC";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

             
        public DataTable LoadMedicalEquipmentByStudent(int studentId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                           м.Инвентарный_номер_оборудования,
                           м.Название,
                           м.Номер_личного_дела_воспитанника,
                           м.Дата_выдачи,
                           в.ФИО as ФИО_воспитанника
                       FROM Мед_оборудование м
                       LEFT JOIN Воспитанник в ON м.Номер_личного_дела_воспитанника = в.Номер_личного_дела_воспитанника
                       WHERE м.Номер_личного_дела_воспитанника = @StudentId 
                          OR м.Номер_личного_дела_воспитанника IS NULL";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;

                        DataTable dataTable = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оборудования воспитанника: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Метод для загрузки только свободного оборудования
        public DataTable LoadFreeMedicalEquipment()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                           Инвентарный_номер_оборудования,
                           Название
                       FROM Мед_оборудование 
                       WHERE Номер_личного_дела_воспитанника IS NULL";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки свободного оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        // ============ МЕТОДЫ ДЛЯ МЕД ОБОРУДОВАНИЯ ============

        public DataTable LoadMedicalEquipment()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        м.Инвентарный_номер_оборудования,
                        м.Название,
                        м.Номер_личного_дела_воспитанника,
                        в.ФИО,
                        м.Дата_выдачи
                    FROM Мед_оборудование м
                    LEFT JOIN Воспитанник в ON м.Номер_личного_дела_воспитанника = в.Номер_личного_дела_воспитанника
                    ORDER BY м.Инвентарный_номер_оборудования";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadAvailableMedicalEquipment()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        Инвентарный_номер_оборудования,
                        Название
                    FROM Мед_оборудование 
                    WHERE Номер_личного_дела_воспитанника IS NULL
                    ORDER BY Название";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

      

        public void AddMedicalEquipment(string name)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Мед_оборудование 
                        (Название) 
                    VALUES 
                        (@Name)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Name", name);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ИСПРАВЛЕННЫЙ МЕТОД AttachEquipmentToStudent (убрано лишнее сообщение)
        public bool AttachEquipmentToStudent(int inventoryNumber, int studentId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем, не привязано ли оборудование уже к другому воспитаннику
                    string checkQuery = @"SELECT Номер_личного_дела_воспитанника 
                                FROM Мед_оборудование 
                                WHERE Инвентарный_номер_оборудования = @InventoryNumber";

                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.Add("@InventoryNumber", SqlDbType.Int).Value = inventoryNumber;
                        var result = checkCommand.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            int currentStudentId = Convert.ToInt32(result);
                            if (currentStudentId != studentId)
                            {
                                MessageBox.Show("Это оборудование уже привязано к другому воспитаннику!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }
                            else
                            {
                                // УБРАНО: MessageBox.Show("Оборудование уже привязано к этому воспитаннику!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return true;
                            }
                        }
                    }

                    // Привязываем оборудование
                    string query = @"UPDATE Мед_оборудование 
                           SET Номер_личного_дела_воспитанника = @StudentId, 
                               Дата_выдачи = @IssueDate
                           WHERE Инвентарный_номер_оборудования = @InventoryNumber";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
                        command.Parameters.Add("@IssueDate", SqlDbType.Date).Value = DateTime.Now;
                        command.Parameters.Add("@InventoryNumber", SqlDbType.Int).Value = inventoryNumber;

                        int rowsAffected = command.ExecuteNonQuery();
                        Debug.WriteLine($"Оборудование привязано. Записей обновлено: {rowsAffected}");

                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL Ошибка при привязке оборудования: {ex.Message}");
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Общая ошибка при привязке оборудования: {ex.Message}");
                MessageBox.Show($"Ошибка при привязке оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Метод для отвязки оборудования от воспитанника
        public bool DetachEquipment(int inventoryNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"UPDATE Мед_оборудование 
                           SET Номер_личного_дела_воспитанника = NULL, 
                               Дата_выдачи = NULL
                           WHERE Инвентарный_номер_оборудования = @InventoryNumber";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@InventoryNumber", SqlDbType.Int).Value = inventoryNumber;

                        int rowsAffected = command.ExecuteNonQuery();
                        Debug.WriteLine($"Оборудование отвязано. Записей обновлено: {rowsAffected}");

                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL Ошибка при отвязке оборудования: {ex.Message}");
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Общая ошибка при отвязке оборудования: {ex.Message}");
                MessageBox.Show($"Ошибка при отвязке оборудования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Метод для проверки привязано ли оборудование
        public bool IsEquipmentAttached(int inventoryNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) 
                           FROM Мед_оборудование 
                           WHERE Инвентарный_номер_оборудования = @InventoryNumber 
                           AND Номер_личного_дела_воспитанника IS NOT NULL";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@InventoryNumber", SqlDbType.Int).Value = inventoryNumber;

                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка проверки привязки оборудования: {ex.Message}");
                return false;
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

        // ============ МЕТОДЫ ДЛЯ ДИАГНОЗОВ ============
        public DataTable LoadDiagnoses()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Код_заболевания, Название FROM Диагноз ORDER BY Название";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable LoadStudentDiagnosesByStudent(int studentId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                ДВ.Номер_личного_дела_воспитанника,
                В.ФИО AS ФИО_воспитанника,
                ДВ.Код_заболевания,
                Д.Название AS Название_диагноза
            FROM Диагноз_воспитанника ДВ
            INNER JOIN Воспитанник В ON ДВ.Номер_личного_дела_воспитанника = В.Номер_личного_дела_воспитанника
            INNER JOIN Диагноз Д ON ДВ.Код_заболевания = Д.Код_заболевания
            WHERE ДВ.Номер_личного_дела_воспитанника = @StudentId
            ORDER BY Д.Название";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void AddStudentDiagnosis(int studentId, string diseaseCode)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO Диагноз_воспитанника 
                (Код_заболевания, Номер_личного_дела_воспитанника) 
            VALUES 
                (@DiseaseCode, @StudentId)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DiseaseCode", diseaseCode);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        

        public DataTable LoadStudentDiagnoses()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                ДВ.Номер_личного_дела_воспитанника,
                В.ФИО AS ФИО_воспитанника,
                ДВ.Код_заболевания,
                Д.Название AS Название_диагноза
            FROM Диагноз_воспитанника ДВ
            INNER JOIN Воспитанник В ON ДВ.Номер_личного_дела_воспитанника = В.Номер_личного_дела_воспитанника
            INNER JOIN Диагноз Д ON ДВ.Код_заболевания = Д.Код_заболевания
            ORDER BY В.ФИО, Д.Название";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

       
        public void AddDiagnosis(string name)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Диагноз 
                        (Название) 
                    VALUES 
                        (@Name)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Name", name);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        // Дополнительный метод для проверки существования записи
        public bool CheckStudentDiagnosisExists(int studentId, string diseaseCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT COUNT(*) 
                FROM [Диагноз_воспитанника] 
                WHERE [Номер_личного_дела_воспитанника] = @StudentId 
                AND [Код_заболевания] = @DiseaseCode";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
                        command.Parameters.Add("@DiseaseCode", SqlDbType.VarChar, 20).Value = diseaseCode;

                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка проверки существования записи: {ex.Message}");
                return false;
            }
        }
        public bool DeleteStudentDiagnosis(int studentId, string diseaseCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Простой прямой запрос
                    string query = "DELETE FROM Диагноз_воспитанника WHERE Номер_личного_дела_воспитанника = @StudentId AND Код_заболевания = @DiseaseCode";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;
                        command.Parameters.Add("@DiseaseCode", SqlDbType.VarChar).Value = diseaseCode;

                        int rowsAffected = command.ExecuteNonQuery();
                        Debug.WriteLine($"Прямое удаление: StudentId={studentId}, DiseaseCode='{diseaseCode}', RowsAffected={rowsAffected}");

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка прямого удаления: {ex.Message}");
                return false;
            }
        }

        public bool IsStudentExists(int studentId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Воспитанник WHERE Номер_личного_дела_воспитанника = @StudentId";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public bool IsDrugExists(string drugName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Препарат WHERE Название_препарата = @DrugName";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DrugName", drugName);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public bool IsDiagnosisExists(int diseaseCode)
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

        public bool IsEquipmentExists(int inventoryNumber)
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
        public DataTable LoadStudentsWithDiagnoses()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT DISTINCT 
                В.Номер_личного_дела_воспитанника,
                В.ФИО
            FROM Воспитанник В
            INNER JOIN Диагноз_воспитанника ДВ ON В.Номер_личного_дела_воспитанника = ДВ.Номер_личного_дела_воспитанника
            ORDER BY В.ФИО";

                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }
        

        // ============ ОБРАБОТКА ОШИБОК ============

        private string GetFriendlyErrorMessage(SqlException ex)
        {
            switch (ex.Number)
            {
                case 2627: // Violation of primary key constraint
                    return "Ошибка: Запись с таким идентификатором уже существует";
                case 547: // Foreign key violation
                    return "Ошибка: Нельзя удалить запись, так как на нее есть ссылки в других таблицах";
                case 18456: // Login failed
                    return "Ошибка: Неверные учетные данные базы данных";
                case 4060: // Cannot open database
                    return "Ошибка подключения к базе данных";
                case 208: // Invalid object name
                    return "Ошибка: Несуществующая таблица";
                default:
                    return $"Ошибка базы данных: {ex.Message}";
            }
        }
    }
}