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

        public bool IsTokenValid(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Convert.FromBase64String("JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr"); // Replace with your actual secret key
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://localhost:5000",
                    ValidAudience = "http://localhost:4200",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (SecurityTokenValidationException)
            {
                // token ko hợp lệ
                return false;
            }
            catch (Exception)
            {
                // ngoại lệ khác
                return false;
            }
        }

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
        {
            //Tự động đăng nhập nếu có cookie
            var login = context.Request.Cookies.Get<LoginModel>("CokieUserWebsiteSellingBonsai");
            if (login != null)
            {
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
                var userInfo = context.Session.Get<ApplicationUser>("userInfo");
                if (userInfo == null)
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var apiServices = scope.ServiceProvider.GetRequiredService<APIServices>();

                        var (Success, thongbao, token) = await apiServices.Login(login);
                        if (!Success)
                        {
                            context.Session.Set("ThongBao", thongbao);
                        }
                    }
                }
            }
            await _next(context);
            //var tokenBytes = context.Session.Get("AuthToken");
            //if (tokenBytes != null)
            //{
            //    var token2 = System.Text.Encoding.UTF8.GetString(tokenBytes);
            //    if (!string.IsNullOrEmpty(token2))
            //    {
            //        context.Session.Set("ThongBao", new ThongBao
            //        {
            //            Message = token2,
            //            MessageType = TypeThongBao.Success,
            //            DisplayTime = 5,
            //        });
            //    }
            //}
            //}
            //var userInfo = context.Session.Get<ApplicationUser>("userInfo");

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
}
