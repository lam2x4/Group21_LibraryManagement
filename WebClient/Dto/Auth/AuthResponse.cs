namespace WebClient.Pages.Dto.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
