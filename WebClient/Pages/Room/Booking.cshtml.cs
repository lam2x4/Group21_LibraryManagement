using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using WebClient.Dto.Room;
using WebClient.Dto.BookingRoom;

namespace WebClient.Pages.Room
{
    public class BookingModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BookingModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public CreateBookingRoomDto BookingRequest { get; set; }

        public RoomDto? SelectedRoom { get; set; }
        public bool IsLoggedIn { get; set; }
        public string? CurrentUserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? roomId)
        {
            Console.WriteLine($"OnGetAsync called with roomId: {roomId}");
            
            CheckUserAuthentication();
            Console.WriteLine($"IsLoggedIn: {IsLoggedIn}");

            if (!IsLoggedIn)
            {
                Console.WriteLine("User not logged in, redirecting to login");
                return RedirectToPage("/Auth/Login");
            }

            if (!roomId.HasValue)
            {
                Console.WriteLine("No roomId provided, redirecting to room list");
                return RedirectToPage("/Room/List");
            }

            Console.WriteLine($"Loading room details for roomId: {roomId.Value}");
            await LoadRoomDetailsAsync(roomId.Value);
            
            Console.WriteLine($"SelectedRoom is null: {SelectedRoom == null}");
            if (SelectedRoom != null)
            {
                Console.WriteLine($"SelectedRoom loaded: {SelectedRoom.RoomName}, ID: {SelectedRoom.RoomId}");
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? roomId)
        {
            Console.WriteLine($"OnPostAsync called with roomId: {roomId}");
            
            CheckUserAuthentication();

            if (!IsLoggedIn)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!roomId.HasValue)
            {
                Console.WriteLine("No roomId provided in POST, redirecting to room list");
                return RedirectToPage("/Room/List");
            }

            // Validate dates
            if (BookingRequest.CheckInDate >= BookingRequest.CheckOutDate)
            {
                ErrorMessage = "Ngày check-out phải sau ngày check-in";
                await LoadRoomDetailsAsync(roomId.Value);
                return Page();
            }

            if (BookingRequest.CheckInDate.Date <= DateTime.Today)
            {
                ErrorMessage = "Ngày check-in phải từ ngày mai trở đi";
                await LoadRoomDetailsAsync(roomId.Value);
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var apiUrl = "api/BookingRoom";

                var bookingData = new
                {
                    checkInDate = BookingRequest.CheckInDate,
                    checkOutDate = BookingRequest.CheckOutDate,
                    roomId = roomId.Value,
                    userId = CurrentUserId
                };

                Console.WriteLine($"Booking data: {JsonSerializer.Serialize(bookingData)}");

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(bookingData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Redirect to Room/List with success message
                    return RedirectToPage("/Room/List", new { successMessage = "Đặt phòng thành công!" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Remove quotes from error message
                    errorContent = errorContent.Trim('"');
                    ErrorMessage = errorContent;
                    await LoadRoomDetailsAsync(roomId.Value);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
                await LoadRoomDetailsAsync(roomId.Value);
                return Page();
            }
        }

        // Proxy endpoint for JavaScript API calls
        [HttpGet("api/BookingRoom/check-availability")]
        public async Task<IActionResult> CheckAvailability(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("Api");
                var apiUrl = $"api/BookingRoom/check-availability?roomId={roomId}&checkInDate={checkInDate:yyyy-MM-dd}&checkOutDate={checkOutDate:yyyy-MM-dd}";
                
                var response = await httpClient.GetAsync(apiUrl);
                var content = await response.Content.ReadAsStringAsync();
                
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private void CheckUserAuthentication()
        {
            var jwtToken = Request.Cookies["JWToken"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                IsLoggedIn = true;
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

        private async Task LoadRoomDetailsAsync(int roomId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("Api");
                var apiUrl = $"api/Room/{roomId}";
                Console.WriteLine($"Loading room details from: {httpClient.BaseAddress}{apiUrl}");
                
                var response = await httpClient.GetAsync(apiUrl);
                Console.WriteLine($"API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API response content: {content}");
                    
                    var room = JsonSerializer.Deserialize<RoomDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (room != null)
                    {
                        Console.WriteLine($"Room loaded successfully: {room.RoomName}, ID: {room.RoomId}");
                        SelectedRoom = room;
                        BookingRequest = new CreateBookingRoomDto
                        {
                            RoomId = room.RoomId,
                            CheckInDate = DateTime.Today.AddDays(1),
                            CheckOutDate = DateTime.Today.AddDays(2)
                        };
                    }
                    else
                    {
                        Console.WriteLine("Room deserialization returned null");
                        ErrorMessage = "Không thể parse thông tin phòng";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API error response: {errorContent}");
                    ErrorMessage = $"Không thể tải thông tin phòng (HTTP {response.StatusCode})";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception loading room details: {ex.Message}");
                ErrorMessage = $"Lỗi tải thông tin phòng: {ex.Message}";
            }
        }
    }
}
