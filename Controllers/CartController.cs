using Microsoft.AspNetCore.Mvc;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http;
using WebsiteSellingBonsaiAPI.DTOS.Carts;
using WebsiteSellingBonsaiAPI.DTOS.User;

namespace WebsiteSellingBonsai.Controllers
{
    public class CartController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;

        // Constructor sửa lại tên hàm
        public CartController(APIServices processingServices, MiniBonsaiDBAPI context)
        {
            _apiServices = processingServices;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if (userInfo == null) return RedirectToAction("Login", "Users", new { area = "Admin" }); // trang login

            var (cart, thongbaopcart) = await _apiServices.FetchDataApiGet<Cart>("CartsAPI/GetCart");
            if (cart == default)
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopcart);
            //    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopcart);

            //var cart = await _context.Carts
            //    .Include(c => c.CartDetails)
            //        .ThenInclude(cd => cd.Bonsai)
            //    .FirstOrDefaultAsync(c => c.USE_ID == userInfo.Id);

            //if (cart == null)
            //{
            //    // Nếu giỏ hàng chưa tồn tại, tạo mới
            //    cart = new Cart
            //    {
            //        USE_ID = userInfo.Id,
            //        CreatedBy = userInfo.UserName,
            //        CreatedDate = DateTime.Now,
            //        UpdatedBy = userInfo.UserName,
            //        UpdatedDate = DateTime.Now,
            //    };
            //    _context.Carts.Add(cart);
            //    await _context.SaveChangesAsync();
            //    cart.CartDetails = new List<CartDetail>();
            //}
            //if (cart.CartDetails == null)
            //{
            //    cart.CartDetails = new List<CartDetail>();
            //}

            return View(cart);
        }

        [HttpPost, ActionName("AddCart")]
        public async Task<IActionResult> AddCart(Addcart addcart,string redirectUrl)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
            if (userInfo == null) return RedirectToAction("Login", "Users", new { area = "Admin" });

            var (Success, thongbaopostcart) = await _apiServices.FetchDataApiPost<Addcart>("CartsAPI/AddBonsai", addcart);
            if (!Success) {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            }
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            return Redirect(redirectUrl ?? Url.Action("Index", "Home"));
            //return NoContent();
        }
        [HttpPost, ActionName("update_quantity")]
        public async Task<IActionResult> update_quantity(Update_cart up)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
            if (userInfo == null) return RedirectToAction("Login", "Users", new { area = "Admin" });

            var (Success, thongbaopostcart) = await _apiServices.FetchDataApiPut<Update_cart>($"CartsAPI/update_cart", up);
            if (!Success)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            }
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost, ActionName("RemoveFromCart")]
        public async Task<IActionResult> RemoveFromCart(int CART_D_ID)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
            if (userInfo == null) return RedirectToAction("Login", "Users", new { area = "Admin" });

            var (Success, thongbaopostcart) = await _apiServices.FetchDataApiDelete($"CartsAPI/{CART_D_ID}" , image: null);
            if (!Success)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            }
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaopostcart);
            return RedirectToAction("Index", "Cart");
        }
 
    }
}
