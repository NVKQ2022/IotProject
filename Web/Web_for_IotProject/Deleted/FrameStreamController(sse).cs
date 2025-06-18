using System;
using Microsoft.AspNetCore.Mvc;
using Web_for_IotProject.Data;

namespace Web_for_IotProject.Deleted
{
    [Route("sse")]
    public class FrameStreamController_sse_ : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet("{cameraId}")]
        public async Task Stream(string cameraId)
        {
            Response.ContentType = "text/event-stream";
            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                if (UdpFrameReceiver.LatestFrames.TryGetValue(cameraId, out var frame))
                {
                    var base64 = Convert.ToBase64String(frame);
                    await Response.WriteAsync($"data:image/jpeg;base64,{base64}\n\n");
                    await Response.Body.FlushAsync();
                }

                await Task.Delay(100); // ~10 FPS
            }
        }
    }
}