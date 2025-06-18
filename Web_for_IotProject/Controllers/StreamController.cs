using Microsoft.AspNetCore.Mvc;
using NetMQ.Sockets;
using Web_for_IotProject.Data;
using NetMQ;
using Web_for_IotProject.Models;
namespace Web_for_IotProject.Controllers
{
    [Route("stream")]
    public class StreamController : Controller
    {



        // sending jpg to the human detection python script
        //[HttpGet("human/{cameraId}")]
        //public async Task Stream(string cameraId)
        //{
        //    //Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";

        //    //using (var zmqClient = new RequestSocket())
        //    //{
        //    //    zmqClient.Connect("tcp://localhost:5555"); // Connect to Python server

        //    //    while (true)
        //    //    {
        //    //        if (StreamRelay.LatestFrames.TryGetValue(cameraId, out var frame))
        //    //        {
        //    //            try
        //    //            {
        //    //                // Send the JPEG frame to Python server
        //    //                zmqClient.SendFrame(frame);

        //    //                // Receive the processed image back
        //    //                var processedFrame = zmqClient.ReceiveFrameBytes();

        //    //                // Stream to client
        //    //                await Response.WriteAsync("--frame\r\n");
        //    //                await Response.WriteAsync("Content-Type: image/jpeg\r\n\r\n");
        //    //                await Response.Body.WriteAsync(processedFrame);
        //    //                await Response.WriteAsync("\r\n");
        //    //                await Response.Body.FlushAsync();

        //    //                Console.WriteLine("[C# Client] Frame sent and response received.");
        //    //            }
        //    //            catch (Exception ex)
        //    //            {
        //    //                Console.WriteLine($"[C# Client] Error: {ex.Message}");
        //    //            }
        //    //        }

        //    //        await Task.Delay(20); // Adjust to your FPS needs
        //    //    }
        //    //}
        //    // Load the fallback image once
        //    var fallbackImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "camera_off.jpg");
        //    byte[] fallbackImage = await System.IO.File.ReadAllBytesAsync(fallbackImagePath);

        //    using (var zmqClient = new RequestSocket())
        //    {
        //        zmqClient.Connect("tcp://localhost:5555");

        //        while (true)
        //        {
        //            byte[] imageToSend;

        //            if (StreamRelay.LatestFrames.TryGetValue(cameraId, out var frame) && frame != null)
        //            {
        //                try
        //                {
        //                    zmqClient.SendFrame(frame);
        //                    imageToSend = zmqClient.ReceiveFrameBytes();

        //                    Console.WriteLine("[C# Client] Frame sent and response received.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"[C# Client] Error processing frame: {ex.Message}");
        //                    imageToSend = fallbackImage;
        //                }
        //            }
        //            else
        //            {
        //                imageToSend = fallbackImage;
        //            }

        //            // Stream the image (processed or fallback)
        //            await Response.WriteAsync("--frame\r\n");
        //            await Response.WriteAsync("Content-Type: image/jpeg\r\n\r\n");
        //            await Response.Body.WriteAsync(imageToSend);
        //            await Response.WriteAsync("\r\n");
        //            await Response.Body.FlushAsync();

        //            await Task.Delay(20);
        //        }
        //    }
        //}


        //[HttpGet("fire/{cameraId}")]
        //public async Task Stream(string cameraId)
        //{

        //    // Load the fallback image once
        //    var fallbackImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "camera_off.jpg");
        //    byte[] fallbackImage = await System.IO.File.ReadAllBytesAsync(fallbackImagePath);

        //    using (var zmqClient = new RequestSocket())
        //    {
        //        zmqClient.Connect("tcp://localhost:5556");

        //        while (true)
        //        {
        //            byte[] imageToSend;

        //            if (StreamRelay.LatestFrames.TryGetValue(cameraId, out var frame) && frame != null)
        //            {
        //                try
        //                {
        //                    zmqClient.SendFrame(frame);
        //                    imageToSend = zmqClient.ReceiveFrameBytes();

        //                    Console.WriteLine("[C# Client] Frame sent and response received.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"[C# Client] Error processing frame: {ex.Message}");
        //                    imageToSend = fallbackImage;
        //                }
        //            }
        //            else
        //            {
        //                imageToSend = fallbackImage;
        //            }

        //            // Stream the image (processed or fallback)
        //            await Response.WriteAsync("--frame\r\n");
        //            await Response.WriteAsync("Content-Type: image/jpeg\r\n\r\n");
        //            await Response.Body.WriteAsync(imageToSend);
        //            await Response.WriteAsync("\r\n");
        //            await Response.Body.FlushAsync();

        //            await Task.Delay(20);
        //        }
        //    }
        //}
        [HttpGet("{type}/{cameraId}")]
        public async Task Stream(string type, string cameraId)
        {
            Console.WriteLine($"{type}/ {cameraId} ");
            System.Diagnostics.Debug.WriteLine($"{type}/ {cameraId} ");
            Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";
            int port = type == "fire" ? 5556 : 5555;
            using (var zmqClient = new RequestSocket())
            {
                zmqClient.Connect($"tcp://localhost:{port}"); // Connect to Python server

                while (true)
                {
                    if (StreamRelay.LatestFrames.TryGetValue(cameraId, out var frame))
                    {
                        try
                        {
                            // Send the JPEG frame to Python server
                            zmqClient.SendFrame(frame);

                            // Receive the processed image back
                            var processedFrame = zmqClient.ReceiveFrameBytes();

                            // Stream to client
                            await Response.WriteAsync("--frame\r\n");
                            await Response.WriteAsync("Content-Type: image/jpeg\r\n\r\n");
                            await Response.Body.WriteAsync(processedFrame);
                            await Response.WriteAsync("\r\n");
                            await Response.Body.FlushAsync();

                            Console.WriteLine("[C# Client] Frame sent and response received.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[C# Client] Error: {ex.Message}");
                        }
                    }

                    await Task.Delay(20); // Adjust FPS
                }
            }
        }
























        [HttpGet("{cameraId}")]
        public async Task Stream(string cameraId)
        {
            Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";

            using (var zmqClient = new RequestSocket())
            {
                zmqClient.Connect("tcp://localhost:5555"); // Connect to Python server

                while (true)
                {
                    if (StreamRelay.LatestFrames.TryGetValue(cameraId, out var frame))
                    {
                        try
                        {
                            // Send the JPEG frame to Python server
                            zmqClient.SendFrame(frame);

                            // Receive the processed image back
                            var processedFrame = zmqClient.ReceiveFrameBytes();

                            // Stream to client
                            await Response.WriteAsync("--frame\r\n");
                            await Response.WriteAsync("Content-Type: image/jpeg\r\n\r\n");
                            await Response.Body.WriteAsync(processedFrame);
                            await Response.WriteAsync("\r\n");
                            await Response.Body.FlushAsync();

                            Console.WriteLine("[C# Client] Frame sent and response received.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[C# Client] Error: {ex.Message}");
                        }
                    }

                    await Task.Delay(20); // Adjust to your FPS needs
                }
            }

        }
    }
}
