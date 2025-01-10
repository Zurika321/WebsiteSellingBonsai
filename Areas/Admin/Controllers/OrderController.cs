using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Utils;
using WebsiteSellingBonsaiAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Reflection;
using Azure;
using Microsoft.AspNetCore.Authorization;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using Microsoft.AspNetCore.Identity;
using System.Threading.Channels;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;

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

            var notConfirmedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.NotConfirmed)
                .ToListAsync();
            return View(notConfirmedOrders);
        }
        [HttpGet]
        public async Task<IActionResult> On_Delivery() 
        {
            var userInfo = HttpContext.Session.Get<ApplicationUser>("userInfo");
            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            var onDeliveryOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.On_Delivery)
                .ToListAsync();
            return View(onDeliveryOrders);
        }
        [HttpGet]
        public async Task<IActionResult> Order_Completed()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUser>("userInfo");
            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            var orderCompletedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.Order_Completed)
                .ToListAsync();

            return View(orderCompletedOrders);
        }
        [HttpGet]
        public async Task<IActionResult> Order_Cancelled()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUser>("userInfo");
            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            var orderCancelledOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Bonsai)
                .Where(o => o.Status == StatusOrder.Order_Cancelled)
                .ToListAsync();
            return View(orderCancelledOrders);
        }
        [HttpGet("Admin/Order/ViewOrder/{ORDER_ID}")]
        public async Task<IActionResult> ViewOrder(int ORDER_ID)
        {
            if (ORDER_ID <= 0)
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
            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> Comfirm(int ORDER_ID)
        {
            if (ORDER_ID <= 0)
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

            if (order.Status != StatusOrder.NotConfirmed)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Không thể thay đổi trạng thái của đơn hàng!",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });
                return RedirectToAction("Index");
            }

            order.Status = StatusOrder.On_Delivery;
            order.UpdatedDate = DateTime.Now.Date;
            order.UpdatedBy = userInfo.UserName;

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
            {
                Message = "Xác nhận đơn hàng thành công!",
                MessageType = TypeThongBao.Success,
                DisplayTime = 5,
            });
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> ComfirmCompleted(int ORDER_ID)
        {
            if (ORDER_ID <= 0)
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

            if (order.Status != StatusOrder.On_Delivery)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Không thể thay đổi trạng thái của đơn hàng!",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });
                return RedirectToAction("Index");
            }

            order.Status = StatusOrder.Order_Completed;
            order.UpdatedDate = DateTime.Now.Date;
            order.UpdatedBy = userInfo.UserName;

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
            {
                Message = "Xác nhận đơn hàng đã giao thành công!",
                MessageType = TypeThongBao.Success,
                DisplayTime = 5,
            });
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int ORDER_ID,string cancelReason,string LinkUrl)
        {
            if (ORDER_ID <= 0 || string.IsNullOrEmpty(cancelReason))
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
                if (!string.IsNullOrEmpty(LinkUrl)) return Redirect(LinkUrl);
                return RedirectToAction("Index");
            }

            if (order.Status == StatusOrder.Order_Completed || order.Status == StatusOrder.Order_Cancelled)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Không thể thay đổi trạng thái của đơn hàng!",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });
                if (!string.IsNullOrEmpty(LinkUrl)) return Redirect(LinkUrl);
                return RedirectToAction("Index");
            }

            order.Status = StatusOrder.Order_Cancelled;
            order.CancelReason = cancelReason;
            order.UpdatedDate = DateTime.Now.Date;
            order.UpdatedBy = userInfo.UserName;

            foreach (var od in order.OrderDetails)
            {
                var product = await _context.Bonsais
                            .FirstOrDefaultAsync(b => b.Id == od.BONSAI_ID);

                if (product != null)
                {
                    product.Quantity += od.Quantity;
                }
                else
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = $"Không tìm thấy sản phẩm với ID: {od.BONSAI_ID}.",
                        MessageType = TypeThongBao.Danger,
                        DisplayTime = 5,
                    });
                    if (!string.IsNullOrEmpty(LinkUrl)) return Redirect(LinkUrl);
                    return RedirectToAction("Index");
                }
            }

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
            {
                Message = "Đơn hàng vừa được hủy!",
                MessageType = TypeThongBao.Warning,
                DisplayTime = 5,
            });
            if (!string.IsNullOrEmpty(LinkUrl)) return Redirect(LinkUrl);
            return RedirectToAction("Index");
        }
    }
}
