using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Web_for_IotProject.Models;

namespace Web_for_IotProject.Data
{
    public class DeviceRepository
    {
        private readonly string _connectionString;
        public DeviceRepository(IConfiguration configuration) 
        {
            _connectionString = configuration.GetConnectionString("IotDatabaseConnection");
        }
        public void UpdateDeviceInfo(Device device )
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
            UPDATE DEVICES
            SET 
                DeviceName = @DeviceName,
                Location = @Location,
                IPAddress = @IPAddress,
                MACAddress = @MACAddress,
                Status = @Status,
                Height = @Height,
                Width = @Width,
                Quality = @Quality,
                FPS = @FPS,               
                LastSeen = @LastSeen
                
            WHERE DeviceID = @DeviceID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DeviceID", device.DeviceId);
                command.Parameters.AddWithValue("@DeviceName", device.DeviceName);
                command.Parameters.AddWithValue("@Location", device.Location);
                command.Parameters.AddWithValue("@IPAddress", device.IpAddress);
                command.Parameters.AddWithValue("@MACAddress", device.MacAddress);
                command.Parameters.AddWithValue("@Status", device.Status);
                command.Parameters.AddWithValue("@Height", device.Height);
                command.Parameters.AddWithValue("@Width", device.Width);
                command.Parameters.AddWithValue("@Quality", device.Quality);
                command.Parameters.AddWithValue("@FPS", device.FPS);
                command.Parameters.AddWithValue("@LastSeen", device.LastSeen);
                
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<Device> GetDevice() //// Need to fix the list of devices
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM DEVICES ";
                var devices = new List<Device>();

                SqlCommand command = new SqlCommand(query, connection);
                
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var device = new Device
                        {
                            DeviceId = reader["DeviceID"].ToString(),
                            DeviceName = reader["DeviceName"].ToString(),
                            Location = reader["Location"].ToString(),
                            IpAddress = reader["IPAddress"].ToString(),
                            MacAddress = reader["MACAddress"].ToString(),
                            Status = Convert.ToBoolean(reader["Status"]),
                            Height = Convert.ToInt32(reader["Height"]),
                            Width = Convert.ToInt32(reader["Width"]),
                            Quality = Convert.ToInt32(reader["Quality"]),
                            FPS = Convert.ToInt32(reader["FPS"]),
                            LastSeen = Convert.ToDateTime(reader["LastSeen"]).ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        devices.Add(device);
                    }
                }
                return devices;
            }
        }
    }
}
