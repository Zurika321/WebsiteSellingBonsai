using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;
        public HomeController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            //var Orders = await _context.Styles.ToListAsync();
            return View();
        }
    }
}
