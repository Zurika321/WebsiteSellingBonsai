using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GeneralMeaningsController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;

        public GeneralMeaningsController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: Admin/GeneralMeanings
        public async Task<IActionResult> Index()
        {
            return View(await _context.GeneralMeaning.ToListAsync());
        }

        // GET: Admin/GeneralMeanings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalMeaning = await _context.GeneralMeaning
                .FirstOrDefaultAsync(m => m.Id == id);
            if (generalMeaning == null)
            {
                return NotFound();
            }

            return View(generalMeaning);
        }

        // GET: Admin/GeneralMeanings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/GeneralMeanings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Meaning,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] GeneralMeaning generalMeaning)
        {
            if (ModelState.IsValid)
            {
                var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
                if (userInfo == null)
                    return RedirectToAction("Login", "Users", new { area = "Admin" });
                generalMeaning.UpdatedDate = DateTime.Now;
                generalMeaning.UpdatedBy = userInfo.UserName;
                generalMeaning.CreatedDate = DateTime.Now;
                generalMeaning.CreatedBy = userInfo.UserName;
                _context.Add(generalMeaning);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(generalMeaning);
        }

        // GET: Admin/GeneralMeanings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalMeaning = await _context.GeneralMeaning.FindAsync(id);
            if (generalMeaning == null)
            {
                return NotFound();
            }
            return View(generalMeaning);
        }

        // POST: Admin/GeneralMeanings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Meaning,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] GeneralMeaning generalMeaning)
        {
            if (id != generalMeaning.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
                    if (userInfo == null)
                        return RedirectToAction("Login", "Users", new { area = "Admin" });
                    generalMeaning.UpdatedDate = DateTime.Now;
                    generalMeaning.UpdatedBy = userInfo.UserName;
                    _context.Update(generalMeaning);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneralMeaningExists(generalMeaning.Id))
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
            return View(generalMeaning);
        }

        // GET: Admin/GeneralMeanings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalMeaning = await _context.GeneralMeaning
                .FirstOrDefaultAsync(m => m.Id == id);
            if (generalMeaning == null)
            {
                return NotFound();
            }

            return View(generalMeaning);
        }

        // POST: Admin/GeneralMeanings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id,string password)
        {
            var generalMeaning = await _context.GeneralMeaning
                .Include(g => g.Bonsais)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (generalMeaning != null)
            {
                if (generalMeaning.Bonsais.Any())
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = "Không thể xóa General Meaning này vì có liên kết với bảng khác!",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5,
                    });

                    return RedirectToAction(nameof(Index));
                }
                //_context.Bonsais.RemoveRange(style.Bonsais);
                _context.GeneralMeaning.Remove(generalMeaning);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GeneralMeaningExists(int id)
        {
            return _context.GeneralMeaning.Any(e => e.Id == id);
        }
        
        public async Task<IActionResult> ConfirmDelete(GeneralMeaning generalMeaning)
        {
            return View(generalMeaning);
        }
    }
     
}
