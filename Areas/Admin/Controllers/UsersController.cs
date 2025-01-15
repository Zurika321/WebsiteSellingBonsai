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

        public UsersController(MiniBonsaiDBAPI context, APIServices apiServices)
        {
            _context = context;
            _apiServices = apiServices;
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
        public async Task<IActionResult> Sigin(RegisterModel sigin)
        {
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
            var (success, thongBao) = await _apiServices.FetchDataApiPost<ResetPassword>("Authenticate/ResetPassword", resetPassword);
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);

            return Redirect("/");
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
    }
}
