using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebClient.Pages.Dto.Role;

namespace WebClient.Pages.Admin
{
    public class RolesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RolesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
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

                await LoadRoles(client);
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải dữ liệu: {ex.Message}";
                IsError = true;
            }

            return Page();
        }

        private async Task LoadRoles(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("api/Roles");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Roles = JsonSerializer.Deserialize<List<RoleDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<RoleDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Message = $"Lỗi khi tải danh sách vai trò: {errorContent}";
                    IsError = true;
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi khi tải danh sách vai trò: {ex.Message}";
                IsError = true;
            }
        }
    }
}
