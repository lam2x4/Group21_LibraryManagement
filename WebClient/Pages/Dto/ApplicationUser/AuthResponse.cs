namespace WebClient.Pages.Dto.ApplicationUser
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Expiration { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; } // Add UserId
        public List<string> Roles { get; set; }
    }

}
