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
using WebsiteSellingBonsaiAPI.Utils;
using WebsiteSellingBonsaiAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Reflection;
using Azure;
using Microsoft.AspNetCore.Authorization;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.DTOS.View;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BonsaisController : Controller
    {
        private readonly APIServices _apiServices;

        public BonsaisController(APIServices processingServices)
        {
            _apiServices = processingServices;
        }

        public async Task<IActionResult> Index(string search_by,string search)
        {
            var (bonsaiList, thongbao) = await _apiServices.FetchDataApiGetList<BonsaiDTO>("bonsaisAPI");
            if (bonsaiList == default)
            {
                bonsaiList = new List<BonsaiDTO>();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }

            //var bonsaiList = bonsaiList.OrderBy(x => x.Style).ToList();

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

            var (bonsai, thongbao) = await _apiServices.FetchDataApiGet<BonsaiDTO>($"bonsaisAPI/{id}");
            if (bonsai == default)
            {
                bonsai = new BonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }

            ViewData["id"] = id;

            return View(bonsai);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var (phanloai, thongbaophanloai) = await _apiServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (phanloai == default)
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaophanloai);
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
                var AvatarPath = await _apiServices.ProcessImage(bonsai.Image, bonsai.ImageOld, "Product");
                
                var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

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
                    CreatedBy = userInfo.UserName,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = userInfo.UserName,
                    UpdatedDate = DateTime.Now,
                };

                var  (isSucces , thongbao) = await _apiServices.FetchDataApiPost<Bonsai>("BonsaisAPI",bonsaiEntity);

                if (isSucces)
                {
                    thongbao.Message = $"Đã tạo thành công {bonsaiEntity.BonsaiName}";
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi dữ liệu tới API.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Lỗi sai định dạng !ModelState.IsValid";
            }

            var (phanloai, thongbaophanloai) = await _apiServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (phanloai == default)
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaophanloai);
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

            var (bonsai, thongbao) = await _apiServices.FetchDataApiGet<BonsaiDTO>($"BonsaisAPI/{id}");
            if (bonsai == default)
            {
                bonsai = new BonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }
            var (phanloai, thongbaophanloai) = await _apiServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (phanloai == default)
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaophanloai);
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
                var AvatarPath = await _apiServices.ProcessImage(bonsai.Image,bonsai.ImageOld,"Product");

                var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

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
                    Image = AvatarPath,
                    TypeId = bonsai.TypeId,
                    StyleId = bonsai.StyleId,
                    GeneralMeaningId = bonsai.GeneralMeaningId,
                    CreatedBy = bonsai.CreatedBy,
                    CreatedDate = bonsai.CreatedDate,
                    UpdatedBy = userInfo.UserName,
                    UpdatedDate = DateTime.Now,
                };

                // Gửi tới API Controller
                var (IsSucces, thongbao) = await _apiServices.FetchDataApiPut<Bonsai>($"BonsaisAPI/{id}", bonsaiEntity);

                if (IsSucces)
                {
                    thongbao.Message = $"Đã cập nhật thành công {bonsaiEntity.BonsaiName}";
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi dữ liệu tới API.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Sai kiểu dữ liệu";
            }
            var (phanloai, thongbaophanloai) = await _apiServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (phanloai == default)
            {
                phanloai = new PhanLoaiBonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaophanloai);
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
            var (bonsai, thongbao) = await _apiServices.FetchDataApiGet<BonsaiDTO>($"BonsaisAPI/{id}");
            if (bonsai == default)
            {
                bonsai = new BonsaiDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
                return View();
            }
            ViewData["id"] = id;
            return View(bonsai);
        }

        // POST: Admin/Bonsais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id,string ImageOld)
        {
            var (result, thongbao) = await _apiServices.FetchDataApiDelete($"BonsaisAPI/{id}", ImageOld);
            
            if (result)
            {
                thongbao.Message = thongbao.Message + $" Bonsai với ID {id}";
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }
            else
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
