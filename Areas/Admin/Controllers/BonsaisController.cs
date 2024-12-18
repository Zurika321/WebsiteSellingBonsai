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
        private readonly ProcessingServices _processingServices;

        public BonsaisController(IWebHostEnvironment hostEnv, IHttpClientFactory httpClientFactory, ProcessingServices processingServices)
        {
            _hostEnv = hostEnv;
            _httpClient = httpClientFactory.CreateClient();
            if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            }
            _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
            _processingServices = processingServices;
        }

        // GET: Admin/Bonsais
        public async Task<IActionResult> Index(string search_by,string search)
        {
            var (bonsaiList, errorbonsai) = await _processingServices.FetchDataApiGetList<BonsaiDTO>("bonsaisAPI");
            if (errorbonsai != "")
            {
                bonsaiList = new List<BonsaiDTO>();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbonsai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
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

            var (bonsai, errorbonsai) = await _processingServices.FetchDataApiGet<BonsaiDTO>($"bonsaisAPI/{id}");
            if (errorbonsai != "")
            {
                bonsai = new BonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbonsai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }

            ViewData["id"] = id;

            return View(bonsai);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var (phanloai, errorphanloai) = await _processingServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (errorphanloai != "")
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorphanloai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            else
            {
                ViewData["GeneralMeaningId"] = new SelectList(phanloai?.GeneralMeanings, "Id", "Meaning");
                ViewData["StyleId"] = new SelectList(phanloai?.Styles, "Id", "Name");
                ViewData["TypeId"] = new SelectList(phanloai?.Types, "Id", "Name");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BonsaiDTO bonsai)
        {
            if (ModelState.IsValid)
            {
                var AvatarPath = await _processingServices.ProcessImage(bonsai.Image, bonsai.ImageOld, "Product");
                
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

                var (newbonsai , errornewbonsai) = await _processingServices.FetchDataApiPost<Bonsai>("BonsaisAPI",bonsaiEntity);

                if (errornewbonsai == "")
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
                        Message = errornewbonsai,
                        MessageType = "Danger",
                        DisplayTime = 5
                    });
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi dữ liệu tới API.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi sai định dạng !ModelState.IsValid";
            }

            var (phanloai, errorphanloai) = await _processingServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (errorphanloai != "")
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorphanloai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            else
            {
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

            var (bonsai, errorbonsai) = await _processingServices.FetchDataApiGet<BonsaiDTO>($"BonsaisAPI/{id}");
            if (errorbonsai != "")
            {
                bonsai = new BonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbonsai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            var (phanloai, errorphanloai) = await _processingServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (errorphanloai != "")
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorphanloai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            else
            {
                ViewData["GeneralMeaningId"] = new SelectList(phanloai?.GeneralMeanings, "Id", "Meaning");
                ViewData["StyleId"] = new SelectList(phanloai?.Styles, "Id", "Name");
                ViewData["TypeId"] = new SelectList(phanloai?.Types, "Id", "Name");
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
                var AvatarPath = await _processingServices.ProcessImage(bonsai.Image,bonsai.ImageOld,"Product");

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
                var (newbonsai, errornewbonsai) = await _processingServices.FetchDataApiPut<Bonsai>($"BonsaisAPI/{id}", bonsaiEntity);

                if (errornewbonsai == "")
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = $"Đã cập nhật thành công {bonsaiEntity.BonsaiName}",
                        MessageType = "Primary",
                        DisplayTime = 5
                    });
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = errornewbonsai,
                        MessageType = "Danger",
                        DisplayTime = 5
                    });
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi dữ liệu tới API.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Sai kiểu dữ liệu";
            }
            var (phanloai, errorphanloai) = await _processingServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (errorphanloai != "")
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorphanloai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            else
            {
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
            var (bonsai, errorbonsai) = await _processingServices.FetchDataApiGet<BonsaiDTO>($"BonsaisAPI/{id}");
            if (errorbonsai != "")
            {
                bonsai = new BonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbonsai,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            ViewData["id"] = id;
            return View(bonsai);
        }

        // POST: Admin/Bonsais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (result, error) = await _processingServices.FetchDataApiDelete($"BonsaisAPI/{id}");
            
            if (result)
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
                    Message = error,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
