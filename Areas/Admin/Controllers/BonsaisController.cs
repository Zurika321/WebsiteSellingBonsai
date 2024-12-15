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
using System.Reflection;
using Azure;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BonsaisController : Controller
    {
        private readonly IWebHostEnvironment _hostEnv;
        private readonly HttpClient _httpClient;

        public BonsaisController(IWebHostEnvironment hostEnv, IHttpClientFactory httpClientFactory)
        {
            _hostEnv = hostEnv;
            _httpClient = httpClientFactory.CreateClient();
            if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            }
            _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
        }

        // GET: Admin/Bonsais
        public async Task<IActionResult> Index(string search_by,string search)
        {
            List<BonsaiDTO> bonsaiList = new List<BonsaiDTO>();
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:44351/api/bonsaisAPI");
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

            if (!string.IsNullOrEmpty(search_by) && !string.IsNullOrEmpty(search))
            {
                var propertyInfo = typeof(BonsaiDTO).GetProperty(search_by, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    bonsaiList = bonsaiList
                        .Where(b =>
                        {
                            var value = propertyInfo.GetValue(b)?.ToString();
                            return value != null && value.Contains(search, StringComparison.OrdinalIgnoreCase);
                        })
                        .ToList();
                    ViewData["search"] =search;
                    ViewData["search_by"] = search_by;
                }
                else
                {
                    ViewBag.ErrorMessage = $"Cột '{search_by}' không tồn tại.";
                }
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
                var responseBonsai = await _httpClient.GetAsync($"https://localhost:44351/api/bonsaisAPI/{id}");

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
            ViewData["id"] = id;

            return View(bonsai);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var phanloaiResponse = await _httpClient.GetAsync("https://localhost:44351/api/PhanLoaiBonsaiAPI");

                if (phanloaiResponse.IsSuccessStatusCode)
                {
                    var phanloai = Newtonsoft.Json.JsonConvert.DeserializeObject<PhanLoaiBonsaiDTO>(
                        await phanloaiResponse.Content.ReadAsStringAsync()
                    );

                    // Tạo SelectList từ dữ liệu của DTO
                    ViewData["GeneralMeaningId"] = new SelectList(phanloai?.GeneralMeanings, "Id", "Meaning");
                    ViewData["StyleId"] = new SelectList(phanloai?.Styles, "Id", "Name");
                    ViewData["TypeId"] = new SelectList(phanloai?.Types, "Id", "Name");

                    return View();
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = "Không thể tải dữ liệu từ API.",
                        MessageType = "Danger",
                        DisplayTime = 5
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = $"Exception: { ex.Message}",
                    MessageType = "Danger",
                    DisplayTime = 10
                }); 
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi gọi API.";
            }

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

                        var extension = Path.GetExtension(bonsai.Image.FileName);
                        var fileName = bonsai.BonsaiName;
                                       /*Path.GetFileNameWithoutExtension(bonsai.Image.FileName);*/
                       
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
                        Image = AvatarPath, // Có thể là "" nếu không có ảnh
                        TypeId = bonsai.TypeId,
                        StyleId = bonsai.StyleId,
                        GeneralMeaningId = bonsai.GeneralMeaningId,
                    };

                    // Gửi tới API Controller
                    var response = await _httpClient.PostAsJsonAsync("https://localhost:44351/api/bonsaisAPI", bonsaiEntity);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                        {
                            Message = $"Đã tạo thành công {bonsaiEntity.BonsaiName}",
                            MessageType = "Primary",
                            DisplayTime = 5
                        });
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                        {
                            Message = "Có lỗi xảy ra khi gửi dữ liệu tới API.",
                            MessageType = "Danger",
                            DisplayTime = 5
                        });
                        ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi dữ liệu tới API.";
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = "Có lỗi xảy ra khi lưu dữ liệu.",
                        MessageType = "Danger",
                        DisplayTime = 5
                    });
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi lưu dữ liệu.";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi sai định dạng !ModelState.IsValid";
            }

            var phanloaiResponse = await _httpClient.GetAsync("https://localhost:44351/api/PhanLoaiBonsaiAPI");
            if (phanloaiResponse.IsSuccessStatusCode)
            {
                var phanloai = Newtonsoft.Json.JsonConvert.DeserializeObject<PhanLoaiBonsaiDTO>(
                    await phanloaiResponse.Content.ReadAsStringAsync()
                );

                // Tạo SelectList từ dữ liệu của DTO
                ViewData["GeneralMeaningId"] = new SelectList(phanloai?.GeneralMeanings, "Id", "Meaning");
                ViewData["StyleId"] = new SelectList(phanloai?.Styles, "Id", "Name");
                ViewData["TypeId"] = new SelectList(phanloai?.Types, "Id", "Name");
            }

            return View();
        }

            // GET: Admin/Bonsais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BonsaiDTO bonsai = null;
            try
            {
                var responseBonsai = await _httpClient.GetAsync($"https://localhost:44351/api/bonsaisAPI/{id}");
                var phanloaiResponse = await _httpClient.GetAsync("https://localhost:44351/api/PhanLoaiBonsaiAPI");

                if (phanloaiResponse.IsSuccessStatusCode &&
                    responseBonsai.IsSuccessStatusCode)
                {
                    var phanloai = Newtonsoft.Json.JsonConvert.DeserializeObject<PhanLoaiBonsaiDTO>(
                        await phanloaiResponse.Content.ReadAsStringAsync()
                    );
                    var jsonData = await responseBonsai.Content.ReadAsStringAsync();
                    bonsai = JsonConvert.DeserializeObject<BonsaiDTO>(jsonData);

                    ViewData["GeneralMeaningId"] = new SelectList(phanloai?.GeneralMeanings, "Id", "Meaning");
                    ViewData["StyleId"] = new SelectList(phanloai?.Styles, "Id", "Name");
                    ViewData["TypeId"] = new SelectList(phanloai?.Types, "Id", "Name");

                    return View(bonsai);
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = "Không thể tải dữ liệu từ API.",
                        MessageType = "Danger",
                        DisplayTime = 5
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = $"Exception: {ex.Message}",
                    MessageType = "Danger",
                    DisplayTime = 10
                });
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi gọi API.";
            }
            @ViewData["id"] = id;

            return View(bonsai);
        }

        // POST: Admin/Bonsais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] BonsaiDTO bonsai)
        {
            if (id != bonsai.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var AvatarPath = bonsai.ImageOld;
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
                        Id = id,
                        BonsaiName = bonsai.BonsaiName,
                        Description = bonsai.Description,
                        FengShuiMeaning = bonsai.FengShuiMeaning,
                        Size = bonsai.Size,
                        YearOld = bonsai.YearOld,
                        MinLife = bonsai.MinLife,
                        MaxLife = bonsai.MaxLife,
                        Price = bonsai.Price,
                        Quantity = bonsai.Quantity,
                        Image = AvatarPath, // Có thể là "" nếu không có ảnh
                        TypeId = bonsai.TypeId,
                        StyleId = bonsai.StyleId,
                        GeneralMeaningId = bonsai.GeneralMeaningId,
                    };

                    // Gửi tới API Controller
                    var response = await _httpClient.PutAsJsonAsync(
                         $"https://localhost:44351/api/bonsaisAPI/{bonsai.Id}",
                         bonsaiEntity
                     );

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                        {
                            Message = $"Đã sửa thành công {bonsaiEntity.BonsaiName}",
                            MessageType = "Primary",
                            DisplayTime = 5
                        });
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                        {
                            Message = "Có lỗi xảy ra khi gửi dữ liệu tới API.",
                            MessageType = "Danger",
                            DisplayTime = 5
                        });
                        ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi dữ liệu tới API.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = "Có lỗi xảy ra khi lưu dữ liệu.",
                        MessageType = "Danger",
                        DisplayTime = 5
                    });
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi lưu dữ liệu.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Sai kiểu dữ liệu";
            }
            var phanloaiResponse = await _httpClient.GetAsync("https://localhost:44351/api/PhanLoaiBonsaiAPI");
            if (phanloaiResponse.IsSuccessStatusCode)
            {
                var phanloai = Newtonsoft.Json.JsonConvert.DeserializeObject<PhanLoaiBonsaiDTO>(
                    await phanloaiResponse.Content.ReadAsStringAsync()
                );

                // Tạo SelectList từ dữ liệu của DTO
                ViewData["GeneralMeaningId"] = new SelectList(phanloai?.GeneralMeanings, "Id", "Meaning");
                ViewData["StyleId"] = new SelectList(phanloai?.Styles, "Id", "Name");
                ViewData["TypeId"] = new SelectList(phanloai?.Types, "Id", "Name");
            }
            return View(bonsai);
        }

        // GET: Admin/Bonsais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BonsaiDTO bonsai = null;

            try
            {
                var responseBonsai = await _httpClient.GetAsync($"https://localhost:44351/api/bonsaisAPI/{id}");

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
            ViewData["id"] = id;

            return View(bonsai);
        }

        // POST: Admin/Bonsais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Gửi yêu cầu DELETE tới API
                var response = await _httpClient.DeleteAsync($"https://localhost:44351/api/bonsaisAPI/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = $"Đã xóa thành công Bonsai với ID {id}",
                        MessageType = "Primary",
                        DisplayTime = 5
                    });
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = "Không thể xóa Bonsai. Có thể ID không tồn tại hoặc xảy ra lỗi trên máy chủ.",
                        MessageType = "Danger",
                        DisplayTime = 5
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Có lỗi xảy ra khi xóa Bonsai.",
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
