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
    public class FavouriteController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;

        // Constructor sửa lại tên hàm
        public FavouriteController(APIServices processingServices, MiniBonsaiDBAPI context)
        {
            _apiServices = processingServices;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if (userInfo == null) return RedirectToAction("Login", "Users", new { area = "Admin" }); // trang login

            return View();
        }   

        [HttpPost, ActionName("AddFavorite")]
        public async Task<IActionResult> AddFavorite(int bonsai_id,string redirectUrl)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
            if (userInfo == null) return RedirectToAction("Login", "Users", new { area = "Admin" });
            
            var havefavourite = await _context.Favourites.FirstOrDefaultAsync(fa => fa.USE_ID == userInfo.Id && fa.BONSAI_ID == bonsai_id);
            if (havefavourite == null)
            {
                var favorite = new Favourite
                {
                    USE_ID = userInfo.Id,
                    BONSAI_ID = bonsai_id,
                    Fav = true
                };
                _context.Favourites.Add(favorite);
            }
            else
            {
                havefavourite.Fav = !havefavourite.Fav;
                _context.Update(havefavourite);
            }
            await _context.SaveChangesAsync();
            return Redirect(redirectUrl ?? Url.Action("Index", "Home"));
        }
    }
}
