using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebsiteSellingBonsaiAPI.Models;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS;
using Azure;
using WebsiteSellingBonsaiAPI.DTOS.User;
using System.Net.Http.Headers;
using NuGet.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.Carts;

namespace WebsiteSellingBonsai.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class UserRoleMiddleware
    {
        private readonly RequestDelegate _next;

        public UserRoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //public bool IsTokenValid(string token)
        //{
        //    try
        //    {
        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var key = Convert.FromBase64String("JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr"); // Replace with your actual secret key
        //        var validationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuer = true,
        //            ValidateAudience = true,
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            ValidIssuer = "http://localhost:5000",
        //            ValidAudience = "http://localhost:4200",
        //            IssuerSigningKey = new SymmetricSecurityKey(key)
        //        };

        //        tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        //        return true;
        //    }
        //    catch (SecurityTokenValidationException)
        //    {
        //        // token ko hợp lệ
        //        return false;
        //    }
        //    catch (Exception)
        //    {
        //        // ngoại lệ khác
        //        return false;
        //    }
        //}

        //var token = context.Session.GetString("AuthToken");
        //if (string.IsNullOrEmpty(token) || !IsTokenValid(token))
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var apiServices = scope.ServiceProvider.GetRequiredService<APIServices>();

        //        var (Success, thongbao, newtoken) = await apiServices.Login(login);
        //        if (!Success)
        //        {
        //            context.Session.Set("ThongBao", thongbao);
        //        }
        //    }
        //}
        public bool Phan_Quyen(List<string> status, string url)
        {
            //if (status == null || status.Count == 0 || string.IsNullOrEmpty(url)) return true;

            url = url.ToLower();

            if (!url.StartsWith("/api"))
            {
                // Trạng thái "NoLogin" (Chưa đăng nhập)
                if (status.Contains("NoLogin"))
                {
                    // Chỉ được truy cập trang chính hoặc các trang đăng nhập liên quan
                    if (url == "/" ||
                        url.StartsWith("/admin/users") ||
                        url.StartsWith("/bonsai") ||
                        url == "/cart/addcart" ||
                        url == "/createpayment" ||
                        url == "/favourite/addfavorite")
                    {
                        return false;
                    }
                    return true;
                }

                // Trạng thái "User" (Đăng nhập với vai trò User)
                if (status.Contains(UserRoles.User))
                {
                    // Không được truy cập khu vực "Admin" ngoại trừ các trang liên quan đến đăng nhập
                    if (url.StartsWith("/admin") &&
                        !(url.StartsWith("/admin/users/login") ||
                          url.StartsWith("/admin/users/sigin") ||
                          url.StartsWith("/admin/users/confirmemail") ||
                          url.StartsWith("/admin/users/logout")))
                    {
                        return true;
                    }
                    return false; // Các trang khác ngoài "Admin" được phép truy cập
                }

                // Trạng thái "Admin" (Đăng nhập với vai trò Admin)
                if (status.Contains(UserRoles.Admin))
                {
                    if (status.Contains("Bonsai") && !(url.StartsWith("/admin/users/login") ||
                          url.StartsWith("/admin/users/sigin") ||
                          url.StartsWith("/admin/users/confirmemail") ||
                          url.StartsWith("/admin/users/logout")))
                    {
                        return true;
                    }
                    return false; // Admin có toàn quyền truy cập
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
        {
            // Tự động đăng nhập nếu có cookie
            var login = context.Request.Cookies.Get<LoginModel>("CokieUserWebsiteSellingBonsai");
            var userInfo = context.Session.Get<ApplicationUserDTO>("userInfo");

            if (login != null)
            {
                var tokenSesion = context.Session.GetString("AuthToken");
                if (tokenSesion == null)
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var apiServices = scope.ServiceProvider.GetRequiredService<APIServices>();

                        var (Success, thongbao, token) = await apiServices.Login(login);
                        if (!Success)
                        {
                            context.Session.Set("ThongBao", thongbao);
                        }
                        userInfo = context.Session.Get<ApplicationUserDTO>("userInfo");
                    }
                }
            }

            var url = context.Request.Path.ToString();
            List<string> roles = userInfo?.Role?.ToList() ?? new List<string>();

            if (userInfo == null)
            {
                if (Phan_Quyen(new List<string> { "NoLogin" }, url))
                {
                    var currentUrl = context.Request.Path + context.Request.QueryString;
                    context.Session.SetString("ReturnUrl", currentUrl);
                    context.Response.Redirect("/Admin/Users/Login");
                    return;
                }
            }
            else
            {
                if (Phan_Quyen(roles, url))
                {
                    var thongbao = new ThongBao
                    {
                        Message = "Bạn không có quyền truy cập trang này!",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5,
                    };
                    context.Session.Set("ThongBao", thongbao);
                    context.Response.Redirect("/");
                    return;
                }
            }

            await _next(context); // Tiến hành xử lý tiếp theo trong pipeline
        }

        //if (!context.Request.Path.StartsWithSegments("/api"))
        //{
        //    if (userInfo == null)
        //    {
        //        if (!context.Request.Path.StartsWithSegments("/")
        //            && !context.Request.Path.StartsWithSegments("/Admin/Users/Login")
        //            && !context.Request.Path.StartsWithSegments("/Admin/Users/Sigin")
        //            && !context.Request.Path.StartsWithSegments("/Admin/Users/Logout"))
        //        {
        //            // Lưu thông báo vào Session (thay vì TempData)
        //            var thongBao = new ThongBao
        //            {
        //                Message = "Bạn cần đăng nhập để vào trang này",
        //                MessageType = "Success",
        //                DisplayTime = 3
        //            };
        //            context.Session.Set("ThongBao", thongBao);

        //            context.Response.Redirect("/Admin/Users/Login");
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        // Quyền của User
        //        if (userInfo.Role == "User")
        //        {
        //            if (context.Request.Path.StartsWithSegments("/Admin"))
        //            {
        //                if (!context.Request.Path.StartsWithSegments("/Admin/Users/Login")
        //                    && !context.Request.Path.StartsWithSegments("/Admin/Users/Sigin")
        //                    && !context.Request.Path.StartsWithSegments("/Admin/Users/Logout"))
        //                {
        //                    var thongBao = new ThongBao
        //                    {
        //                        Message = "Bạn không có quyền truy cập trang này",
        //                        MessageType = "Warning",
        //                        DisplayTime = 3
        //                    };
        //                    context.Session.Set("ThongBao", thongBao);

        //                    context.Response.Redirect("/");
        //                    return;
        //                }
        //            }
        //        }
        //        // Quyền của Admin
        //        else if (userInfo.Role == "Admin")
        //        {
        //            //all lane
        //        }
        //    }
        //}
    }
}
