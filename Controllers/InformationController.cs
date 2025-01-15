using Microsoft.AspNetCore.Mvc;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http;
using WebsiteSellingBonsaiAPI.DTOS.Carts;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebsiteSellingBonsai.Controllers
{
    public class InformationController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;

        // Constructor sửa lại tên hàm
        public InformationController(APIServices processingServices, MiniBonsaiDBAPI context)
        {
            _apiServices = processingServices;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> update_information(ChangeInformation ci)
        {
            var (success, thongbao) = await _apiServices.changeInformation(ci);

            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> update_avatar([FromForm] IFormFile newAvatar,string ImageOld)
        {
            if (ImageOld == "Data/usernoimage.png")
            {
                ImageOld = "";
            }
            var AvatarPath = await _apiServices.ProcessImage(newAvatar, ImageOld, "Users");
            
            if (string.IsNullOrEmpty(AvatarPath))
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Không tìm thấy ảnh để thay đổi",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });
                return RedirectToAction("Index");
            }
            var (success, thongbao) = await _apiServices.changeAvatar(AvatarPath);

            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);

            return RedirectToAction("Index");
        }
    }
}
