using System.Net.WebSockets;
using System;
using Microsoft.AspNetCore.Mvc;
using Web_for_IotProject.Data;

namespace Web_for_IotProject.Controllers
{
    [Route("ws/upload/{cameraId}")]
    public class WebSocketUploadController : ControllerBase
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
            var buffer = new byte[1024 * 1024];

            while (socket.State == WebSocketState.Open)
            {
                var ms = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage); 

                StreamRelay.UpdateFrame(cameraId, ms.ToArray());
            }
        }
    }
}
