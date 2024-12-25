using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannersController : Controller
    {
        //private readonly HttpClient _httpClient;
        private readonly ProcessingServices _processingServices;

        public BannersController(/*IHttpClientFactory httpClientFactory, */ProcessingServices processingServices)
        {
            //_httpClient = httpClientFactory.CreateClient();
            //if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            //{
            //    _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            //}
            //_httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
            _processingServices = processingServices;
        }
        // GET: Admin/Banners
        public async Task<IActionResult> Index()
        {
            var (bannerList, errorbanner) = await _processingServices.FetchDataApiGetList<BannerDTO>("BannersAPI");
            if (errorbanner != "")
            {
                bannerList = new List<BannerDTO>();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbanner,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
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

            var (banner, errorbanner) = await _processingServices.FetchDataApiGet<BannerDTO>($"BannerssAPI/{id}");
            if (errorbanner != "")
            {
                banner = new BannerDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbanner,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
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
                var AvatarPath = await _processingServices.ProcessImage(banner.Image, banner.ImageOld, "banner");

                //var userInfo = HttpContext.Session.Get<AdminUser>("userInfo");

                var bannerEntity = new Banner
                {
                    Title = banner.Title,
                    ImageUrl = AvatarPath

                };

                var (newbanner, errornewbanner) = await _processingServices.FetchDataApiPost<Banner>("BannersAPI", bannerEntity);

                if (errornewbanner == "")
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = $"Đã tạo thành công {bannerEntity.Title}",
                        MessageType = "Primary",
                        DisplayTime = 5
                    });
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = errornewbanner,
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
            return View();
        }

        // GET: Admin/Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var (banner, errorbanner) = await _processingServices.FetchDataApiGet<BannerDTO>($"BannersAPI/{id}");
            if (errorbanner != "")
            {
                banner = new BannerDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbanner,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
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
                var AvatarPath = await _processingServices.ProcessImage(banner.Image, banner.ImageOld, "banner");

                //var userInfo = HttpContext.Session.Get<AdminUser>("userInfo");

                var bannerEntity = new Banner
                {
                    BAN_ID = banner.BAN_ID,
                    Title = banner.Title,
                    ImageUrl = AvatarPath,
                };

                // Gửi tới API Controller
                var (newbanner, errornewbanner) = await _processingServices.FetchDataApiPut<Banner>($"BannersAPI/{id}", bannerEntity);

                if (errornewbanner == "")
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = $"Đã cập nhật thành công {bannerEntity.Title}",
                        MessageType = "Primary",
                        DisplayTime = 5
                    });
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = errornewbanner,
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
            return View(banner);
        }

        // GET: Admin/Banners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var (banner, errorbanner) = await _processingServices.FetchDataApiGet<BannerDTO>($"BannersAPI/{id}");
            if (errorbanner != "")
            {
                banner = new BannerDTO();
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = errorbanner,
                    MessageType = "Danger",
                    DisplayTime = 5
                });
            }
            ViewData["id"] = id;
            return View(banner);
        }

        // POST: Admin/Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (result, error) = await _processingServices.FetchDataApiDelete($"BannersAPI/{id}");

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
