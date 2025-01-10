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
            var (adminuser , thongbao) = await _apiServices.FetchDataApiGetList<AdminUser>("AdminUsersAPI");
            if (adminuser != default)
            {
                return View(adminuser);
            }
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            adminuser = new List<AdminUser>();
            return View(adminuser);
        }

        // GET: Admin/AdminUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminUser = await _context.AdminUser
                .FirstOrDefaultAsync(m => m.USE_ID == id);
            if (adminUser == null)
            {
                return NotFound();
            }

            return View(adminUser);
        }

        // GET: Admin/AdminUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminUser = await _context.AdminUser.FindAsync(id);
            if (adminUser == null)
            {
                return NotFound();
            }
            return View(adminUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("USE_ID,Username,Password,Displayname,Email,Phone")] AdminUser adminUser)
        {
            if (id != adminUser.USE_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adminUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminUserExists(adminUser.USE_ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(adminUser);
        }

        // GET: Admin/AdminUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adminUser = await _context.AdminUser
                .FirstOrDefaultAsync(m => m.USE_ID == id);
            if (adminUser == null)
            {
                return NotFound();
            }

            return View(adminUser);
        }

        // POST: Admin/AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminUser = await _context.AdminUser.FindAsync(id);
            if (adminUser != null)
            {
                _context.AdminUser.Remove(adminUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdminUserExists(int id)
        {
            return _context.AdminUser.Any(e => e.USE_ID == id);
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
                    return View();
                }
            }
            ViewData["Message"] = "Vui lòng điền đầy đủ.";
            return View();
        }
        // GET: Admin/Users/Login
        [HttpGet]
        public async Task<ActionResult> Login()
        {
            var login = Request.Cookies.Get<LoginDTO>("CokieUserWebsiteSellingBonsai");
            if (login != null)
            {
                var result = await _context.AdminUser.AsTracking()
                            .FirstOrDefaultAsync(x => x.Username == login.Username
                            && x.Password == login.Password);
                if (result != null)
                {
                    HttpContext.Session.Set<AdminUser>("userInfo", result);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["Messenger"] = "cookie của bạn bị trùng tên với trang web khác hoặc mật khẩu đã bị thay đổi, vui lòng cập nhật";
                }
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
                return Redirect("/");
            }
            else
            {
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

            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
            if (success) return RedirectToAction("Login", "Users", new { area = "Admin" });
            return RedirectToAction("Index", "Home");
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

            if (HttpContext.Session.Get<ApplicationUser>("userInfo") != null)
            {
                HttpContext.Session.Remove("userInfo");
            }

            // Chuyển thông báo thành công qua TempData
            var thongBao = new ThongBao
            {
                Message = "Đã đăng xuất thành công",
                MessageType = "Succes",
                DisplayTime = 3
            };
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);

            // Redirect về trang đăng nhập
            return RedirectToAction("Login", "Users");
        }
    }
}
