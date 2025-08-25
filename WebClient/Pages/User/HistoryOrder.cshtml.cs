using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WebClient.Pages.Dto.Loan;
using System.Text;
using System.Globalization;

namespace WebClient.Pages.User
{
    public class HistoryOrderModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HistoryOrderModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<LoanDto> Loans { get; set; } = new List<LoanDto>();
        public List<LoanDto> FilteredLoans { get; set; } = new List<LoanDto>();
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "all";

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Lấy thông tin user từ cookie hoặc sử dụng user mặc định từ seed data
                var currentUserId = Request.Cookies["UserId"] ?? "user-1";

                Console.WriteLine($"Loading loan history for UserId: {currentUserId}");

                var client = _httpClientFactory.CreateClient("Api"); // Sử dụng "Api" thay vì "API"
                var response = await client.GetAsync($"/api/Loan/user/{currentUserId}");

                Console.WriteLine($"Loan history API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Loan history API response: {content}");
                    
                    // Try to deserialize as direct array first
                    try
                    {
                        Loans = JsonSerializer.Deserialize<List<LoanDto>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<LoanDto>();
                    }
                    catch
                    {
                        // If that fails, try to deserialize as object with $values property
                        try
                        {
                            var wrapper = JsonSerializer.Deserialize<JsonElement>(content);
                            if (wrapper.TryGetProperty("$values", out var valuesElement))
                            {
                                Loans = JsonSerializer.Deserialize<List<LoanDto>>(valuesElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                }) ?? new List<LoanDto>();
                            }
                            else
                            {
                                Loans = new List<LoanDto>();
                            }
                        }
                        catch
                        {
                            Loans = new List<LoanDto>();
                        }
                    }
                    
                    Console.WriteLine($"Loaded {Loans.Count} loans for user {currentUserId}");

                    // Apply filters
                    ApplyFilters();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Loan history API error: {errorContent}");
                    ErrorMessage = "Không thể tải lịch sử mượn sách";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in OnGetAsync: {ex.Message}");
                ErrorMessage = $"Lỗi: {ex.Message}";
            }

            return Page();
        }

        private void ApplyFilters()
        {
            var filtered = Loans.AsEnumerable();

            // Filter by date range
            if (StartDate.HasValue)
            {
                filtered = filtered.Where(l => l.LoanDate >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                filtered = filtered.Where(l => l.LoanDate <= EndDate.Value);
            }

            // Filter by status
            switch (StatusFilter?.ToLower())
            {
                case "returned":
                    filtered = filtered.Where(l => l.IsReturned);
                    break;
                case "overdue":
                    filtered = filtered.Where(l => !l.IsReturned && DateTime.Now > l.DueDate);
                    break;
                case "active":
                    filtered = filtered.Where(l => !l.IsReturned && DateTime.Now <= l.DueDate);
                    break;
                default:
                    // "all" - no filtering
                    break;
            }

            FilteredLoans = filtered.ToList();
        }





        public async Task<IActionResult> OnPostReturnBookAsync(int loanId)
        {
            try
            {
                Console.WriteLine($"Returning book for LoanId: {loanId}");

                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsync($"/api/Loan/return/{loanId}", null);

                Console.WriteLine($"Return book API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Return book API success response: {content}");
                    SuccessMessage = "Trả sách thành công!";
                    
                    // Reload loan history
                    await OnGetAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Return book API error: {errorContent}");
                    ErrorMessage = $"Lỗi: {errorContent}";
                    
                    // Reload loan history
                    await OnGetAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in OnPostReturnBookAsync: {ex.Message}");
                ErrorMessage = $"Lỗi: {ex.Message}";
                
                // Reload loan history
                await OnGetAsync();
            }

            return Page();
        }
    }
}
