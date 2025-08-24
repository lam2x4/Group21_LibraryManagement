using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using WebClient.Pages.Dto.Book;
using WebClient.Pages.Dto.Comment;
using WebClient.Pages.Dto.Rating;
using WebClient.Pages.Dto.Loan;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebClient.Pages.Book
{
    public class DetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetailModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public BookDto? Book { get; set; }
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public List<RatingDto> Ratings { get; set; } = new List<RatingDto>();
        public List<BookItemDto> BookItems { get; set; } = new List<BookItemDto>();
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public bool IsLoggedIn { get; set; }
        public string? CurrentUserId { get; set; }
        public bool HasUserLoanedBook { get; set; }

        [BindProperty]
        public AddCommentDto NewComment { get; set; } = new AddCommentDto();

        [BindProperty]
        public AddRatingDto NewRating { get; set; } = new AddRatingDto();

        [BindProperty]
        public UpdateCommentDto UpdateComment { get; set; } = new UpdateCommentDto();

        [BindProperty]
        public UpdateRatingDto UpdateRating { get; set; } = new UpdateRatingDto();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Check if user is logged in and get UserId
            CheckUserAuthentication();

            // Load book details
            await LoadBookAsync(id);
            
            if (Book == null)
            {
                return NotFound();
            }

            // Load comments and ratings
            await LoadCommentsAsync(id);
            await LoadRatingsAsync(id);
            await LoadBookItemsAsync(id);

            // Check if user has loaned this book
            if (IsLoggedIn && !string.IsNullOrEmpty(CurrentUserId))
            {
                await CheckUserLoanHistoryAsync(id);
            }

            // Set BookId for forms
            NewComment.BookId = id;
            NewRating.BookId = id;

            return Page();
        }

        public async Task<IActionResult> OnPostCommentAsync()
        {
            // Check authentication first
            CheckUserAuthentication();
            
            if (!IsLoggedIn)
            {
                ErrorMessage = "Bạn cần đăng nhập để bình luận.";
                return await OnGetAsync(NewComment.BookId);
            }

            // Check if user has loaned this book
            await CheckUserLoanHistoryAsync(NewComment.BookId);
            if (!HasUserLoanedBook)
            {
                ErrorMessage = "Bạn cần mượn sách này trước khi có thể bình luận.";
                return await OnGetAsync(NewComment.BookId);
            }

            NewComment.UserId = CurrentUserId;

            // Debug: Log validation errors
            if (ModelState.GetFieldValidationState(nameof(NewComment)) == ModelValidationState.Invalid)
            {
                var errors = ModelState.Where(ms => ms.Key.StartsWith(nameof(NewComment)))
                                       .SelectMany(ms => ms.Value.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                if (errors.Any())
                {
                    Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
                    ErrorMessage = $"Vui lòng kiểm tra lại thông tin bình luận: {string.Join(", ", errors)}";
                    return await OnGetAsync(NewComment.BookId);
                }
            }


            var success = await CreateCommentAsync();
            if (success)
            {
                SuccessMessage = "Bình luận của bạn đã được thêm thành công!";
                NewComment.Content = string.Empty; // Clear form
            }

            return await OnGetAsync(NewComment.BookId);
        }

        public async Task<IActionResult> OnPostRatingAsync()
        {
            // Check authentication first
            CheckUserAuthentication();
            
            if (!IsLoggedIn)
            {
                ErrorMessage = "Bạn cần đăng nhập để đánh giá.";
                return await OnGetAsync(NewRating.BookId);
            }

            // Check if user has loaned this book
            await CheckUserLoanHistoryAsync(NewRating.BookId);
            if (!HasUserLoanedBook)
            {
                ErrorMessage = "Bạn cần mượn sách này trước khi có thể đánh giá.";
                return await OnGetAsync(NewRating.BookId);
            }

            NewRating.UserId = CurrentUserId;

            // Debug: Log validation errors
            if (!TryValidateModel(NewRating, nameof(NewRating)))
            {
                var errors = ModelState.Where(ms => ms.Key.StartsWith(nameof(NewRating)))
                                       .SelectMany(ms => ms.Value.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                if (errors.Any())
                {
                    ErrorMessage = $"Vui lòng chọn điểm đánh giá hợp lệ: {string.Join(", ", errors)}";
                    return await OnGetAsync(NewRating.BookId);
                }
            }

            var success = await CreateRatingAsync();
            if (success)
            {
                SuccessMessage = "Đánh giá của bạn đã được thêm thành công!";
                NewRating.Score = 0; // Reset form
            }

            return await OnGetAsync(NewRating.BookId);
        }

        public async Task<IActionResult> OnPostUpdateCommentAsync()
        {
            // Check authentication first
            CheckUserAuthentication();
            
            if (!IsLoggedIn)
            {
                ErrorMessage = "Bạn cần đăng nhập để chỉnh sửa bình luận.";
                return await OnGetAsync(UpdateComment.BookId);
            }

            // Debug: Log validation errors
            if (!TryValidateModel(UpdateComment, nameof(UpdateComment)))
            {
                var errors = ModelState.Where(ms => ms.Key.StartsWith(nameof(UpdateComment)))
                                       .SelectMany(ms => ms.Value.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                if (errors.Any())
                {
                    ErrorMessage = $"Vui lòng kiểm tra lại thông tin bình luận: {string.Join(", ", errors)}";
                    return await OnGetAsync(UpdateComment.BookId);
                }
            }

            var success = await UpdateCommentAsync();
            if (success)
            {
                SuccessMessage = "Cập nhật bình luận thành công!";
            }

            return await OnGetAsync(UpdateComment.BookId);
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId, int bookId)
        {
            // Check authentication first
            CheckUserAuthentication();
            
            if (!IsLoggedIn)
            {
                ErrorMessage = "Bạn cần đăng nhập để xóa bình luận.";
                return await OnGetAsync(bookId);
            }

            var success = await DeleteCommentAsync(commentId);
            if (success)
            {
                SuccessMessage = "Xóa bình luận thành công!";
            }

            return await OnGetAsync(bookId);
        }

        public async Task<IActionResult> OnPostUpdateRatingAsync()
        {
            // Check authentication first
            CheckUserAuthentication();
            
            if (!IsLoggedIn)
            {
                ErrorMessage = "Bạn cần đăng nhập để chỉnh sửa đánh giá.";
                return await OnGetAsync(UpdateRating.BookId);
            }

            // Debug: Log validation errors
            if (!TryValidateModel(UpdateRating, nameof(UpdateRating)))
            {
                var errors = ModelState.Where(ms => ms.Key.StartsWith(nameof(UpdateRating)))
                                       .SelectMany(ms => ms.Value.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                if (errors.Any())
                {
                    ErrorMessage = $"Vui lòng chọn điểm đánh giá hợp lệ: {string.Join(", ", errors)}";
                    return await OnGetAsync(UpdateRating.BookId);
                }
            }

            var success = await UpdateRatingAsync();
            if (success)
            {
                SuccessMessage = "Cập nhật đánh giá thành công!";
            }

            return await OnGetAsync(UpdateRating.BookId);
        }

        public async Task<IActionResult> OnPostDeleteRatingAsync(int ratingId, int bookId)
        {
            // Check authentication first
            CheckUserAuthentication();
            
            if (!IsLoggedIn)
            {
                ErrorMessage = "Bạn cần đăng nhập để xóa đánh giá.";
                return await OnGetAsync(bookId);
            }

            var success = await DeleteRatingAsync(ratingId);
            if (success)
            {
                SuccessMessage = "Xóa đánh giá thành công!";
            }

            return await OnGetAsync(bookId);
        }

        private void CheckUserAuthentication()
        {
            // Debug: Log all cookies
            var allCookies = Request.Cookies.Keys.ToList();
            Console.WriteLine($"Available cookies: {string.Join(", ", allCookies)}");
            
            // First try to get UserId from cookie (easier and more reliable)
            CurrentUserId = Request.Cookies["UserId"];
            Console.WriteLine($"UserId from cookie: {CurrentUserId}");
            
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                IsLoggedIn = true;
                Console.WriteLine("User is logged in via UserId cookie");
                return;
            }
            
            // Fallback: try to get from JWT token
            var token = Request.Cookies["JWToken"];
            Console.WriteLine($"JWToken exists: {!string.IsNullOrEmpty(token)}");
            
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    
                    // Debug: Log all claims
                    var allClaims = jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                    Console.WriteLine($"JWT Claims: {string.Join(", ", allClaims)}");
                    
                    CurrentUserId = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value 
                                 ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value
                                 ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
                    
                    Console.WriteLine($"Extracted UserId from JWT: {CurrentUserId}");
                    IsLoggedIn = !string.IsNullOrEmpty(CurrentUserId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing JWT: {ex.Message}");
                    IsLoggedIn = false;
                    CurrentUserId = null;
                }
            }
            else
            {
                Console.WriteLine("No JWToken found");
                IsLoggedIn = false;
                CurrentUserId = null;
            }
            
            Console.WriteLine($"Final result - IsLoggedIn: {IsLoggedIn}, CurrentUserId: {CurrentUserId}");
        }

        private async Task LoadBookAsync(int bookId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("Api");
                var response = await httpClient.GetAsync($"odata/Books?$expand=Publisher&$filter=BookId eq {bookId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    var odataResponse = JsonSerializer.Deserialize<BooksODataResponse>(jsonString, options);
                    Book = odataResponse?.Value?.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể tải thông tin sách.";
            }
        }

        private async Task LoadCommentsAsync(int bookId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("Api");
                var response = await httpClient.GetAsync($"api/Comment/GetByBook/{bookId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    Comments = JsonSerializer.Deserialize<List<CommentDto>>(jsonString, options) ?? new List<CommentDto>();
                }
            }
            catch (Exception ex)
            {
                // Comments loading failed, but don't show error to user
            }
        }

        private async Task LoadRatingsAsync(int bookId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("Api");
                var response = await httpClient.GetAsync($"api/Rating/GetByRating/{bookId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    Ratings = JsonSerializer.Deserialize<List<RatingDto>>(jsonString, options) ?? new List<RatingDto>();
                    
                    // Calculate average rating
                    if (Ratings.Any())
                    {
                        AverageRating = Ratings.Average(r => r.Score);
                        TotalRatings = Ratings.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                // Ratings loading failed, but don't show error to user
            }
        }

        private async Task LoadBookItemsAsync(int bookId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("Api");
                var response = await httpClient.GetAsync($"api/BookItems/book/{bookId}");
                
                Console.WriteLine($"LoadBookItemsAsync - Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"LoadBookItemsAsync - JSON response: {jsonString}");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    // Try to deserialize as direct array first
                    try
                    {
                        BookItems = JsonSerializer.Deserialize<List<BookItemDto>>(jsonString, options) ?? new List<BookItemDto>();
                    }
                    catch
                    {
                        // If that fails, try to deserialize as object with $values property
                        try
                        {
                            var wrapper = JsonSerializer.Deserialize<JsonElement>(jsonString);
                            if (wrapper.TryGetProperty("$values", out var valuesElement))
                            {
                                BookItems = JsonSerializer.Deserialize<List<BookItemDto>>(valuesElement.GetRawText(), options) ?? new List<BookItemDto>();
                            }
                            else
                            {
                                BookItems = new List<BookItemDto>();
                            }
                        }
                        catch
                        {
                            BookItems = new List<BookItemDto>();
                        }
                    }
                    
                    Console.WriteLine($"LoadBookItemsAsync - Loaded {BookItems.Count} book items for book {bookId}");
                    foreach (var item in BookItems)
                    {
                        Console.WriteLine($"  Item {item.ItemId}: {item.Barcode} - {item.Status}");
                    }
                }
                else
                {
                    Console.WriteLine($"LoadBookItemsAsync - Failed to load book items. Status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"LoadBookItemsAsync - Error content: {errorContent}");
                    BookItems = new List<BookItemDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadBookItemsAsync - Exception: {ex.Message}");
                BookItems = new List<BookItemDto>();
            }
        }

        private async Task<bool> CreateCommentAsync()
        {
            try
            {
                Console.WriteLine($"Creating comment - BookId: {NewComment.BookId}, UserId: {NewComment.UserId}, Content: {NewComment.Content}");
                
                var httpClient = _httpClientFactory.CreateClient("Api");
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(NewComment, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Add authorization header if needed
                var token = Request.Cookies["JWToken"];
                Console.WriteLine($"JWToken exists: {!string.IsNullOrEmpty(token)}");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("Authorization header added");
                }

                var response = await httpClient.PostAsync("api/Comment/Create", jsonContent);
                Console.WriteLine($"Comment API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Comment API error: {errorContent}");
                    if (errorContent.Contains("3 lần"))
                    {
                        ErrorMessage = "Bạn chỉ được phép bình luận tối đa 3 lần cho mỗi cuốn sách.";
                    }
                    else
                    {
                        ErrorMessage = "Có lỗi xảy ra khi thêm bình luận.";
                    }
                    return false;
                }
                
                Console.WriteLine("Comment created successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateCommentAsync: {ex.Message}");
                ErrorMessage = "Có lỗi xảy ra khi thêm bình luận.";
                return false;
            }
        }

        private async Task<bool> CreateRatingAsync()
        {
            try
            {
                Console.WriteLine($"Creating rating - BookId: {NewRating.BookId}, UserId: {NewRating.UserId}, Score: {NewRating.Score}");
                
                var httpClient = _httpClientFactory.CreateClient("Api");
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(NewRating, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Add authorization header if needed
                var token = Request.Cookies["JWToken"];
                Console.WriteLine($"JWToken exists: {!string.IsNullOrEmpty(token)}");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("Authorization header added");
                }

                var response = await httpClient.PostAsync("api/Rating/Create", jsonContent);
                Console.WriteLine($"Rating API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Rating API error: {errorContent}");
                    if (errorContent.Contains("3 lần"))
                    {
                        ErrorMessage = "Bạn chỉ được phép đánh giá tối đa 3 lần cho mỗi cuốn sách.";
                    }
                    else
                    {
                        ErrorMessage = "Có lỗi xảy ra khi thêm đánh giá.";
                    }
                    return false;
                }
                
                Console.WriteLine("Rating created successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateRatingAsync: {ex.Message}");
                ErrorMessage = "Có lỗi xảy ra khi thêm đánh giá.";
                return false;
            }
        }

        private async Task<bool> UpdateCommentAsync()
        {
            try
            {
                Console.WriteLine($"Updating comment - CommentId: {UpdateComment.CommentId}, Content: {UpdateComment.Content}");
                
                var httpClient = _httpClientFactory.CreateClient("Api");
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(UpdateComment, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Add authorization header if needed
                var token = Request.Cookies["JWToken"];
                Console.WriteLine($"JWToken exists: {!string.IsNullOrEmpty(token)}");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("Authorization header added");
                }

                var response = await httpClient.PutAsync("api/Comment/Update", jsonContent);
                Console.WriteLine($"Update Comment API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Update Comment API error: {errorContent}");
                    if (errorContent.Contains("15 phút"))
                    {
                        ErrorMessage = "Bình luận chỉ có thể chỉnh sửa trong vòng 15 phút sau khi tạo.";
                    }
                    else
                    {
                        ErrorMessage = "Có lỗi xảy ra khi cập nhật bình luận.";
                    }
                    return false;
                }
                
                Console.WriteLine("Comment updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateCommentAsync: {ex.Message}");
                ErrorMessage = "Có lỗi xảy ra khi cập nhật bình luận.";
                return false;
            }
        }

        private async Task<bool> DeleteCommentAsync(int commentId)
        {
            try
            {
                Console.WriteLine($"Deleting comment - CommentId: {commentId}");
                
                var httpClient = _httpClientFactory.CreateClient("Api");

                // Add authorization header if needed
                var token = Request.Cookies["JWToken"];
                Console.WriteLine($"JWToken exists: {!string.IsNullOrEmpty(token)}");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("Authorization header added");
                }

                var response = await httpClient.DeleteAsync($"api/Comment/Delete/{commentId}");
                Console.WriteLine($"Delete Comment API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Delete Comment API error: {errorContent}");
                    if (errorContent.Contains("15 phút"))
                    {
                        ErrorMessage = "Bình luận chỉ có thể xóa trong vòng 15 phút sau khi tạo.";
                    }
                    else
                    {
                        ErrorMessage = "Có lỗi xảy ra khi xóa bình luận.";
                    }
                    return false;
                }
                
                Console.WriteLine("Comment deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteCommentAsync: {ex.Message}");
                ErrorMessage = "Có lỗi xảy ra khi xóa bình luận.";
                return false;
            }
        }

        private async Task<bool> UpdateRatingAsync()
        {
            try
            {
                Console.WriteLine($"Updating rating - RatingId: {UpdateRating.RatingId}, Score: {UpdateRating.Score}");
                
                var httpClient = _httpClientFactory.CreateClient("Api");
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(UpdateRating, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Add authorization header if needed
                var token = Request.Cookies["JWToken"];
                Console.WriteLine($"JWToken exists: {!string.IsNullOrEmpty(token)}");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("Authorization header added");
                }

                var response = await httpClient.PutAsync("api/Rating/Update", jsonContent);
                Console.WriteLine($"Update Rating API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Update Rating API error: {errorContent}");
                    if (errorContent.Contains("15 phút"))
                    {
                        ErrorMessage = "Đánh giá chỉ có thể chỉnh sửa trong vòng 15 phút sau khi tạo.";
                    }
                    else
                    {
                        ErrorMessage = "Có lỗi xảy ra khi cập nhật đánh giá.";
                    }
                    return false;
                }
                
                Console.WriteLine("Rating updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateRatingAsync: {ex.Message}");
                ErrorMessage = "Có lỗi xảy ra khi cập nhật đánh giá.";
                return false;
            }
        }

        private async Task<bool> DeleteRatingAsync(int ratingId)
        {
            try
            {
                Console.WriteLine($"Deleting rating - RatingId: {ratingId}");
                
                var httpClient = _httpClientFactory.CreateClient("Api");

                // Add authorization header if needed
                var token = Request.Cookies["JWToken"];
                Console.WriteLine($"JWToken exists: {!string.IsNullOrEmpty(token)}");
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("Authorization header added");
                }

                var response = await httpClient.DeleteAsync($"api/Rating/Delete/{ratingId}");
                Console.WriteLine($"Delete Rating API response status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Delete Rating API error: {errorContent}");
                    if (errorContent.Contains("15 phút"))
                    {
                        ErrorMessage = "Đánh giá chỉ có thể xóa trong vòng 15 phút sau khi tạo.";
                    }
                    else
                    {
                        ErrorMessage = "Có lỗi xảy ra khi xóa đánh giá.";
                    }
                    return false;
                }
                
                Console.WriteLine("Rating deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteRatingAsync: {ex.Message}");
                ErrorMessage = "Có lỗi xảy ra khi xóa đánh giá.";
                return false;
            }
        }

        private async Task CheckUserLoanHistoryAsync(int bookId)
        {
            try
            {
                Console.WriteLine($"CheckUserLoanHistoryAsync - BookId: {bookId}, CurrentUserId: {CurrentUserId}");
                
                var httpClient = _httpClientFactory.CreateClient("Api");
                
                // Add authorization header if needed
                var token = Request.Cookies["JWToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // Call API to check if user has loaned this book
                var apiUrl = $"api/Loan/user/{CurrentUserId}";
                Console.WriteLine($"Calling API: {apiUrl}");
                var response = await httpClient.GetAsync(apiUrl);
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response JSON: {jsonString}");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    var loans = JsonSerializer.Deserialize<List<LoanDto>>(jsonString, options);
                    Console.WriteLine($"Deserialized {loans?.Count ?? 0} loans");
                    
                    if (loans != null)
                    {
                        foreach (var loan in loans)
                        {
                            Console.WriteLine($"Loan {loan.LoanId}: BookId={loan.BookId}, BookTitle={loan.BookTitle}");
                        }
                    }
                    
                    // Check if user has any loan for this book
                    HasUserLoanedBook = loans?.Any(loan => loan.BookId == bookId) ?? false;
                    Console.WriteLine($"HasUserLoanedBook result: {HasUserLoanedBook}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {errorContent}");
                    HasUserLoanedBook = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CheckUserLoanHistoryAsync: {ex.Message}");
                HasUserLoanedBook = false;
            }
        }
    }
}
