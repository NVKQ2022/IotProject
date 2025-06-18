using System.ServiceModel.Channels;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using Web_for_IotProject.Data;
using Web_for_IotProject.Models;
namespace Web_for_IotProject.Controllers
{
    public class AuthenticateController : Controller
    {
        //public IConfiguration configuration;
        //private UserRepository _userRepository= new UserRepository(configuration);
        private readonly UserRepository _userRepository;

        public AuthenticateController(UserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        [HttpGet]
        public IActionResult MyLogin()
        {
            return View();
        }
        public IActionResult MyRegister()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MyRegister(string email, string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Message = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Message = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            //string salt = SecurityHelper.GenerateSalt();
            //string hashedPassword = SecurityHelper.HashPassword(password, salt);

            var user = new User
            {
                Id = _userRepository.GetCurrentUserId()+1,
                Email = email,
                Name = username,
                Password = password,
                IsAdmin = true
            };

           


            try
            {
               
                _userRepository.AddUser(user);
                _userRepository.UpdateCurrentUserId();
                ViewBag.Message = "Đăng ký thành công! Bạn có thể đăng nhập.";
                return RedirectToAction("MyLogin");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Đăng ký thất bại: " + ex.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult Create(string name, string email)
        {
            var user = new User { Name = name, Email = email };
            return View(user);
        }

        [HttpPost]
        public IActionResult MyLogin(string email, string password)
        {
            
            User? user = _userRepository.AuthenticateUser(email, password);
            if (user != null)
            {
                ViewBag.Name = user.Name;
                HttpContext.Response.Cookies.Append("SessionID", _userRepository.SessionGenerator(user));
                return View("Index"); // need to make index
               
            }
            ViewBag.Message = "Sai thông tin!";
            
            return View("MyLogin");
        }
    }
}
