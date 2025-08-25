using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WebClient.Pages.Dto.Book;
using System.IdentityModel.Tokens.Jwt;

namespace WebClient.Pages.Book
{
    public class ListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<BookDto> Books { get; set; } = new List<BookDto>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTitle { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? SearchPublisher { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? SearchYear { get; set; }

        public bool IsLoggedIn { get; set; }
        public string? CurrentUserId { get; set; }

        public async Task OnGetAsync()
        {
            // Check if user is logged in
            CheckUserAuthentication();
            
            await LoadBooksAsync();
        }

        private async Task LoadBooksAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("Api");
            
            // Build OData query with filters
            var queryParams = new List<string> { "$expand=Publisher" };
            
            var filters = new List<string>();
            
            if (!string.IsNullOrEmpty(SearchTitle))
            {
                filters.Add($"contains(tolower(Title), '{SearchTitle.ToLower()}')");
            }
            
            if (!string.IsNullOrEmpty(SearchPublisher))
            {
                filters.Add($"contains(tolower(Publisher/Name), '{SearchPublisher.ToLower()}')");
            }
            
            if (SearchYear.HasValue)
            {
                filters.Add($"PublicationYear eq {SearchYear.Value}");
            }
            
            if (filters.Any())
            {
                queryParams.Add($"$filter={string.Join(" and ", filters)}");
            }
            
            var queryString = string.Join("&", queryParams);
            var apiUrl = $"odata/Books?{queryString}";
            
            try
            {
                var response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                
                var jsonString = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var odataResponse = JsonSerializer.Deserialize<BooksODataResponse>(jsonString, options);
                Books = odataResponse?.Value ?? new List<BookDto>();
            }
            catch (Exception ex)
            {
                // Log error or handle appropriately
                Books = new List<BookDto>();
            }
        }

        private void CheckUserAuthentication()
        {
            // First try to get UserId from cookie (easier and more reliable)
            CurrentUserId = Request.Cookies["UserId"];
            
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                IsLoggedIn = true;
                return;
            }
            
            // Fallback: try to get from JWT token
            var token = Request.Cookies["JWToken"];
            
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    
                    CurrentUserId = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value 
                                 ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value
                                 ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
                    
                    IsLoggedIn = !string.IsNullOrEmpty(CurrentUserId);
                }
                catch (Exception ex)
                {
                    IsLoggedIn = false;
                    CurrentUserId = null;
                }
            }
            else
            {
                IsLoggedIn = false;
                CurrentUserId = null;
            }
        }
    }
}
