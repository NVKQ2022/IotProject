using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Web_for_IotProject.Models;
using Web_for_IotProject.Data;
namespace Web_for_IotProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserRepository _userRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        public HomeController(ILogger<HomeController> logger, UserRepository userRepository )
        {
            _logger = logger;
            _userRepository= userRepository;
        }
        //private readonly StreamRelay _relay;
        

        //public HomeController(StreamRelay relay, ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        //{
        //    _logger = logger;
        //    _httpClientFactory = httpClientFactory;
        //    _relay = relay;
        //}

        //[HttpGet("/camera-stream")]
        [HttpGet]
        public async Task Proxy1()
        {
            var cameraUrl = "http://172.31.10.153:8080/video"; // ip and port of the camera

            var client = _httpClientFactory.CreateClient();

            try
            {
                using var response = await client.GetAsync(cameraUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    Response.StatusCode = (int)response.StatusCode;
                    await Response.WriteAsync("Failed to connect to camera.");
                    return;
                }

                Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "multipart/x-mixed-replace";
                await response.Content.CopyToAsync(Response.Body);
            }
            catch (HttpRequestException ex)
            {
                Response.StatusCode = 502; // Bad Gateway
                await Response.WriteAsync($"Error connecting to camera: {ex.Message}");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                await Response.WriteAsync($"Unexpected error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task Proxy2()
        {
           // await _relay.AddClientAsync(Response);

        }
        public IActionResult Index()
        {
            var sessionId = HttpContext.Request.Cookies["SessionID"];
            if (sessionId == null) return View("LoginRequired");
            bool IsValid = _userRepository.IsSessionValid(sessionId);
            //bool isAdmin = await _userRepository.IsAdminFromSessionId(sessionId);
            var history = HttpContext.Session.GetString("TerminalHistory") ?? string.Empty;
            ViewBag.TerminalHistory = history;
            if (IsValid)
            {
                ViewBag.Message = ""; // Pass SSH command output to the view
                return View("Index");
            }

            else if (IsValid)
            {
                return View("LoginRequired");
            }
            else
            {
                return View("~/Views/Authenticate/MyLogin");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
