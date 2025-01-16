using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text;
using WebsiteSellingBonsai.Middleware;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingMiniBonsai
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var MyPolicies = "_Policies"; // CorsPolicy
            var AllowLocalhost = "AllowLocalhost";
            var builder = WebApplication.CreateBuilder(args);

            //Thêm Database MiniBonsaiDB
            builder.Services.AddDbContext<MiniBonsaiDBAPI>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký DbContext cho Identity
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Thêm dịch vụ Identity để quản lý người dùng và vai trò
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Cấu hình JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JWT");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["ValidIssuer"],
                    ValidAudience = jwtSettings["ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(UserRoles.AdminOrUser, policy =>
                     policy.RequireAssertion(context =>
                         context.User.IsInRole(UserRoles.Admin) ||
                         context.User.IsInRole(UserRoles.User)
                     )
                );
                options.AddPolicy(UserRoles.User, policy => policy.RequireRole(UserRoles.User));
                options.AddPolicy(UserRoles.Admin, policy => policy.RequireRole(UserRoles.Admin));
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = "sub";
            });


            // Đăng ký AuthService làm dịch vụ
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.AddScoped<IUrlService, UrlService>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<EmailSender>();
            builder.Services.AddScoped<ICaptchaService, CaptchaService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<APIServices>();
            //builder.Services.AddTransient<IEmailSender, EmailSender>();
            //builder.Services.AddScoped<IEmailSender, EmailSender>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyPolicies,
                    builder =>
                    {
                        builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
            });

            builder.Services.AddHttpClient();
            //builder.Services.AddHttpClient("MyClient", client =>
            //{
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_token");
            //});

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

            // Thêm d?ch v? Controllers và Views
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();
            //app.UseMiddleware<ApiKeyMiddleware>();
            app.UseMiddleware<UserRoleMiddleware>();

            // Sử dụng Authentication và Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            app.UseCors(MyPolicies);

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}
