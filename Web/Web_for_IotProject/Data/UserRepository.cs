using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Web_for_IotProject.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Web_for_IotProject.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("IotDatabaseConnection");
        }


        // 🛠 Thêm User vào Database
        public void AddUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO USERS (ID, IsAdmin, Name, Email, Password) VALUES (@Id,@IsAdmin,@Name, @Email,@Password)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public User? AuthenticateUser(string email, string password)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT  ID, IsAdmin, Name, Email FROM USERS WHERE Email = @Email AND Password = @Password";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password); // Nên hash mật khẩu trong thực tế!

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {

                        Id = reader.GetInt32(0),
                        IsAdmin = reader.GetBoolean(1),
                        Name = reader.GetString(2),
                        Email = reader.GetString(3)
                    };
                }
            }
            return null; // Trả về null nếu sai tài khoản/mật khẩu
        }



        public User? AuthenticateUser(string userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT  ID, IsAdmin, Name, Email FROM USERS WHERE ID=@ID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", userId);
              
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {

                        Id = reader.GetInt32(0),
                        IsAdmin = reader.GetBoolean(1),
                        Name = reader.GetString(2),
                        Email = reader.GetString(3)
                    };
                }
            }
            return null; // Trả về null nếu sai tài khoản/mật khẩu
        }

        public bool IsSessionValid(string sessionID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                
                string query = "SELECT  SessionID FROM SESSION WHERE SessionID=@SessionID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SessionID", sessionID );

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    if (reader.GetString(0) == null)
                    {
                        return false;
                    }
                }
                return true;
            }

        }

        public string SessionGenerator(User user)
        {

            string sessionId = SecurityHelper.GenerateSessionId();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO SESSION (UserId, SessionID , IsAdmin, UserName) VALUES (@UserId, @SessionId, @IsAdmin, @UserName)";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserId", user.Id);
                cmd.Parameters.AddWithValue("@SessionId", sessionId);
                cmd.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);
                cmd.Parameters.AddWithValue("@UserName", user.Name);

                cmd.ExecuteNonQuery();

                

            }
            return sessionId;
        }

        public async Task<bool> IsAdminFromSessionId(string sessionId)
        {
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT IsAdmin  FROM SESSION WHERE SessionID = @SessionId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@SessionId", sessionId);

                    connection.Open();
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync()) 
                    {
                        return reader.GetBoolean(0); 
                    }

                    return false;
                }
            }
        }



        public int GetCurrentUserId()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT ID FROM userId";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }

                return 10000; // Trả về null nếu sai tài khoản/mật khẩu
            }
        }

        public bool UpdateCurrentUserId()
        {
            int newId = GetCurrentUserId() + 1;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE userId SET ID = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", newId);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();



            }
            return true;
        }


    }
}
