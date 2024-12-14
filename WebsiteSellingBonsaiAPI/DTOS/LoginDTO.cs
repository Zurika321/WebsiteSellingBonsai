namespace WebsiteSellingBonsaiAPI.DTOS
{
    public class LoginDTO
    {
        public required string Password { get; set; }
        public string? Username { get; set; }
        public bool RememberMe { get; set; }
    }
}
