using Microsoft.AspNetCore.Mvc;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http;
using WebsiteSellingBonsaiAPI.DTOS.Carts;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.DTOS.View;

namespace WebsiteSellingBonsai.Controllers
{
    public class BonsaiController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;

        // Constructor sửa lại tên hàm
        public BonsaiController(APIServices processingServices, MiniBonsaiDBAPI context)
        {
            _apiServices = processingServices;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            return View();
        }
        [HttpGet("bonsai/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var (bonsais, thongbaobonsai) = await _apiServices.FetchDataApiGetList<BonsaiDTO>("bonsaisAPI");

            if (bonsais == default)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaobonsai);
            }
            var bonsai = bonsais.FirstOrDefault(b => b.Id == id);

            var (reviews, thongbaoreview) = await _apiServices.FetchDataApiGetList<Review>($"ReviewsAPI/GetReviewByBonsaiId/{id}");

            if (reviews == default)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaoreview);
            }

            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if (userInfo != null)
            {
                ViewData["MyReview"] = reviews.FirstOrDefault(r => r.USE_ID == userInfo.Id);
                ViewData["Reviews"] = reviews.Where(r => r.USE_ID != userInfo.Id).ToList();
            }
            else
            {
                ViewData["Reviews"] = reviews;
            }

            if (bonsai == null)
            {
                return NotFound();
            }

            var BonsaiRelatedMeaning = bonsais.Where(b => b.GeneralMeaningId == bonsai.GeneralMeaningId && b.Id != bonsai.Id).ToList();
            var BonsaiRelatedStyle = bonsais.Where(b => b.StyleId == bonsai.StyleId && b.Id != bonsai.Id).ToList();
            var BonsaiRelatedType = bonsais.Where(b => b.TypeId == bonsai.TypeId && b.Id != bonsai.Id).ToList();

            var model = new BonsaiDetailViewModel
            {
                CurrentBonsai = bonsai,
                BonsaiRelatedMeaning = BonsaiRelatedMeaning,
                BonsaiRelatedStyle = BonsaiRelatedStyle,
                BonsaiRelatedType = BonsaiRelatedType,
            };

            return View(model);
        }
    }
}
