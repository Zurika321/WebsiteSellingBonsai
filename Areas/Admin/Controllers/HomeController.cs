using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.Orders;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;
        //private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(MiniBonsaiDBAPI context/*, UserManager<ApplicationUser> userManager*/)
        {
            _context = context;
            //_userManager = userManager;
        }
        public async Task<IActionResult> Index([FromServices] UserManager<ApplicationUser> userManager)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            // Lọc các đơn hàng theo trạng thái
            var notConfirmedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.NotConfirmed)
                .ToListAsync();

            var onDeliveryOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.On_Delivery)
                .ToListAsync();

            var orderCompletedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.Order_Completed)
                .ToListAsync();

            var orderCancelledOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.Order_Cancelled)
                .ToListAsync();

            // Tạo một đối tượng chứa các danh sách đơn hàng theo trạng thái
            //var model = new OrderStatusViewModel
            //{
            //    NotConfirmedOrders = notConfirmedOrders,
            //    OnDeliveryOrders = onDeliveryOrders,
            //    OrderCompletedOrders = orderCompletedOrders,
            //    OrderCancelledOrders = orderCancelledOrders
            //};

            // Tính tổng doanh thu trong ngày (chia theo các mốc thời gian: 0-4 giờ, 4-8 giờ, ...)
            var today = DateTime.Today;
            var revenueToday = new int[6]; // Chứa doanh thu cho 6 mốc (0-4h, 4-8h, ..., 20-24h)
            var revenueThisWeek = new int[7]; // Doanh thu theo ngày trong tuần
            var revenueThisMonth = new int[12]; // Doanh thu theo tháng trong năm
            var revenueThisYear = new int[12]; // Doanh thu theo tháng trong năm

            foreach (var order in orderCompletedOrders)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var orderDate = order.UpdatedDate;
                    var totalPrice = detail.TotalPrice;
                    // Kiểm tra nếu orderDate có giá trị
                    if (orderDate.HasValue)
                    {
                        var date = orderDate.Value; // Lấy giá trị của orderDate (non-nullable)
                        // Tính doanh thu cho hôm nay (chia theo các mốc thời gian 4 tiếng)
                        if (date.Date == today)
                        {
                            int timeSlot = (date.Hour / 4); // 0-4h => 0, 4-8h => 1, ...
                            revenueToday[timeSlot] += (int)totalPrice;
                        }
                        // Tính doanh thu trong tuần (theo ngày trong tuần)
                        if (date >= today.AddDays(-(int)today.DayOfWeek) && date <= today.AddDays(6 - (int)today.DayOfWeek))
                        {
                            int dayOfWeek = (int)date.DayOfWeek; // Chủ nhật = 0, Thứ hai = 1, ...
                            revenueThisWeek[dayOfWeek] += (int)totalPrice;
                        }
                        // Tính doanh thu trong tháng
                        if (date.Month == today.Month)
                        {
                            int month = date.Month - 1; // Index tháng từ 0-11
                            revenueThisMonth[month] += (int)totalPrice;
                        }
                        // Tính doanh thu trong năm
                        if (date.Year == today.Year)
                        {
                            int month = date.Month - 1; // Index tháng từ 0-11
                            revenueThisYear[month] += (int)totalPrice;
                        }
                    }
                }
            }

            var model = new
            {
                RevenueData = new
                {
                    today = revenueToday,
                    thisWeek = revenueThisWeek,
                    thisMonth = revenueThisMonth,
                    thisYear = revenueThisYear
                },
                Labels = new
                {
                    today = new[] { "12am-4am", "4am-8am", "8am-12pm", "12pm-4pm", "4pm-8pm", "8pm-12am" },
                    thisWeek = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" },
                    thisMonth = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
                    thisYear = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
                }
            };

            // Tổng doanh thu hôm nay từ các đơn hàng đã hoàn thành
            var totalRevenueToday = orderCompletedOrders
                .Where(o => o.UpdatedDate?.Date == today)
                .Sum(o => o.Total);

            // Tổng số đơn hàng chưa xác nhận hôm nay
            var totalNotConfirmedToday = notConfirmedOrders
                .Count(o => o.UpdatedDate?.Date == today);

            //Tổng đơn hàng đang vận chuyển hôm nay
            var totalonDeliveryOrdersToday = onDeliveryOrders
                .Count(o => o.UpdatedDate?.Date == today);

            //Tổng đơn hàng hoàn thành hôm nay
            var totalorderCompletedOrdersToday = orderCompletedOrders
                .Count(o => o.UpdatedDate?.Date == today);

            //Tổng đơn hàng bị hủy hôm nay
            var totalorderCancelledOrdersToday = orderCancelledOrders
               .Count(o => o.UpdatedDate?.Date == today);

            // Tổng số khách hàng mới hôm nay
            var totalNewClientsToday = await userManager.Users
                .Where(u => u.CreatedDate.Date == today)
                .CountAsync();

            ViewData["TotalRevenueToday"] = totalRevenueToday;

            ViewData["TotalNotConfirmedToday"] = totalNotConfirmedToday;
            ViewData["totalonDeliveryOrdersToday"] = totalonDeliveryOrdersToday;
            ViewData["totalorderCompletedOrdersToday"] = totalorderCompletedOrdersToday;
            ViewData["totalorderCancelledOrdersToday"] = totalorderCancelledOrdersToday;

            ViewData["TotalNewClientsToday"] = totalNewClientsToday;

            return View(model);
        }
    }
}
