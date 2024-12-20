using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;
using System.Numerics;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;

        public UsersController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: Admin/AdminUsers
        public async Task<IActionResult> Index()
        {
            var thongBao = new ThongBao
			{
                Message = "Thao tác thành công",
                MessageType = "Primary",
                DisplayTime = 5
            };

            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
            return View(await _context.AdminUser.ToListAsync());
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

        // GET: Admin/AdminUsers/Create
        public IActionResult Sigin()
        {
            return View();
        }

        // POST: Admin/AdminUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sigin(LoginDTO sigin)
        {
            if (ModelState.IsValid)
            {
                if (sigin.Password != sigin.ComfrimPassword)
                {
                    ViewData["Message"] = "Password and confirm password do not match!";
                    sigin.Password = "";
                    sigin.ComfrimPassword = "";
                    return View(sigin);
                }
                bool hasUserName = _context.AdminUser
                .Any(user => user.Username == sigin.Username);
                if (hasUserName)
                {
                    ViewData["Message"] = "username already exists!";
                    return View();
                }
                AdminUser adminuser = new AdminUser(){
                    Username = sigin.Username,
                    Password = sigin.Username,
                    Avatar = "Data/usernoimage.png",
                    Displayname = sigin.Username,
                    Address = "",
                    Email = "",
                    Phone = "",
                    Role = "User",
                };
                _context.Add(adminuser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            ViewData["Message"] = "Please enter full information!";

            return View();
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

        // POST: Admin/AdminUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginDTO login)
        {
            var result = await _context.AdminUser.AsTracking()
                .FirstOrDefaultAsync(x => x.Username == login.Username
                && x.Password == login.Password);
            if (result != null)
            {
                string mes;
                int time = 3;
                if (login.RememberMe)
                {
                    Response.Cookies.Append<LoginDTO>("CokieUserWebsiteSellingBonsai", login, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(1),
                        HttpOnly = true,
                        IsEssential = true,
                        //Secure = false
                    });
                    mes = "đăng nhập thành công! & Đã tạo cookie cho lần đăng nhập sau";
                    time = 5;
                }
                else
                {
                    mes = "đăng nhập thành công";
                }
                HttpContext.Session.Set<AdminUser>("userInfo", result);
                var thongBao = new ThongBao
                {
                    Message = mes,
                    MessageType = "Success",
                    DisplayTime = time
                };

                // Chuyển model sang TempData dưới dạng JSON
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);

                if (result.Role == "Admin") return RedirectToAction("Index", "Home", new { area = "Admin" });
                return RedirectToAction("", "", new { area = "Home" });
            }
            else
            {
                ViewData["Message"] = "Wrong username or password";
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

            if (HttpContext.Session.Get<AdminUser>("userInfo") != null)
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
