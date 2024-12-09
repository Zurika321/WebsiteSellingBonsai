using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteSellingMiniBonsai.Areas.Admin.DTOS;
using WebsiteSellingMiniBonsai.Areas.Admin.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingMiniBonsai.Models;

namespace WebsiteSellingMiniBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly MiniBonsaiDB _context;

        public UsersController(MiniBonsaiDB context)
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("USE_ID,Username,Password,Displayname,Email,Phone")] AdminUser adminUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(adminUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
            var login = Request.Cookies.Get<LoginDTO>("UserCredential");
            if (login != null)
            {
                var result = await _context.AdminUser.AsTracking()
                            .FirstOrDefaultAsync(x => x.Username == login.Username
                            && x.Password == login.Password);
                if (result != null)
                {
                    HttpContext.Session.Set<AdminUser>("userInfo", result);
                    var thongBao = new ThongBao
                    {
                        Message = "Phiên làm việc login vừa hết hạn, bạn vừa được đăng nhập lại bằng cookie!",
                        MessageType = "Primary",
                        DisplayTime = 5
                    };

                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
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
                    Response.Cookies.Append<LoginDTO>("UserCredential", login, new CookieOptions
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
                    MessageType = "Succes",
                    DisplayTime = time
                };

                // Chuyển model sang TempData dưới dạng JSON
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["Message"] = "Wrong username or password";
            }
            return View();
        }
    }
}
