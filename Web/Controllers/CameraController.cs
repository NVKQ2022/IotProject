using Microsoft.AspNetCore.Mvc;
using Web_for_IotProject.Data;

namespace Web_for_IotProject.Controllers
{
    public class CameraController : Controller
    {
        public readonly UserRepository _userRepository;
        public CameraController(UserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IActionResult> CameraView()
        {
            var sessionId = HttpContext.Request.Cookies["SessionId"];
            if (sessionId == null) return View("AccessDenied");
            bool isAdmin =/* await _userRepository.IsAdminFromSessionId(sessionId);*/  _userRepository.IsSessionValid(sessionId);
            if (isAdmin)
            {
                
                return View();
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

        [HttpGet("/pi-stream")]
        public async Task Stream()
        {

        }
    }
}
