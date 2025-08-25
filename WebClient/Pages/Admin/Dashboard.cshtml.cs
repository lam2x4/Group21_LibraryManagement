using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Net.Http.Headers;

namespace WebClient.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DashboardModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Dashboard Statistics
        public int TotalBooks { get; set; }
        public int TotalLoans { get; set; }
        public int TotalUsers { get; set; }
        public int OverdueBooks { get; set; }
        public List<RecentLoanDto> RecentLoans { get; set; } = new List<RecentLoanDto>();

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

                // Load dashboard statistics
                await LoadDashboardData(client);
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
                // Set default values
                TotalBooks = 0;
                TotalLoans = 0;
                TotalUsers = 0;
                OverdueBooks = 0;
            }

            return Page();
        }

        private async Task LoadDashboardData(HttpClient client)
        {
            try
            {
                // Lấy thống kê sách
                var booksResponse = await client.GetAsync("api/Books");
                if (booksResponse.IsSuccessStatusCode)
                {
                    var booksJson = await booksResponse.Content.ReadAsStringAsync();
                    var books = JsonSerializer.Deserialize<List<BookDto>>(booksJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TotalBooks = books?.Count ?? 0;
                }

                // Lấy thống kê users
                var usersResponse = await client.GetAsync("api/Users");
                if (usersResponse.IsSuccessStatusCode)
                {
                    var usersJson = await usersResponse.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<object>>(usersJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    TotalUsers = users?.Count ?? 0;
                }

                // Mock data for other statistics (sẽ được thay thế bằng API thực tế)
                TotalLoans = 145;
                OverdueBooks = 7;

                // Mock data for recent loans
                RecentLoans = new List<RecentLoanDto>
                {
                    new RecentLoanDto
                    {
                        UserName = "Nguyễn Văn A",
                        BookTitle = "Tôi thấy hoa vàng trên cỏ xanh",
                        LoanDate = DateTime.Now.AddDays(-5),
                        DueDate = DateTime.Now.AddDays(9),
                        IsReturned = false
                    },
                    new RecentLoanDto
                    {
                        UserName = "Trần Thị B",
                        BookTitle = "Đắc nhân tâm",
                        LoanDate = DateTime.Now.AddDays(-3),
                        DueDate = DateTime.Now.AddDays(11),
                        IsReturned = false
                    },
                    new RecentLoanDto
                    {
                        UserName = "Lê Văn C",
                        BookTitle = "Tôi tài giỏi, bạn cũng thế",
                        LoanDate = DateTime.Now.AddDays(-10),
                        DueDate = DateTime.Now.AddDays(-3),
                        IsReturned = false
                    },
                    new RecentLoanDto
                    {
                        UserName = "Phạm Thị D",
                        BookTitle = "Tôi thấy hoa vàng trên cỏ xanh",
                        LoanDate = DateTime.Now.AddDays(-15),
                        DueDate = DateTime.Now.AddDays(-1),
                        IsReturned = true
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadDashboardData: {ex.Message}");
            }
        }
    }

    public class BookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ISBN13 { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class RecentLoanDto
    {
        public string UserName { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsReturned { get; set; }
    }
}
