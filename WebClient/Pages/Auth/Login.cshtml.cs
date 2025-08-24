using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;
using WebClient.Pages.Dto.ApplicationUser;
using Microsoft.Extensions.Configuration;
using WebClient.Pages.Dto.Auth; 

namespace WebClient.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public InputModel Input { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient("Api");
            var apiUrl = "api/Auth/login";

            var loginData = new
            {
                email = Input.Email,
                password = Input.Password
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (!string.IsNullOrEmpty(result?.Token))
                {
                    Console.WriteLine($"Login successful - Token: {result.Token?.Substring(0, 20)}..., UserId: {result.UserId}");
                    
                    // Lưu token vào Cookie
                    HttpContext.Response.Cookies.Append("JWToken", result.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false, // Set to false for development
                        SameSite = SameSiteMode.Lax, // More permissive for development
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    // Save UserId to cookie
                    if (!string.IsNullOrEmpty(result.UserId))
                    {
                        Console.WriteLine($"Saving UserId cookie: {result.UserId}");
                        HttpContext.Response.Cookies.Append("UserId", result.UserId, new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = false, // Set to false for development
                            SameSite = SameSiteMode.Lax, // More permissive for development
                            Expires = DateTimeOffset.UtcNow.AddHours(1)
                        });
                    }

                    if (result.Roles != null && result.Roles.Any())
                    {
                        HttpContext.Response.Cookies.Append("UserRoles",
                            string.Join(",", result.Roles),
                            new CookieOptions
                            {
                                HttpOnly = false,
                                Secure = false, // Set to false for development
                                SameSite = SameSiteMode.Lax, // More permissive for development
                                Expires = DateTimeOffset.UtcNow.AddHours(1)
                            });
                    }
                }

                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Vui lòng kiểm tra lại email và mật khẩu.");
                return Page();
            }
        }

        // Phương thức mới để xử lý đăng xuất
        public IActionResult OnPostLogout()
        {
                    // Xóa cookie JWToken
        HttpContext.Response.Cookies.Delete("JWToken");

        // Xóa cookie UserId
        HttpContext.Response.Cookies.Delete("UserId");

        // Xóa cookie UserRoles (nếu có)
        HttpContext.Response.Cookies.Delete("UserRoles");

            // Chuyển hướng người dùng về trang đăng nhập
            return RedirectToPage("/Auth/Login");
        }
    }
}