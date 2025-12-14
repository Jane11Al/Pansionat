using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class DatabaseHelper
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["PansionatDBConnection"].ConnectionString;

        public DataTable LoadStudents()
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Воспитанник";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }
        public void AddStudent(int number, string fio, DateTime birthDate, string gender, string diagnosis)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO Воспитанник 
                (Номер_личного_дела_воспитанника, ФИО, Дата_рождения, Пол, Основной_диагноз) 
            VALUES 
                (@Numb, @FIO, @BirthDate, @Gender, @Diagnosis)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Numb", number);
                cmd.Parameters.AddWithValue("@FIO", fio);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteStudent(int number)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Воспитанник WHERE Номер_личного_дела_воспитанника = @Number";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Number", number);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public int GetMaxStudentNumber()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT MAX(Номер_личного_дела) FROM Воспитанник";
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        public void UpdateStudent(int currentNumber, string fio, DateTime birthDate, string gender, string diagnosis)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            UPDATE Воспитанник 
            SET 
                ФИО = @FIO,
                Дата_рождения = @BirthDate,
                Пол = @Gender,
                Основной_диагноз = @Diagnosis
            WHERE 
                Номер_личного_дела_воспитанника = @CurrentNumber";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@FIO", fio);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);
                cmd.Parameters.AddWithValue("@CurrentNumber", currentNumber);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private string GetFriendlyErrorMessage(SqlException ex)
        {
            switch (ex.Number)
            {
                case 18456:
                    return "Неверные учетные данные базы данных";
                case 4060:
                    return "Ошибка подключения к базе данных";
                case 208:
                    return "Несуществующая таблица";
                default:
                    return ex.Message;
            }
        }
        public bool IsStudentNumberExists(int number)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Воспитанник WHERE Номер_личного_дела_воспитанника = @Number";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Number", number);

                connection.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public void UpdateStudent(int originalNumber, string fio, DateTime birthDate, string gender, string diagnosis, int newNumber)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Воспитанник 
                SET 
                    Номер_личного_дела_воспитанника = @NewNumber,
                    ФИО = @FIO,
                    Дата_рождения = @BirthDate,
                    Пол = @Gender,
                    Основной_диагноз = @Diagnosis
                WHERE 
                    Номер_личного_дела_воспитанника = @OriginalNumber";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                cmd.Parameters.AddWithValue("@FIO", fio);
                cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);
                cmd.Parameters.AddWithValue("@OriginalNumber", originalNumber);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

}
