using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;
using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using WebsiteSellingBonsaiAPI.DTOS.User;
using System.Net.Http;
using Azure;
using Microsoft.AspNetCore.Identity.UI.Services;
using NuGet.Common;
using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.IdentityModel.Tokens;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserManagerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly APIServices _apiServices;

        public UserManagerController(UserManager<ApplicationUser> userManager, APIServices apiServices)
        {
            _userManager = userManager;
            _apiServices = apiServices;
        }

        // GET: Admin/AdminUsers
        public async Task<IActionResult> Index()
        {
            var (users , thongbao) = await _apiServices.FetchDataApiGetList<ApplicationUserDTO>("authenticate/getlistusers");
            if (users == default)
            {
                TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(thongbao);
                users = new List<ApplicationUserDTO>();
            }
            return View(users);
        }
    }
}
