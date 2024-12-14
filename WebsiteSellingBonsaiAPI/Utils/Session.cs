using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebsiteSellingBonsaiAPI.Utils
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            // Chuyển đối tượng thành chuỗi JSON và lưu vào session
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            // Lấy chuỗi JSON từ session và chuyển thành đối tượng
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
