using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Web_for_IotProject.Data;
using Web_for_IotProject.Models;
using static Org.BouncyCastle.Math.EC.ECCurve;
namespace Web_for_IotProject.Controllers
{
    public class DeviceControlController : Controller
    {
        private readonly DeviceRepository _deviceRepository;
        private readonly string _configstring;
        public readonly UserRepository _userRepository;
        public DeviceControlController(DeviceRepository deviceRepository, IConfiguration configuration, UserRepository userRepository) 
        {
            _deviceRepository = deviceRepository;
            _configstring= configuration.GetConnectionString("IotDatabaseConnection");
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}


        public async Task<IActionResult> Index()
        {
            var sessionId = HttpContext.Request.Cookies["SessionId"];
            if (sessionId == null) return View("AccessDenied");
            bool isAdmin = await _userRepository.IsAdminFromSessionId(sessionId);
            if (isAdmin)
            {// Pass SSH command output to the view
                return View("Index");
            }

            else if (!isAdmin)
            {
                return View("AccessDenied");
            }
            else
            {
                return View("~/Views/Authenticate/MyLogin");
            }
            // Load history from session

        }


        [HttpPost]
        public async Task<IActionResult> SendCommand([FromBody] ControlCommand controlCommand)
        {
            if (controlCommand == null /*|| string.IsNullOrEmpty(controlCommand.DeviceId)*/)
            {
                Console.WriteLine($"DeviceId is required.: {controlCommand.DeviceId}");
                return BadRequest(new { error = "DeviceId is required." });

            }
               
            
                var cmd = new
            {
                command = controlCommand.Command,
                parameter = controlCommand.Parameter
            };

            var json = System.Text.Json.JsonSerializer.Serialize(cmd);
            await ControlRelay.SendCommandAsync(controlCommand.DeviceId, json);

            return Ok(new { status = "sent", command = controlCommand.Command });
        }
        
        [HttpGet]
        public IActionResult GetDeviceInfo(string deviceId)
        {
            // Simulated data — replace with real DB/service call later
            var devices = _deviceRepository.GetDevice();

            var device = devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device == null)
                return NotFound();
            return Ok(device);
        }




        public class UpdateDeviceSettingsModel
        {
            public string DeviceId { get; set; }
            public string DeviceName { get; set; }
            public string Location { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public int Quality { get; set; }
            public int Fps { get; set; }
        }
        [HttpPost]
        public IActionResult UpdateDeviceSettings([FromBody] UpdateDeviceSettingsModel model)
        {
            Device device = new Device
            {
                DeviceId = model.DeviceId,
                Location = model.Location,
                Height = model.Height,
                Width = model.Width,
                Quality = model.Quality,
                FPS = model.Fps,

            };
            using (var connection = new SqlConnection(_configstring))
            {
                var query = @"UPDATE DEVICES 
                      SET DeviceName = @DeviceName, Location = @Location, 
                          Height = @Height, Width = @Width, 
                          Quality = @Quality, FPS = @FPS 
                      WHERE DeviceID = @DeviceID";

                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@DeviceID", model.DeviceId);
                cmd.Parameters.AddWithValue("@DeviceName", model.DeviceName);
                cmd.Parameters.AddWithValue("@Location", model.Location);
                cmd.Parameters.AddWithValue("@Height", model.Height);
                cmd.Parameters.AddWithValue("@Width", model.Width);
                cmd.Parameters.AddWithValue("@Quality", model.Quality);
                cmd.Parameters.AddWithValue("@FPS", model.Fps);

                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true });
        }

        


    }
}
