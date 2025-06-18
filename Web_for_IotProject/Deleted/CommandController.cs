using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_for_IotProject.Data;

namespace Web_for_IotProject.Deleted
{
    // [Route("api/[controller]")]
    [ApiController]
    [Route("api/command")]
    public class CommandController : ControllerBase
    {
        [HttpPost("{deviceId}")]
        public async Task<IActionResult> SendCommand(string deviceId, [FromBody] object command)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(command);
            await ControlRelay.SendCommandAsync(deviceId, json);
            return Ok("Command sent.");
        }
    }
}
