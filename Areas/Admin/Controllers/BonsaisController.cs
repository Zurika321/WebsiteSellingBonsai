using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using WebsiteSellingMiniBonsai.Areas.Admin.DTOS;
using WebsiteSellingMiniBonsai.Areas.Admin.Utils;
using WebsiteSellingMiniBonsai.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BonsaisController : Controller
    {
        private readonly MiniBonsaiDB _context;
        private readonly IWebHostEnvironment _hostEnv;

        public BonsaisController(MiniBonsaiDB context, IWebHostEnvironment hostEnv)
        {
            _context = context;
            _hostEnv = hostEnv;
        }
        
        // GET: Admin/Bonsais
        public async Task<IActionResult> Index()
        {
            var miniBonsaiDB = _context.Bonsais.Include(b => b.GeneralMeaning).Include(b => b.Style).Include(b => b.Type);
            return View(await miniBonsaiDB.ToListAsync());
        }

        // GET: Admin/Bonsais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonsai = await _context.Bonsais
                .Include(b => b.GeneralMeaning)
                .Include(b => b.Style)
                .Include(b => b.Type)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bonsai == null)
            {
                return NotFound();
            }

            return View(bonsai);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["GeneralMeaningId"] = new SelectList(_context.GeneralMeaning, "Id", "Meaning");
            ViewData["StyleId"] = new SelectList(_context.Styles, "Id", "Name");
            ViewData["TypeId"] = new SelectList(_context.Types, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BonsaiDTO bonsai)
        {
            if (ModelState.IsValid)
            {
                // Tạo danh sách chứa lỗi
                var errorMessages = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                ViewBag.ErrorMessages = errorMessages;

                // Ghi log để debug nếu cần
                foreach (var error in errorMessages)
                {
                    Console.WriteLine($"Validation Error: {error}");
                }

                try
                {
                    var AvatarPath = "";
                    if (bonsai.Image != null)
                    {
                        var webRootPath = _hostEnv.WebRootPath;

                        var uploadFolder = Path.Combine(webRootPath, "Data/Product");

                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        var fileName = bonsai.BonsaiName;/*Path.GetFileNameWithoutExtension(bonsai.Image.FileName);*/
                        var extension = Path.GetExtension(bonsai.Image.FileName);
                        var newFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

                        var filePath = Path.Combine(uploadFolder, newFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await bonsai.Image.CopyToAsync(fileStream);
                        }

                        AvatarPath = $"Data/Product/{newFileName}";
                    }

                    var userInfo = HttpContext.Session.Get<AdminUser>("userInfo");
                    var bonsaiEntity = new Bonsai
                    {
                        BonsaiName = bonsai.BonsaiName,
                        Description = bonsai.Description,
                        FengShuiMeaning = bonsai.FengShuiMeaning,
                        Size = bonsai.Size,
                        YearOld = bonsai.YearOld,
                        MinLife = bonsai.MinLife,
                        MaxLife = bonsai.MaxLife,
                        Price = bonsai.Price,
                        Quantity = bonsai.Quantity,
                        Image = AvatarPath,
                        TypeId = bonsai.TypeId,
                        StyleId = bonsai.StyleId,
                        GeneralMeaningId = bonsai.GeneralMeaningId, 

                    };
                    _context.Add(bonsaiEntity);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tạo mới Bonsai thành công!";
                    var thongBao = new ThongBao
                    {
                        Message = "Tạo mới Bonsai thành công!",
                        MessageType = "Primary",
                        DisplayTime = 5
                    };

                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    var thongBao = new ThongBao
                    {
                        Message = "Có lỗi xảy ra khi lưu dữ liệu.",
                        MessageType = "Primary",
                        DisplayTime = 5
                    };

                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi lưu dữ liệu.";
                    ViewData["GeneralMeaningId"] = new SelectList(_context.GeneralMeaning, "Id", "Meaning");
                    ViewData["StyleId"] = new SelectList(_context.Styles, "Id", "Name");
                    ViewData["TypeId"] = new SelectList(_context.Types, "Id", "Name");
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi sai định dạng !ModelState.IsValid";
            }
            ViewData["GeneralMeaningId"] = new SelectList(_context.GeneralMeaning, "Id", "Meaning");
            ViewData["StyleId"] = new SelectList(_context.Styles, "Id", "Name");
            ViewData["TypeId"] = new SelectList(_context.Types, "Id", "Name");
            return View();
        }

        // GET: Admin/Bonsais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonsai = await _context.Bonsais.FindAsync(id);
            if (bonsai == null)
            {
                return NotFound();
            }
            ViewData["GeneralMeaningId"] = new SelectList(_context.GeneralMeaning, "Id", "Meaning", bonsai.GeneralMeaningId);
            ViewData["StyleId"] = new SelectList(_context.Styles, "Id", "Name", bonsai.StyleId);
            ViewData["TypeId"] = new SelectList(_context.Types, "Id", "Name", bonsai.TypeId);
            return View(bonsai);
        }

        // POST: Admin/Bonsais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BonsaiName,Description,FengShuiMeaning,Size,Price,Quantity,YearOld,MinLife,MaxLife,TypeId,StyleId,GeneralMeaningId")] Bonsai bonsai)
        {
            if (id != bonsai.Id)
            {
                return NotFound();
            }

            if (bonsai.BonsaiName != null)
            {
                try
                {
                    _context.Update(bonsai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonsaiExists(bonsai.Id))
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
            ViewData["GeneralMeaningId"] = new SelectList(_context.GeneralMeaning, "Id", "Meaning", bonsai.GeneralMeaningId);
            ViewData["StyleId"] = new SelectList(_context.Styles, "Id", "Name", bonsai.StyleId);
            ViewData["TypeId"] = new SelectList(_context.Types, "Id", "Name", bonsai.TypeId);
            return View(bonsai);
        }

        // GET: Admin/Bonsais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonsai = await _context.Bonsais
                .Include(b => b.GeneralMeaning)
                .Include(b => b.Style)
                .Include(b => b.Type)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bonsai == null)
            {
                return NotFound();
            }

            return View(bonsai);
        }

        // POST: Admin/Bonsais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bonsai = await _context.Bonsais.FindAsync(id);
            if (bonsai != null)
            {
                _context.Bonsais.Remove(bonsai);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BonsaiExists(int id)
        {
            return _context.Bonsais.Any(e => e.Id == id);
        }
    }
}
