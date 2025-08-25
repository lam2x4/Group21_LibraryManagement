using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WebClient.Dto.Room;

namespace WebClient.Pages.Room
{
    public class ListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
        public bool IsLoggedIn { get; set; }
        public string? CurrentUserId { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync(string? successMessage)
        {
            CheckUserAuthentication();
            await LoadRoomsAsync();
            
            // Set success message if provided
            if (!string.IsNullOrEmpty(successMessage))
            {
                SuccessMessage = successMessage;
            }
        }

        private void CheckUserAuthentication()
        {
            // Kiểm tra JWT token
            var jwtToken = Request.Cookies["JWToken"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                IsLoggedIn = true;
                
                // Lấy UserId từ cookie
                var userId = Request.Cookies["UserId"];
                if (!string.IsNullOrEmpty(userId))
                {
                    CurrentUserId = userId;
                }
            }
            else
            {
                IsLoggedIn = false;
            }
        }

        private async Task LoadRoomsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/Room");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<RoomDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (rooms != null)
                    {
                        Rooms = rooms;
                    }
                }
                else
                {
                    Console.WriteLine($"Error loading rooms: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception loading rooms: {ex.Message}");
            }
        }
    }
}
