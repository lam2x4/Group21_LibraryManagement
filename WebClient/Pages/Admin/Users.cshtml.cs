using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebClient.Pages.Dto.ApplicationUser;

namespace WebClient.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
        public bool IsError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra xem user có phải admin không
            var userRoles = Request.Cookies["UserRoles"];
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("Admin"))
            {
                return RedirectToPage("/Auth/Login");
            }

            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Load users
                await LoadUsers(client);
                await LoadAvailableRoles(client);
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải dữ liệu: {ex.Message}";
                IsError = true;
            }

            return Page();
        }

        private async Task LoadUsers(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("api/Users");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Users = JsonSerializer.Deserialize<List<UserDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<UserDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tải danh sách người dùng: {errorContent}";
                    IsError = true;
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải danh sách người dùng: {ex.Message}";
                IsError = true;
            }
        }

        private async Task LoadAvailableRoles(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("api/Users/roles");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    AvailableRoles = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<string>();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the page load
                Console.WriteLine($"Error loading roles: {ex.Message}");
            }
        }
    }
}
