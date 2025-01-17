﻿using Microsoft.AspNetCore.Mvc;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.Orders;
using System.Text.Json;
using WebsiteSellingBonsaiAPI.DTOS.User;

namespace WebsiteSellingBonsai.Controllers
{
    public class PaymentController : Controller
    {
        private readonly APIServices _apiServices;
        private readonly MiniBonsaiDBAPI _context;

        // Constructor sửa lại tên hàm
        public PaymentController(APIServices processingServices, MiniBonsaiDBAPI context)
        {
            _apiServices = processingServices;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            var order = HttpContext.Session.Get<Order>("Payment_Order");

            if (order == null)
            {
                return RedirectToAction("NoOrder");
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> NoOrder()
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            return View();
        }

        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment(Create_order create_Order,string redirectUrl)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if(create_Order.Address == "Không có địa chỉ")
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Vui lòng nhập địa chỉ",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });
                return Redirect(redirectUrl ?? Url.Action("Index", "Home"));
            }    

            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            var (Success, thongbao) = await _apiServices.Create_Session_Payment(create_Order);
            if (!Success)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
                if (thongbao.Message == "Vui lòng nhập địa chỉ để giao hàng" || thongbao.Message.EndsWith("không đủ số lượng trong kho."))
                {
                    return Redirect(redirectUrl ?? Url.Action("Index", "Home"));
                }
                    
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index");
        }

        [HttpPost("Create_Order")]
        public async Task<IActionResult> Create_Order(/*string paymentMethod*/)
        {
            var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");

            if (userInfo == null)
                return RedirectToAction("Login", "Users", new { area = "Admin" });

            var order = HttpContext.Session.Get<Order>("Payment_Order");

            if (order == null)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                {
                    Message = "Session Payment của bạn vừa hết hạn vui lòng thanh toán lại!",
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5,
                });
                return RedirectToAction("NoOrder");
            }

            var (Success, thongbao) = await _apiServices.FetchDataApiPost<Order>("OrdersAPI/create_order", order);
            TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);

            if (HttpContext.Session.Get<Order>("Payment_Order") != null)
            {
                HttpContext.Session.Remove("Payment_Order");
            }

            return RedirectToAction("Index" , "Order");
        }
    }
}
