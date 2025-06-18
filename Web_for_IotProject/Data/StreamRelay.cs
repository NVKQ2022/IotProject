using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace Web_for_IotProject.Data
{
    public class StreamRelay
    {
        //    //private readonly string cameraUrl = "http://192.168.153.196:8080/video";
        //    //private readonly List<Stream> clients = new();
        //    //private bool isRunning = false;

        //    //public async Task StartAsync()
        //    //{
        //    //    if (isRunning) return;
        //    //    isRunning = true;

        //    //    var httpClient = new HttpClient();
        //    //    var stream = await httpClient.GetStreamAsync(cameraUrl);
        //    //    var buffer = new byte[4096];
        //    //    int bytesRead;

        //    //    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //    //    {
        //    //        lock (clients)
        //    //        {
        //    //            foreach (var client in clients.ToList())
        //    //            {
        //    //                try
        //    //                {
        //    //                    client.Write(buffer, 0, bytesRead);
        //    //                    client.Flush(); // <-- VERY IMPORTANT
        //    //                }
        //    //                catch
        //    //                {
        //    //                    clients.Remove(client); // remove dead connection
        //    //                }
        //    //            }
        //    //        }
        //    //    }

        //    //}

        //    //public void AddClient(Stream responseStream)
        //    //{
        //    //    lock (clients) clients.Add(responseStream);
        //    //}

        //    //public void RemoveClient(Stream responseStream)
        //    //{
        //    //    lock (clients) clients.Remove(responseStream);
        //    //}
        //    private readonly string cameraUrl = "http://192.168.153.196:8080/video";
        //    private readonly ConcurrentBag<HttpResponse> clients = new();
        //    private readonly HttpClient httpClient = new();
        //    private bool isRunning = false;
        //    private byte[] latestFrame = null;
        //    private readonly object frameLock = new();

        //    public async Task AddClientAsync(HttpResponse response)
        //    {
        //        response.ContentType = "multipart/x-mixed-replace; boundary=--frame";
        //        clients.Add(response);

        //        byte[] frameToSend = null;

        //        lock (frameLock)
        //        {
        //            if (latestFrame != null)
        //                frameToSend = latestFrame.ToArray(); // copy outside the lock
        //        }

        //        if (frameToSend != null)
        //        {
        //            var header = "--frame\r\nContent-Type: image/jpeg\r\nContent-Length: " + frameToSend.Length + "\r\n\r\n";
        //            var headerBytes = Encoding.ASCII.GetBytes(header);

        //            await response.Body.WriteAsync(headerBytes, 0, headerBytes.Length);
        //            await response.Body.WriteAsync(frameToSend, 0, frameToSend.Length);
        //            await response.Body.WriteAsync(Encoding.ASCII.GetBytes("\r\n"));
        //            await response.Body.FlushAsync();
        //        }

        //        await StartAsync();
        //    }



        //    public async Task StartAsync()
        //    {
        //        if (isRunning) return;
        //        isRunning = true;

        //        var stream = await httpClient.GetStreamAsync(cameraUrl);
        //        var reader = new StreamReader(stream);

        //        var boundaryRegex = new Regex(@"--.*\r\nContent-Type: image/jpeg\r\nContent-Length: (\d+)\r\n\r\n", RegexOptions.Compiled);

        //        var buffer = new byte[4096];
        //        var memoryStream = new MemoryStream();

        //        while (true)
        //        {
        //            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        //            if (bytesRead <= 0) break;

        //            memoryStream.Write(buffer, 0, bytesRead);

        //            var data = memoryStream.ToArray();
        //            var dataString = System.Text.Encoding.ASCII.GetString(data);

        //            var match = boundaryRegex.Match(dataString);
        //            if (match.Success)
        //            {
        //                var frameStartIndex = match.Index + match.Length;
        //                int contentLength = int.Parse(match.Groups[1].Value);

        //                if (data.Length >= frameStartIndex + contentLength)
        //                {
        //                    var jpegData = data.Skip(frameStartIndex).Take(contentLength).ToArray();

        //                    // Store the latest frame
        //                    lock (frameLock)
        //                    {
        //                        latestFrame = jpegData;
        //                    }

        //                    var boundaryHeader = "--frame\r\nContent-Type: image/jpeg\r\nContent-Length: " + jpegData.Length + "\r\n\r\n";
        //                    var boundaryBytes = Encoding.ASCII.GetBytes(boundaryHeader);

        //                }
        //            }
        //        }
        //    }


        public static ConcurrentDictionary<string, byte[]> LatestFrames = new();
        public static void UpdateFrame(string cameraId, byte[] frame)
        {
            LatestFrames[cameraId] = frame;
        }

    }
}
