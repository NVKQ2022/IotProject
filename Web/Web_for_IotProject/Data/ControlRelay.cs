using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Web_for_IotProject.Data
{
    public class ControlRelay
    {
        public static ConcurrentDictionary<string, WebSocket> DeviceSockets = new();
        public static async Task SendCommandAsync(string cameraId, string message)
        {
            if (DeviceSockets.TryGetValue(cameraId, out var socket) && socket.State == WebSocketState.Open)
            {
                var data = System.Text.Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
