using Microsoft.AspNetCore.Mvc;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;
using WebsiteSellingBonsaiAPI.DTOS;

namespace WebsiteSellingBonsai.Controllers
{
    public class CartController : Controller
    {
        private readonly APIServices _apiServices;

        // Constructor sửa lại tên hàm
        public CartController(APIServices processingServices)
        {
            _apiServices = processingServices;
        }
        public async Task<IActionResult> Index()
        {
            var (cart,thongbao) = await _apiServices.FetchDataApiGetList<Cart>("CartsAPI");
            if (cart == null) return View(new Cart());
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddCart(int bonsai_id,int quantity)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUser>("userInfo");
            if  (userInfo == null) return RedirectToAction("Login", "Users", new { area = "Admin" });

            var addcart = new Addcart
            {
                bonsai_id = bonsai_id,
                quantity = quantity,
            };

            var (Success, thongbaopostcart) = await _apiServices.FetchDataApiPost<Addcart>("CartsAPI/addbonsai", addcart);
            if (!Success) {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            }
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            return View();
        }
    }
}
