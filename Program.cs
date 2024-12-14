using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsai.Middleware;

namespace WebsiteSellingMiniBonsai
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var MyPolicies = "_Policies";
            var AllowLocalhost = "AllowLocalhost";
            var builder = WebApplication.CreateBuilder(args);

            //Thêm Database MiniBonsaiDB
            builder.Services.AddDbContext<MiniBonsaiDBAPI>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1); // Th?i gian h?t h?n session
                options.Cookie.HttpOnly = true; // ??m b?o cookie ch? có th? truy c?p qua HTTP
                options.Cookie.IsEssential = true; // ??m b?o cookie ???c s? d?ng cho session
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
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();
            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            app.UseCors(MyPolicies);

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
