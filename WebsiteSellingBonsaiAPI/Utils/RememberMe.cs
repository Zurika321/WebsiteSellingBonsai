using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebsiteSellingBonsaiAPI.Utils
{
    public static class CookieExtensions
    {
        // Phương thức Append để lưu trữ đối tượng vào cookie với JSON serialization
        public static IResponseCookies Append<T>(this IResponseCookies cookies, string key, T value, CookieOptions options)
        {
            try
            {
                // Serialize đối tượng thành chuỗi JSON và lưu vào cookie
                var serializedValue = JsonSerializer.Serialize(value);
                cookies.Append(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                // Log exception nếu cần thiết (hoặc xử lý riêng)
                throw new InvalidOperationException($"Error appending cookie for key '{key}': {ex.Message}");
            }

            return cookies;
        }

        // Phương thức Get để lấy đối tượng từ cookie với JSON deserialization
        public static T? Get<T>(this IRequestCookieCollection cookies, string key)
        {
            try
            {
                var value = cookies[key];
                // Nếu cookie không tồn tại hoặc là null, trả về giá trị mặc định
                if (string.IsNullOrEmpty(value))
                {
                    return default;
                }

                // Deserialize chuỗi JSON thành đối tượng
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                // Log exception nếu cần thiết (hoặc xử lý riêng)
                throw new InvalidOperationException($"Error retrieving cookie for key '{key}': {ex.Message}");
            }
        }
    }
}
