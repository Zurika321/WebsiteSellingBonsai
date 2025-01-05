using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol.Model;
using System.Diagnostics;
using System.Net.NetworkInformation;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsai.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly APIServices _apiServices;

        // Constructor sửa lại tên hàm
        public HomeController(ILogger<HomeController> logger, APIServices processingServices)
        {
            _apiServices = processingServices;
            _logger = logger;
        }

        // Đánh dấu phương thức là async để sử dụng await
        [HttpGet]
        public async Task<IActionResult> Index(string search, int? typeid, int? styleid, int? GeneralMeaningId, int? page, string sortby, string typesort)
        {
            //var deleteimage = await _apiServices.DeleteImage("Data/banner/backgroud2_20241223145843.jpg");
            //if (deleteimage != "")
            //{
            //    ViewData["Error"] = deleteimage;
            //}
            if (page == null) page = 1;
            if (string.IsNullOrEmpty(sortby)) sortby = "Id";
            if (string.IsNullOrEmpty(typesort)) typesort = "low";

            var (bonsais, thongbaobonsai) = await _apiServices.FetchDataApiGetList<BonsaiDTO>("bonsaisAPI");
            var (banners, thongbaobanner) = await _apiServices.FetchDataApiGetList<BannerDTO>("bannersAPI");

            var (phanloai, thongbaophanloai) = await _apiServices.FetchDataApiGet<PhanLoaiBonsaiDTO>("PhanLoaiBonsaiAPI");
            if (phanloai != default)
            {
                ViewData["GeneralMeaningList"] = new SelectList(phanloai.GeneralMeanings, "Id", "Meaning");
                ViewData["StyleList"] = new SelectList(phanloai.Styles, "Id", "Name");
                ViewData["TypeList"] = new SelectList(phanloai.Types, "Id", "Name");
            }
            else
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaophanloai);
            }

            ViewData["Search"] = search;
            ViewData["TypeId"] = typeid;
            ViewData["StyleId"] = styleid;
            ViewData["GeneralMeaningId"] = GeneralMeaningId;
            ViewData["sortby"] = sortby;
            ViewData["typesort"] = typesort;

            const int pageSize = 12;
            
            
            if (bonsais != default)
            {
                if (typeid.HasValue) bonsais = bonsais.Where(b => b.TypeId == typeid.Value).ToList();
                if (styleid.HasValue) bonsais = bonsais.Where(b => b.StyleId == styleid.Value).ToList();
                if (GeneralMeaningId.HasValue) bonsais = bonsais.Where(b => b.GeneralMeaningId == GeneralMeaningId.Value).ToList();
                //if (!string.IsNullOrEmpty(search)) bonsais = bonsais.Where(b => b.BonsaiName.Contains(search) || b.Type.Name.Contains(search)).ToList();
                if (!string.IsNullOrEmpty(search))
                {
                    string lowerSearch = search.ToLower();
                    bonsais = bonsais.Where(b =>
                        b.BonsaiName.ToLower().Contains(lowerSearch)).ToList();
                }
                var totalCount = bonsais.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var pagedBonsais = new List<BonsaiDTO>();

                // Sử dụng Reflection để sắp xếp theo thuộc tính động
                var propertyInfo = typeof(BonsaiDTO).GetProperty(sortby);
                if (propertyInfo != null)
                {
                    if (typesort == "low")
                    {
                        pagedBonsais = bonsais
                            .OrderBy(b => propertyInfo.GetValue(b, null))  // Sắp xếp tăng dần
                            .Skip((page.Value - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
                    }
                    else
                    {
                        pagedBonsais = bonsais
                            .OrderByDescending(b => propertyInfo.GetValue(b, null))  // Sắp xếp giảm dần
                            .Skip((page.Value - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
                    }
                }

                if (banners == default)
                {
                    ViewData["Banners"] = new BannerDTO();
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaobanner);
                }
                else
                {
                    ViewData["Banners"] = banners;
                }

                ViewData["TotalPages"] = totalPages;
                ViewData["CurrentPage"] = page;
                ViewData["totalBonsais"] = bonsais.Count();

                return View(pagedBonsais);
            }
            ViewData["Error"] = thongbaobonsai.Message;
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaobonsai);
            ViewData["TotalPages"] = 1;
            ViewData["CurrentPage"] = 1;
            ViewData["totalBonsais"] = 0;

            return View(null);
        }

        //if (sizeMin.HasValue && sizeMin >= 1) bonsais = bonsais.Where(b => b.Size >= sizeMin.Value).ToList();
        //if (sizeMax.HasValue && sizeMax >= 1) bonsais = bonsais.Where(b => b.Size <= sizeMax.Value).ToList()
        public IActionResult Privacy()
        {
            ViewData["Search"] = "";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new WebsiteSellingBonsai.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
