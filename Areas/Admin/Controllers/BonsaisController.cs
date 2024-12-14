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
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Utils;
using WebsiteSellingBonsaiAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BonsaisController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly HttpClient _httpClient;

        public BonsaisController(MiniBonsaiDBAPI context, IWebHostEnvironment hostEnv, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _hostEnv = hostEnv;
            _httpClient = httpClientFactory.CreateClient();
            if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            }
            // Thêm API Key
            _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
        }

        // GET: Admin/Bonsais
        public async Task<IActionResult> Index()
        {
            List<BonsaiDTO> bonsaiList = new List<BonsaiDTO>();
            try
            {
                // Gọi API để lấy danh sách bonsai
                var response = await _httpClient.GetAsync("https://localhost:44351/api/bonsais");
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize JSON trả về thành danh sách BonsaiDTO
                    var jsonData = await response.Content.ReadAsStringAsync();
                    bonsaiList = JsonConvert.DeserializeObject<List<BonsaiDTO>>(jsonData);
                }
                else
                {
                    ViewBag.ErrorMessage = "Không thể lấy dữ liệu từ API.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi kết nối tới API.";
            }

            return View(bonsaiList);
        }


        // GET: Admin/Bonsais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BonsaiDTO bonsai = null;

            try
            {
                var responseBonsai = await _httpClient.GetAsync($"https://localhost:44351/api/bonsais/{id}");

                if (responseBonsai.IsSuccessStatusCode)
                {
                    // Đọc dữ liệu JSON và deserialize thành BonsaiDTO
                    var jsonData = await responseBonsai.Content.ReadAsStringAsync();
                    bonsai = JsonConvert.DeserializeObject<BonsaiDTO>(jsonData);
                }
                else
                {
                    ViewBag.ErrorMessage = "Không thể lấy dữ liệu từ API.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HttpRequestException: {httpEx.Message}");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi kết nối tới API.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi kết nối tới API.";
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
                        //var newFileName = $"{Guid.NewGuid()}{extension}"; bảo mật 1
                        //var fileBytes = memoryStream.ToArray(); bảo mật siêu cấp
                        //var hash = SHA256.HashData(fileBytes);
                        //var fileHash = BitConverter.ToString(hash).Replace("-", "").ToLower();

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
            var bonsaiDTO = new BonsaiDTO
            {
                Id = bonsai.Id,
                BonsaiName = bonsai.BonsaiName,
                Description = bonsai.Description,
                FengShuiMeaning = bonsai.FengShuiMeaning,
                Size = bonsai.Size,
                YearOld = bonsai.YearOld,
                MinLife = bonsai.MinLife,
                MaxLife = bonsai.MaxLife,
                Price = bonsai.Price,
                Quantity = bonsai.Quantity,
                ImageOld = bonsai.Image,
                nopwr = bonsai.NOPWR,
                rates = bonsai.Rates,
                TypeId = bonsai.TypeId,
                StyleId = bonsai.StyleId,
                GeneralMeaningId = bonsai.GeneralMeaningId,
                //CreatedBy = product.CreatedBy,
                //CreatedDate = product.CreatedDate,
                //UpdatedBy = product.UpdatedBy,
                //UpdatedDate = product.UpdatedDate,
            };
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
