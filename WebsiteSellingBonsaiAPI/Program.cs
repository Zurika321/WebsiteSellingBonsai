
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
            var MyPolicies = "_Policies";
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // Dùng để hỗ trợ vòng lặp
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyPolicies,
                    //builder =>
                    //{
                    //	builder.WithOrigins("https://localhost:44345")  // Chỉ cho phép nguồn này
                    //		.WithHeaders("Role", "X-API-KEY") // Cho phép header Role và mật khẩu
                    //		.AllowAnyMethod(); // Cho phép tất cả các phương thức HTTP
                    //});
                    builder =>
                    {
                        builder.SetIsOriginAllowed(_ => true) // Cho phép tất cả nguồn gốc
                        .AllowAnyHeader()                // Cho phép tất cả header
                        .AllowCredentials()              // Cho phép gửi cookie/credentials
                        .AllowAnyMethod();               // Cho phép tất cả HTTP method (GET, POST, DELETE, v.v.)
                    });
            });


            builder.Services.AddDbContext<MiniBonsaiDBAPI>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors(MyPolicies);
            app.MapControllers();

            app.Run();
        }
	}
}
