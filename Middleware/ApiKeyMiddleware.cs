using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebsiteSellingBonsai.Middleware
{
	public class ApiKeyMiddleware
	{
		private readonly RequestDelegate _next;
		private const string ApiKeyHeaderName = "WebsiteSellingBonsai";

		public ApiKeyMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (context.Request.Path.StartsWithSegments("/api"))
			{
				// Kiểm tra mật khẩu
				if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var passApiKey))
				{
					context.Response.StatusCode = 401; // Unauthorized nếu không có mật khẩu

					//var headersList = string.Join("\n", context.Request.Headers.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}"));
					//await context.Response.WriteAsync($"API Key is missing.\n\nHeaders received:\n{headersList}"); danh sách header nhận được
					return;
				}

				var validApiKey = "kjasdfh32112"; // Mật khẩu hợp lệ

				if (passApiKey != validApiKey)
				{
					context.Response.StatusCode = 403; // Forbidden nếu mật khẩu không đúng
					await context.Response.WriteAsync("Unauthorized client.");
					return;
				}

				await _next(context); // Tiếp tục với pipeline nếu tất cả hợp lệ
			}
			else
			{
				await _next(context); // Tiếp tục với pipeline nếu tất cả hợp lệ
			}
		}
	}
}
