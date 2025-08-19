using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace WebClient.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public InputModel Input { get; set; }

        public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải dài từ 6 đến 100 ký tự.")]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
            public string? ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["ApiSettings:AuthApiUrl"] + "/register";

            var registerData = new
            {
                email = Input.Email,
                password = Input.Password
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(registerData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("./Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Đăng ký thất bại. Vui lòng thử lại.");
                return Page();
            }
        }
    }
}