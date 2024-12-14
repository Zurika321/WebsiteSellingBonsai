using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol.Model;
using System.Diagnostics;
using WebsiteSellingBonsai.Models;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsai.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MiniBonsaiDBAPI _context;
        private readonly HttpClient _httpClient;

        // Constructor sửa lại tên hàm
        public HomeController(MiniBonsaiDBAPI context, ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            }
            // Thêm API Key
            _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
        }

        // Đánh dấu phương thức là async để sử dụng await
        [HttpGet]
        public async Task<IActionResult> Index(string search, int? typeid, int? styleid, int? GeneralMeaningId, int? sizeMin, int? sizeMax, int? yearOldMin, int? yearOldMax, int? page)
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
            }
            try
            {
                if (page == null) page = 1;

                var bonsaisApiUrl = "https://localhost:44351/api/bonsais";
                var productsResponse = await _httpClient.GetAsync(bonsaisApiUrl);
                productsResponse.EnsureSuccessStatusCode();
                var productsJson = await productsResponse.Content.ReadAsStringAsync();
                var bonsais = JsonConvert.DeserializeObject<List<BonsaiDTO>>(productsJson);

                var thongBao = new ThongBao
                {
                    Message = "Nhận dữ liệu thành công.",
                    MessageType = "Primary",
                    DisplayTime = 5
                };

                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);

                //var bonsais = _context.Bonsais
                //.Include(b => b.Type)
                //.Include(b => b.GeneralMeaning)
                //.Include(b => b.Style)
                //.AsQueryable();
                if (typeid.HasValue) bonsais = bonsais.Where(b => b.TypeId == typeid.Value).ToList();

                if (styleid.HasValue) bonsais = bonsais.Where(b => b.StyleId == styleid.Value).ToList();

                if (GeneralMeaningId.HasValue) bonsais = bonsais.Where(b => b.GeneralMeaningId == GeneralMeaningId.Value).ToList();

                if (sizeMin.HasValue && sizeMin >= 1) bonsais = bonsais.Where(b => b.Size >= sizeMin.Value).ToList();
                if (sizeMax.HasValue && sizeMax >= 1) bonsais = bonsais.Where(b => b.Size <= sizeMax.Value).ToList();

                if (yearOldMin.HasValue && yearOldMin >= 1) bonsais = bonsais.Where(b => b.YearOld >= yearOldMin.Value).ToList();
                if (yearOldMax.HasValue && yearOldMax >= 1) bonsais = bonsais.Where(b => b.YearOld <= yearOldMax.Value).ToList();

                if (!string.IsNullOrEmpty(search)) bonsais = bonsais.Where(b => b.BonsaiName.Contains(search) || b.Type.Name.Contains(search)).ToList();

                ViewData["Search"] = search;
                ViewData["TypeId"] = typeid;
                ViewData["StyleId"] = styleid;
                ViewData["GeneralMeaningId"] = GeneralMeaningId;
                ViewData["SizeMin"] = sizeMin;
                ViewData["SizeMax"] = sizeMax;
                ViewData["YearOldMin"] = yearOldMin;

                var types = await _context.Types.Select(t => new { t.Id, t.Name }).OrderBy(t => t.Name).ToListAsync();
                ViewData["TypeList"] = new SelectList(types, "Id", "Name");
                var styles = await _context.Styles.Select(s => new { s.Id, s.Name }).ToListAsync();
                ViewData["StyleList"] = new SelectList(styles, "Id", "Name");
                var generalMeanings = await _context.GeneralMeaning.Select(g => new { g.Id, g.Meaning }).ToListAsync();
                ViewData["GeneralMeaningList"] = new SelectList(generalMeanings, "Id", "Meaning");

                const int pageSize = 4;
                // Tính tổng số trang
                var totalCount = bonsais.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Lấy các bonsai đã phân trang
                var pagedBonsais = bonsais
                    .OrderBy(b => b.Id) // Có thể thay đổi thứ tự sắp xếp nếu cần
                    .Skip((page.Value - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Truyền totalPages vào ViewData để hiển thị trong View
                ViewData["TotalPages"] = totalPages;
                ViewData["CurrentPage"] = page;
                ViewData["totalBonsais"] = bonsais.Count();

                return View(pagedBonsais);
            }
            catch (HttpRequestException ex)
            {
                // Chuyển đổi các headers thành chuỗi
                var headersInfo = string.Join("; ", _httpClient.DefaultRequestHeaders
                    .Select(header => $"{header.Key}: {string.Join(", ", header.Value)}"));
                var thongBao = new ThongBao
                {
                    Message = $"Không thể tải dữ liệu từ API. Lỗi: {ex.Message}. Headers: {headersInfo}",
                    MessageType = "Danger",
                    DisplayTime = 10
                };

                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongBao);
                var types = await _context.Types.Select(t => new { t.Id, t.Name }).OrderBy(t => t.Name).ToListAsync();
                ViewData["TypeList"] = new SelectList(types, "Id", "Name");
                var styles = await _context.Styles.Select(s => new { s.Id, s.Name }).ToListAsync();
                ViewData["StyleList"] = new SelectList(styles, "Id", "Name");
                var generalMeanings = await _context.GeneralMeaning.Select(g => new { g.Id, g.Meaning }).ToListAsync();
                ViewData["GeneralMeaningList"] = new SelectList(generalMeanings, "Id", "Meaning");
                return View();
            }
        }

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
