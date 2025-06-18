using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System;

namespace Web_for_IotProject.Data
{
    public class UdpFrameReceiver: BackgroundService
    {
        private readonly int port = 5005; // UDP port
        public static ConcurrentDictionary<string, byte[]> LatestFrames = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var udpClient = new UdpClient(port);
            var endPoint = new IPEndPoint(IPAddress.Any, port);

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                var packet = result.Buffer;

                // CameraId prefix (first 8 bytes = ASCII)
                string cameraId = System.Text.Encoding.ASCII.GetString(packet, 0, 8).Trim();
                byte[] image = packet[8..];

                LatestFrames[cameraId] = image;
            }
        }
    }
}
