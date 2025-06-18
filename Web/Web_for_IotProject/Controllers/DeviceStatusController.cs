using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_for_IotProject.Data;
using Web_for_IotProject.Models;
namespace Web_for_IotProject.Controllers
{
    [Route("api/info")]
    [ApiController]
    public class DeviceStatusController : ControllerBase
    {
        private readonly DeviceRepository _deviceRepository;
        public DeviceStatusController(DeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }


        [HttpPost("{deviceId}")]
        public IActionResult PostDeviceStatus([FromBody] Device device)
        {



            if (device == null)
                return BadRequest();


            // TODO: Update your Devices table using EF Core
            // Example pseudo-code:
            // var device = _context.Devices.FirstOrDefault(d => d.DeviceName == status.DeviceName);
            // if (device != null) { device.Status = status.Status; device.LastSeen = DateTime.UtcNow; ... }
            // _context.SaveChanges();






            Console.WriteLine($"{device.DeviceId}\n{device.DeviceName}\n{device.IpAddress}\n{device.MacAddress}\n{device.Location}\n{device.Height}\n{device.Width}\n{device.Quality}\n{device.FPS}");
            System.Diagnostics.Debug.WriteLine($"{device.DeviceId}\n{device.DeviceName}\n{device.IpAddress}\n{device.MacAddress}\n{device.Location}\n{device.Height}\n{device.Width}\n{device.Quality}\n{device.FPS}");
            _deviceRepository.UpdateDeviceInfo(device);
            return Ok(new { ack = true });

        }
    }
    
}
