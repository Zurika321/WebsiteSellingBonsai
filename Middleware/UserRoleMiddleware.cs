using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebsiteSellingBonsaiAPI.Models;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS;

namespace WebsiteSellingBonsai.Middleware
{
	// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
	public class UserRoleMiddleware
	{
		private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;

        public UserRoleMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
		{
			_next = next;
            _httpClient = httpClientFactory.CreateClient();
            if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            }
            _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //Tự động đăng nhập nếu có cookie
            var login = context.Request.Cookies.Get<LoginDTO>("CokieUserWebsiteSellingBonsai");
            if (login != null)
            {
                var response = await _httpClient.GetAsync("https://localhost:44351/api/adminusersapi");

                if (response.IsSuccessStatusCode)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdminUser>>(
                        await response.Content.ReadAsStringAsync()
                    );

                    var result = data.FirstOrDefault(x => x.Username == login.Username && x.Password == login.Password);
                    if (result != null)
                    {
                        context.Session.Set<AdminUser>("userInfo", result);
                    }
                }
            }
            var userInfo = context.Session.Get<AdminUser>("userInfo");

            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                if (userInfo == null)
                {
                    if (!context.Request.Path.StartsWithSegments("/")
                        && !context.Request.Path.StartsWithSegments("/Admin/Users/Login")
                        && !context.Request.Path.StartsWithSegments("/Admin/Users/Sigin")
                        && !context.Request.Path.StartsWithSegments("/Admin/Users/Logout"))
                    {
                        // Lưu thông báo vào Session (thay vì TempData)
                        var thongBao = new ThongBao
                        {
                            Message = "Bạn cần đăng nhập để vào trang này",
                            MessageType = "Success",
                            DisplayTime = 3
                        };
                        context.Session.Set("ThongBao", thongBao);

                        context.Response.Redirect("/Admin/Users/Login");
                        return;
                    }
                }
                else
                {
                    // Quyền của User
                    if (userInfo.Role == "User")
                    {
                        if (context.Request.Path.StartsWithSegments("/Admin"))
                        {
                            if (!context.Request.Path.StartsWithSegments("/Admin/Users/Login")
                                && !context.Request.Path.StartsWithSegments("/Admin/Users/Sigin")
                                && !context.Request.Path.StartsWithSegments("/Admin/Users/Logout"))
                            {
                                var thongBao = new ThongBao
                                {
                                    Message = "Bạn không có quyền truy cập trang này",
                                    MessageType = "Warning",
                                    DisplayTime = 3
                                };
                                context.Session.Set("ThongBao", thongBao);

                                context.Response.Redirect("/");
                                return;
                            }
                        }
                    }
                    // Quyền của Admin
                    else if (userInfo.Role == "Admin")
                    {
                        //all lane
                    }
                }
            }

            await _next(context);
        }
    }
}
