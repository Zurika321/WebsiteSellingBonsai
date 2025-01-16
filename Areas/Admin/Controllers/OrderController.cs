//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Drawing;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using Newtonsoft.Json;
//using WebsiteSellingBonsaiAPI.DTOS;
//using WebsiteSellingBonsaiAPI.Models;
//using static System.Runtime.InteropServices.JavaScript.JSType;
//using System.IO;
//using System.Reflection;
//using Azure;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using System.Threading.Channels;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using NuGet.Common;
using Microsoft.AspNetCore.Mvc;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;
        private readonly EmailSender _emailSender;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlService _urlService;

        public OrderController(APIServices processingServices,
            MiniBonsaiDBAPI context,
            EmailSender emailSender,
            IActionContextAccessor actionContextAccessor,
            IUrlService urlService
            )
        {
            _apiServices = processingServices; 
            _context = context;
            _emailSender = emailSender;
            _actionContextAccessor = actionContextAccessor;
            _urlService = urlService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
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
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
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
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
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
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
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
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

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
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

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
            else
            {
                foreach (var detail in order.OrderDetails)
                {
                    var bonsai = await _context.Bonsais.FindAsync(detail.BONSAI_ID);
                    if (bonsai == null)
                    {
                        TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                        {
                            Message = $"Không tìm thấy Bonsai với ID: {detail.BONSAI_ID}",
                            MessageType = TypeThongBao.Warning,
                            DisplayTime = 5,
                        });
                    }

                    // Trừ số lượng của Bonsai
                    if (bonsai.Quantity < detail.Quantity)
                    {
                        TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                        {
                            Message = $"Số lượng Bonsai không đủ {detail.Quantity} sản phẩm.{bonsai.BonsaiName} còn lại: {bonsai.Quantity} sản phẩm",
                            MessageType = TypeThongBao.Warning,
                            DisplayTime = 5,
                        });
                    }

                    bonsai.Quantity -= detail.Quantity;
                    _context.Update(bonsai);
                }
            }

            order.Status = StatusOrder.On_Delivery;
            order.UpdatedDate = DateTime.Now;
            order.UpdatedBy = userInfo.UserName;

            var (user, thongbaogetuser) = await _apiServices.FetchDataApiGet<ApplicationUserDTO>($"Authenticate/getuserinfobyid?id={order.USE_ID}");
            if (user == null)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaogetuser);
                return RedirectToAction("Index");
            }
            
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
            {
                Message = "Xác nhận đơn hàng thành công!",
                MessageType = TypeThongBao.Success,
                DisplayTime = 5,
            });

            SendGmail(user.Email, "Đơn hàng đang được vận chuyển WebsiteSellingBonsai",
            $"Đơn hàng với mã đơn hàng {ORDER_ID} của bạn đang được vận chuyển",
            ORDER_ID);
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> ComfirmCompleted(int ORDER_ID)
        {
            if (ORDER_ID <= 0)
            {
                return RedirectToAction("Index");
            }
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

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
            order.UpdatedDate = DateTime.Now;
            order.UpdatedBy = userInfo.UserName;

            var (user, thongbaogetuser) = await _apiServices.FetchDataApiGet<ApplicationUserDTO>($"Authenticate/getuserinfobyid?id={order.USE_ID}");
            if (user == null)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaogetuser);
                return RedirectToAction("Index");
            }

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
            {
                Message = "Xác nhận đơn hàng đã giao thành công!",
                MessageType = TypeThongBao.Success,
                DisplayTime = 5,
            });

            SendGmail(user.Email, "Đơn hàng đã giao thành công trên WebsiteSellingBonsai",
            $"Đơn hàng với mã đơn hàng {ORDER_ID} của bạn đã giao thành công",
            ORDER_ID);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int ORDER_ID, string cancelReason, string LinkUrl)
        {
            if (ORDER_ID <= 0 || string.IsNullOrEmpty(cancelReason))
            {
                return RedirectToAction("Index");
            }
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

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
            order.UpdatedDate = DateTime.Now;
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
            var (user, thongbaogetuser) = await _apiServices.FetchDataApiGet<ApplicationUserDTO>($"Authenticate/getuserinfobyid?id={order.USE_ID}");

            if (user == null) { 
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbaogetuser);
                return RedirectToAction("Index");
            }
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
            {
                Message = "Đơn hàng vừa được hủy!",
                MessageType = TypeThongBao.Warning,
                DisplayTime = 5,
            });

            SendGmail(user.Email, "Đơn hàng bị hủy trên WebsiteSellingBonsai",
            $"Đơn hàng của bạn đã bị hủy vì lý do: {cancelReason}",
            ORDER_ID);
            if (!string.IsNullOrEmpty(LinkUrl)) return Redirect(LinkUrl);
            return RedirectToAction("Index");
        }
        public async void SendGmail(string email,string title,string body,int ORDER_ID)
        {
            var actionContext = _actionContextAccessor.ActionContext;
            var confirmationLink = _urlService.GenerateUrl(
                                action: "ViewOrder",
                                controller: "Order",
                                values: new { ORDER_ID = ORDER_ID },
                                area: "",
                                scheme: actionContext.HttpContext.Request.Scheme
            );

            await _emailSender.SendEmailAsync(email,
                title,
                $"{body} <a href='{confirmationLink}'>Xem chi tiết</a>"
             );
        }
    }
}
