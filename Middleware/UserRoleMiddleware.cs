using WebsiteSellingBonsaiAPI.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebsiteSellingBonsaiAPI.Models;

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

		public async Task InvokeAsync(HttpContext context)
		{
			// Kiểm tra nếu là yêu cầu tới API hoặc trang khác ngoài trang Index của Home
			//	var userInfo = context.Session.Get<AdminUser>("userInfo");

			//	// Nếu không có userInfo trong session
			//	if (userInfo == null)
			//	{
			//		// Nếu yêu cầu không phải là trang Home/Index, chuyển hướng về trang Index của Home
			//		if (!context.Request.Path.StartsWithSegments("/Home/Index"))
			//		{
			//			context.Response.Redirect("/Home/Index");
			//			return;
			//		}
			//	}
			//	else
			//	{
			//		// Kiểm tra phân quyền, có thể tuỳ vào role của user trong userInfo
			//		if (userInfo.Role == "Guest")
			//		{
			//			// Có thể cho phép guest vào một số trang nhất định, nếu muốn
			//			// Ví dụ như: Nếu Guest không có quyền truy cập vào trang Admin, có thể chuyển hướng về một trang khác
			//			if (context.Request.Path.StartsWithSegments("/Admin"))
			//			{
			//				context.Response.Redirect("/Home/Index");
			//				return;
			//			}
			//		}
			//		// Có thể kiểm tra các vai trò khác nếu cần, như User, Admin...
			//	}

			//	await _next(context); // Tiếp tục với pipeline nếu mọi thứ hợp lệ
		}
	}
}
