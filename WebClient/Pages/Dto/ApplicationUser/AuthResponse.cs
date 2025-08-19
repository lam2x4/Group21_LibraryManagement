namespace WebClient.Pages.Dto.ApplicationUser
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Expiration { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
    }

}
