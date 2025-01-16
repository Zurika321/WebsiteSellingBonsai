using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;
using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using WebsiteSellingBonsaiAPI.DTOS.User;
using System.Net.Http;
using Azure;
using Microsoft.AspNetCore.Identity.UI.Services;
using NuGet.Common;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;
        private readonly APIServices _apiServices;
        private readonly ICaptchaService _captchaService;


        public UsersController(MiniBonsaiDBAPI context, APIServices apiServices, ICaptchaService captchaService)
        {
            _context = context;
            _apiServices = apiServices;
            _captchaService = captchaService;
        }

        // GET: Admin/AdminUsers
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "UserManager", new { area = "Admin" });
        }
        public IActionResult Sigin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Sigin(RegisterModel sigin ,string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse))
            {
                ViewData["Message"] = "reCAPTCHA không hợp lệ.";
                return View(sigin);
            }

            var isCaptchaValid = await _captchaService.VerifyCaptchaAsync(captchaResponse);
            if (!isCaptchaValid)
            {
                ViewData["Message"] = "Xác thực reCAPTCHA thất bại.";
                return View(sigin);
            }

            if (ModelState.IsValid)
            {
                if (sigin.Password != sigin.ComfrimPassword)
                {
                    ViewData["Message"] = "mật khẩu và mật khẩu xác nhận không khớp";
                    sigin.Password = "";
                    sigin.ComfrimPassword = "";
                    return View(sigin);
                }
                var (success, thongBao) = await _apiServices.Register(sigin);

                ViewData["Message"] = thongBao.Message;
                if (success)
                {
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    return View(sigin);
                }
            }
            ViewData["Message"] = "Vui lòng điền đầy đủ.";
            return View(sigin);
        }
        // GET: Admin/Users/Login
        [HttpGet]
        public async Task<ActionResult> Login()
        {
            var returnUrl = HttpContext.Session.GetString("ReturnUrl");
            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewData["nexturl"] = $"and go to the page {returnUrl}";
            }
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel login)
        {
            if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                ViewData["Message"] = "Vui lòng điền đầy đủ";
                return View();

            }
            var (success, thongBao, token) = await _apiServices.Login(login);
            var returnUrl = HttpContext.Session.GetString("ReturnUrl");
            if (success)
            {
                // Lưu token vào cookie nếu đăng nhập thành công và RememberMe được chọn
                if (login.RememberMe)
                {
                    Response.Cookies.Append<LoginModel>("CokieUserWebsiteSellingBonsai", login, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(1),
                        HttpOnly = true,
                        IsEssential = true,
                        //Secure = false
                    });
                    thongBao.Message = "Đăng nhập thành công! & Đã tạo cookie cho lần đăng nhập sau";
                }
                
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    ViewData["nexturl"] = $"and go to the page {returnUrl}";
                }
                ViewData["Message"] = thongBao.Message;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmail comfrim)
        {
            if (comfrim != null && string.IsNullOrEmpty(comfrim.userId) || string.IsNullOrEmpty(comfrim.token))
            {
                return BadRequest("Invalid user ID or token.");
            }

            var (success, thongBao) = await _apiServices.FetchDataApiPost<ConfirmEmail>("Authenticate/ConfirmEmail" ,comfrim);

            //TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
            ViewData["thongbao"] = thongBao.Message;
            if (success)
            {
                ViewData["Success"] = true;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewData["Message"] = "Vui lòng nhập email để gửi xác nhận về email";
                return View();
            }
            var (success, thongBao) = await _apiServices.FetchDataApiPost("Authenticate/ForgotPassword", email);
            ViewData["Message"] = thongBao.Message;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string userId,string token)
        {
            var model = new ResetPassword
            {
                newpassword = "",
                Comfirmpassword = "",
                userid = userId,
                token = token,
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            if (string.IsNullOrEmpty(resetPassword.newpassword) || string.IsNullOrEmpty(resetPassword.Comfirmpassword))
            {
                ViewData["Message"] = "Vui lòng nhập đầy đủ password và password comfirm";
                return View();
            }
            if (string.IsNullOrEmpty(resetPassword.token) || string.IsNullOrEmpty(resetPassword.userid))
            {
                ViewData["Message"] = "Không xác nhận được người thay đổi mật khẩu";
                return View();
            }
            if (ModelState.IsValid)
            {
                var (success, thongBao) = await _apiServices.FetchDataApiPost<ResetPassword>("Authenticate/ResetPassword", resetPassword);
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);

                return Redirect("/");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            if (Request.Cookies.ContainsKey("CokieUserWebsiteSellingBonsai"))
            {
                Response.Cookies.Delete("CokieUserWebsiteSellingBonsai");
            }

            if (HttpContext.Session.Get("AuthToken") != null)
            {
                HttpContext.Session.Remove("AuthToken");
            }

            if (HttpContext.Session.Get<ApplicationUserDTO>("userInfo") != null)
            {
                HttpContext.Session.Remove("userInfo");
            }

            // Chuyển thông báo thành công qua TempData
            var thongBao = new ThongBao
            {
                Message = "Đã đăng xuất thành công",
                MessageType = TypeThongBao.Success,
                DisplayTime = 3
            };
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);

            // Redirect về trang đăng nhập
            return RedirectToAction("Login", "Users");
        }
        //public async Task<bool> VerifyCaptchaAsync(string captchaResponse)
        //{
        //    var secretKey = "6LcM7bcqAAAAAAhCqRTR6JyRTINm-5rcJfRboe9E"; // Thay bằng secret key của bạn
        //    var client = new HttpClient();
        //    var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}", null);
        //    var jsonString = await response.Content.ReadAsStringAsync();
        //    dynamic jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
        //    return jsonData.success == true;
        //}
    }
}
