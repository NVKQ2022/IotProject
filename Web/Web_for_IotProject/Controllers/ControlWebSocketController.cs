using System.Net.WebSockets;
using System;
using Microsoft.AspNetCore.Mvc;
using Web_for_IotProject.Data;

namespace Web_for_IotProject.Controllers
{
    [Route("ws/control/{cameraId}")]
    public class ControlWebSocketController : ControllerBase
    {
        [HttpGet]
        public async Task Get(string cameraId)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            ControlRelay.DeviceSockets[cameraId] = socket;

            var buffer = new byte[4096];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"[Device {cameraId}] {message}");
                System.Diagnostics.Debug.WriteLine($"[Device {cameraId}] {message}");
            }

            ControlRelay.DeviceSockets.TryRemove(cameraId, out _);
        }
    }
}