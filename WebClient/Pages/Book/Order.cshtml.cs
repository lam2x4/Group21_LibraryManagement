using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WebClient.Pages.Dto.Book;
using WebClient.Pages.Dto.Loan;

namespace WebClient.Pages.Book
{
    public class OrderModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrderModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public CreateLoanDto CreateLoanRequest { get; set; } = new CreateLoanDto();

        [BindProperty]
        public BookDto? Book { get; set; }

        [BindProperty]
        public int ItemId { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int itemId)
        {
            ItemId = itemId;
            
            try
            {
                await LoadBookInfoAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Lấy thông tin user từ cookie hoặc sử dụng user mặc định từ seed data
                var currentUserId = Request.Cookies["UserId"] ?? "user-1"; // Sử dụng user-1 từ seed data
                var librarianId = "librarian-1"; // Sử dụng librarian-1 từ seed data

                Console.WriteLine($"Creating loan - ItemId: {ItemId}, UserId: {currentUserId}, LibrarianId: {librarianId}");
                Console.WriteLine($"LoanDate: {CreateLoanRequest.LoanDate}, DueDate: {CreateLoanRequest.DueDate}");

                var createLoanDto = new CreateLoanDto
                {
                    ItemId = ItemId, // Sử dụng ItemId từ route parameter
                    UserId = currentUserId,
                    LibrarianId = librarianId,
                    LoanDate = CreateLoanRequest.LoanDate,
                    DueDate = CreateLoanRequest.DueDate
                };

                var client = _httpClientFactory.CreateClient("Api");
                var json = JsonSerializer.Serialize(createLoanDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                Console.WriteLine($"Request JSON: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/Loan", content);
                Console.WriteLine($"Loan API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Loan API success response: {responseContent}");
                    SuccessMessage = "Đặt mượn sách thành công! Bạn có thể xem lịch sử mượn sách trong trang cá nhân.";
                    
                    // Clear form
                    CreateLoanRequest = new CreateLoanDto();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Loan API error: {errorContent}");
                    ErrorMessage = $"Lỗi: {errorContent}";
                }
                
                // Luôn load lại thông tin book sau khi xử lý loan (thành công hoặc thất bại)
                await LoadBookInfoAsync();
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in OnPostAsync: {ex.Message}");
                ErrorMessage = $"Lỗi: {ex.Message}";
                
                // Nếu có exception, cần load lại thông tin book
                await LoadBookInfoAsync();
                return Page();
            }
        }

        // Tách logic load book info thành method riêng
        private async Task LoadBookInfoAsync()
        {
            Console.WriteLine($"LoadBookInfoAsync called for ItemId: {ItemId}");
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync($"/api/BookItems/{ItemId}");

                Console.WriteLine($"LoadBookInfoAsync - API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"LoadBookInfoAsync - API response content: {content}");

                    var bookItem = JsonSerializer.Deserialize<BookItemDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (bookItem != null)
                    {
                        // Tạo BookDto từ thông tin trong BookItemDto
                        Book = new BookDto
                        {
                            BookId = bookItem.BookId,
                            Title = bookItem.BookTitle,
                            ISBN13 = bookItem.BookISBN13,
                            PublicationYear = bookItem.BookPublicationYear,
                            ImageUrl = bookItem.BookImageUrl,
                            Description = bookItem.BookDescription,
                            Publisher = new PublisherDto
                            {
                                Name = bookItem.PublisherName
                            }
                        };
                        Console.WriteLine($"LoadBookInfoAsync - Successfully loaded book: {Book.Title}");
                    }
                    else
                    {
                        Console.WriteLine("LoadBookInfoAsync - Deserialized bookItem is null.");
                        ErrorMessage = "Không thể tải thông tin sách: Dữ liệu sách trống.";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"LoadBookInfoAsync - API error: {errorContent}");
                    ErrorMessage = $"Không thể tải thông tin sách: {response.StatusCode} - {errorContent}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadBookInfoAsync - Exception: {ex.Message}");
                ErrorMessage = $"Lỗi khi tải thông tin sách: {ex.Message}";
            }
        }
    }
}
