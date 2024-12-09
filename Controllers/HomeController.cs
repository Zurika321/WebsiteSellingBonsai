using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Model;
using System.Diagnostics;
using WebsiteSellingMiniBonsai.Models; // Đảm bảo đây là namespace chính xác

namespace WebsiteSellingMiniBonsai.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MiniBonsaiDB _context;

        // Constructor sửa lại tên hàm
        public HomeController(MiniBonsaiDB context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Đánh dấu phương thức là async để sử dụng await
        [HttpGet]
        public async Task<IActionResult> Index(string search, int? typeid, int? styleid, int? GeneralMeaningId, int? sizeMin, int? sizeMax, int? yearOldMin, int? yearOldMax, int? page)
        {
            if (page == null) page = 1;

            var bonsais = _context.Bonsais
                .Include(b => b.Type)
                .Include(b => b.GeneralMeaning)
                .Include(b => b.Style)
                .AsQueryable();

            if (typeid.HasValue) bonsais = bonsais.Where(b => b.TypeId == typeid.Value);
            
            if (styleid.HasValue) bonsais = bonsais.Where(b => b.StyleId == styleid.Value);
            
            if (GeneralMeaningId.HasValue) bonsais = bonsais.Where(b => b.GeneralMeaningId == GeneralMeaningId.Value);
            
            if (sizeMin.HasValue && sizeMin >= 1) bonsais = bonsais.Where(b => b.Size >= sizeMin.Value);
            if (sizeMax.HasValue && sizeMax >= 1) bonsais = bonsais.Where(b => b.Size <= sizeMax.Value);

            if (yearOldMin.HasValue && yearOldMin >= 1) bonsais = bonsais.Where(b => b.YearOld >= yearOldMin.Value);
            if (yearOldMax.HasValue && yearOldMax >= 1) bonsais = bonsais.Where(b => b.YearOld <= yearOldMax.Value);

            if (!string.IsNullOrEmpty(search)) bonsais = bonsais.Where(b => b.BonsaiName.Contains(search) || b.Type.Name.Contains(search));
            
            ViewData["Search"] = search;
            ViewData["TypeId"] = typeid;
            ViewData["StyleId"] = styleid;
            ViewData["GeneralMeaningId"] = GeneralMeaningId;
            ViewData["SizeMin"] = sizeMin;
            ViewData["SizeMax"] = sizeMax;
            ViewData["YearOldMin"] = yearOldMin;
            ViewData["YearOldMax"] = yearOldMax;

            // Lấy danh sách Type để sử dụng trong View
            var types = await _context.Types.Select(t => new { t.Id, t.Name }).OrderBy(t => t.Name).ToListAsync();
            ViewData["TypeList"] = new SelectList(types, "Id", "Name");
            var types2 = await _context.Types.ToListAsync(); // Truyền toàn bộ bảng Type vào View
            ViewData["TypeList2"] = types2;
            var styles = await _context.Styles.Select(t => new { t.Id, t.Name }).ToListAsync();
            ViewData["StyleList"] = new SelectList(styles, "Id", "Name");
            var generalMeanings = await _context.GeneralMeaning.Select(g => new { g.Id, g.Meaning }).ToListAsync();
            ViewData["GeneralMeaningList"] = new SelectList(generalMeanings, "Id", "Meaning");

            const int pageSize = 4;
            // Tính tổng số trang
            var totalCount = await bonsais.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Lấy các bonsai đã phân trang
            var pagedBonsais = await bonsais
                .OrderBy(b => b.Id) // Có thể thay đổi thứ tự sắp xếp nếu cần
                .Skip((page.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền totalPages vào ViewData để hiển thị trong View
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["totalBonsais"] = bonsais.Count();

            return View(pagedBonsais);
        }

        public IActionResult Privacy()
        {
            ViewData["Search"] = "";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
