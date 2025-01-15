using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS.View;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannersController : Controller
    {
        private readonly APIServices _apiServices;

        public BannersController(APIServices apiServices)
        {
            _apiServices = apiServices;
        }
        // GET: Admin/Banners
        public async Task<IActionResult> Index()
        {
            var (bannerList, thongbao) = await _apiServices.FetchDataApiGetList<BannerDTO>("BannersAPI");
            if (bannerList == default)
            {
                bannerList = new List<BannerDTO>();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }
            return View(bannerList);
        }

        // GET: Admin/Banners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var (banner, thongbao) = await _apiServices.FetchDataApiGet<BannerDTO>($"BannerssAPI/{id}");
            if (banner == default)
            {
                banner = new BannerDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }

            ViewData["id"] = id;

            return View(banner);
        }

        // GET: Admin/Banners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Banners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BannerDTO banner)
        {
            if (ModelState.IsValid)
            {
                var AvatarPath = await _apiServices.ProcessImage(banner.Image, banner.ImageOld, "banner");

                //var userInfo = HttpContext.Session.Get<AdminUser>("userInfo");

                var bannerEntity = new Banner
                {
                    Title = banner.Title,
                    ImageUrl = AvatarPath

                };

                var (IsSucces , thongbao) = await _apiServices.FetchDataApiPost<Banner>("BannersAPI", bannerEntity);

                if (IsSucces)
                {
                    thongbao.Message = $"Đã tạo thành công {bannerEntity.Title}";
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
            return View();
        }

        // GET: Admin/Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var (banner, thongbao) = await _apiServices.FetchDataApiGet<BannerDTO>($"BannersAPI/{id}");
            if (banner == default)
            {
                banner = new BannerDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }
            return View(banner);
        }

        // POST: Admin/Banners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] BannerDTO banner)
        {
            if (id != banner.BAN_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var AvatarPath = await _apiServices.ProcessImage(banner.Image, banner.ImageOld, "banner");

                //var userInfo = HttpContext.Session.Get<AdminUser>("userInfo");

                var bannerEntity = new Banner
                {
                    BAN_ID = banner.BAN_ID,
                    Title = banner.Title,
                    ImageUrl = AvatarPath,
                };

                // Gửi tới API Controller
                var (IsSucces, thongbao) = await _apiServices.FetchDataApiPut<Banner>($"BannersAPI/{id}", bannerEntity);

                if (IsSucces)
                {
                    thongbao.Message = $"Đã cập nhật thành công {bannerEntity.Title}";
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
            return View(banner);
        }

        // GET: Admin/Banners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var (banner, thongbao) = await _apiServices.FetchDataApiGet<BannerDTO>($"BannersAPI/{id}");
            if (banner == default)
            {
                banner = new BannerDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
            }
            ViewData["id"] = id;
            return View(banner);
        }

        // POST: Admin/Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id,string ImageOld)
        {
            var (result, thongbao) = await _apiServices.FetchDataApiDelete($"BannersAPI/{id}", ImageOld);

            if (result)
            {
                thongbao.Message = thongbao.Message + $" Banner với ID {id}";
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
