using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Renci.SshNet;
using Web_for_IotProject.Data;
using Web_for_IotProject.Models;
namespace Web_for_IotProject.Controllers
{
    public class SettingController : Controller
    {
        public readonly UserRepository _userRepository;
        private readonly SshService _sshService;
        private static string Password ="changeme";
        public SettingController(UserRepository userRepository, SshService sshService) 
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _sshService = sshService;
        }
        public async Task< IActionResult> Index(User user, string output=null)
        {
            var sessionId = HttpContext.Request.Cookies["SessionId"];
            if (sessionId ==null) return View("AccessDenied");
            bool isAdmin = await _userRepository.IsAdminFromSessionId(sessionId);
            var history = HttpContext.Session.GetString("TerminalHistory") ?? string.Empty;
            ViewBag.TerminalHistory = history;
            if (isAdmin)
            {
                ViewBag.Message = output; // Pass SSH command output to the view
                return View("Index");
            }

            else if (!isAdmin)
            {
                return View("AccessDenied");
            }
            else
            {
                return View("~/Views/Authenticate/MyLogin");
            }
            // Load history from session
            
        }

        //[HttpPost]
        //public IActionResult SendCommand(string command)
        //{
        //    if (string.IsNullOrWhiteSpace(command))
        //        return RedirectToAction("Index");

        //    var result = _sshService.ExecuteCommand(command);
        //    var output = result.Success ? result.Output : $"Error: {result.Error}";

        //    // Append to session history
        //    var previous = HttpContext.Session.GetString("TerminalHistory") ?? "";
        //    var newHistory = $"{previous}\n$ {command}\n{output}";
        //    HttpContext.Session.SetString("TerminalHistory", newHistory);

        //    return RedirectToAction("Index");
        //}
        public IActionResult SendCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return RedirectToAction("Index");

            var result = _sshService.ExecuteCommand(command); // No sudo needed
            var output = result.Success ? result.Output : $"Error: {result.Error}";

            var previous = HttpContext.Session.GetString("TerminalHistory") ?? "";
            var newHistory = $"{previous}\n$ {command}\n{output}";
            HttpContext.Session.SetString("TerminalHistory", newHistory);

            return RedirectToAction("Index");
        }


        //[HttpPost]
        //public IActionResult SendCommand(string command)
        //{
        //    if (string.IsNullOrWhiteSpace(command))
        //        return RedirectToAction("Index");

        //    bool requiresSudo = command.TrimStart().StartsWith("sudo");
        //    string finalCommand = command;

        //    if (requiresSudo)
        //    {
        //        // Inject password for sudo via -S (stdin)
        //        // Strip `sudo` to avoid `sudo sudo`
        //        var actualCommand = command.Trim().Replace("sudo", "").Trim();
        //        finalCommand = $"echo '{_sshService.password}' | sudo -S {actualCommand}";
        //    }

        //    var result = _sshService.ExecuteCommand(finalCommand, useRawCommand: true);
        //    var output = result.Success ? result.Output : $"Error: {result.Error}";

        //    // Append to session history
        //    var previous = HttpContext.Session.GetString("TerminalHistory") ?? "";
        //    var newHistory = $"{previous}\n$ {command}\n{output}";
        //    HttpContext.Session.SetString("TerminalHistory", newHistory);

        //    return RedirectToAction("Index");
        //}


        [HttpPost]
        public IActionResult ClearTerminal()
        {
            HttpContext.Session.Remove("TerminalHistory");
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Execute(string actionType)
        {
            string command = actionType switch
            {
                "connect" => "uptime",
                "camera_on" => "libcamera-vid -t 0 --inline --listen &",
                "camera_off" => "pkill libcamera-vid",
                "update" => "sudo apt update && sudo apt upgrade -y",
                _ => ""
            };

            if (string.IsNullOrEmpty(command))
                return RedirectToAction("Index", new { output = "Unknown action." });

            var result = _sshService.ExecuteCommand(command);
            var message = result.Success ? result.Output : $"Error: {result.Error}";
            ViewBag.Message=message;

            return RedirectToAction("Index", new { output = message });
        }



        public IActionResult Ping()
        {
            string ip = "192.168.31.212"; // Replace with your Raspberry Pi IP
            string message;

            try
            {
                using var ping = new Ping();
                PingReply reply = ping.Send(ip, 1000); // timeout = 1000ms

                if (reply.Status == IPStatus.Success)
                {
                    message = $"Ping successful: {reply.RoundtripTime} ms";
                }
                else
                {
                    message = $"Ping failed: {reply.Status}";
                }
            }
            catch (Exception ex)
            {
                message = $"Ping error: {ex.Message}";
            }

            return RedirectToAction("Index", new { output = message });
        }
    }
}
