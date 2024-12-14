using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("AdminUsers")]
    public class AdminUser //: BaseModel
    {
        [Key]
        public int USE_ID { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? Displayname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }
    }
}
