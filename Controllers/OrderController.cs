using Microsoft.AspNetCore.Mvc;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.Orders;

namespace WebsiteSellingBonsai.Controllers
{
    public class OrderController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;

        // Constructor sửa lại tên hàm
        public OrderController(APIServices processingServices, MiniBonsaiDBAPI context)
        {
            _apiServices = processingServices;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUser>("userInfo");

            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            // Lọc các đơn hàng theo trạng thái
            var notConfirmedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.NotConfirmed && o.USE_ID == userInfo.Id)
                .ToListAsync();

            var cart = await _context.Carts
                .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Bonsai)
                .FirstOrDefaultAsync(c => c.USE_ID == userInfo.Id);

            var onDeliveryOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.On_Delivery && o.USE_ID == userInfo.Id)
                .ToListAsync();

            var orderCompletedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.Order_Completed && o.USE_ID == userInfo.Id)
                .ToListAsync();

            var orderCancelledOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.Order_Cancelled && o.USE_ID == userInfo.Id)
                .ToListAsync();

            // Tạo một đối tượng chứa các danh sách đơn hàng theo trạng thái
            var model = new OrderStatusViewModel
            {
                NotConfirmedOrders = notConfirmedOrders,
                OnDeliveryOrders = onDeliveryOrders,
                OrderCompletedOrders = orderCompletedOrders,
                OrderCancelledOrders = orderCancelledOrders
            };

            return View(model);
        }

        [HttpGet("Order/ViewOrder/{ORDER_ID}")]
        public async Task<IActionResult> ViewOrder(int ORDER_ID)
        {
            if (ORDER_ID == null)
            {
                return RedirectToAction("Index");
            }
            var userInfo = HttpContext.Session.Get<ApplicationUser>("userInfo");

            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Bonsai)
                .FirstOrDefaultAsync(o => o.ORDER_ID == ORDER_ID);

            if (order == null)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Đơn hàng không tồn tại",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });

                return RedirectToAction("Index");
            }

            if (order.USE_ID != userInfo.Id) 
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Đơn hàng không tồn tại",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });

                return RedirectToAction("Index");
            }

            return View(order);
        }
    }
}
