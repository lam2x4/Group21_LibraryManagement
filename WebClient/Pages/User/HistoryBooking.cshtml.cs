using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WebClient.Dto.BookingRoom;
using System.Text;
using System.Globalization;

namespace WebClient.Pages.User
{
    public class HistoryBookingModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HistoryBookingModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<BookingRoomDto> Bookings { get; set; } = new List<BookingRoomDto>();
        public List<BookingRoomDto> FilteredBookings { get; set; } = new List<BookingRoomDto>();
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

                Console.WriteLine($"Loading booking history for UserId: {currentUserId}");

                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync($"/api/BookingRoom/user/{currentUserId}");

                Console.WriteLine($"Booking history API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Booking history API response: {content}");
                    
                    // Try to deserialize as direct array first
                    try
                    {
                        Bookings = JsonSerializer.Deserialize<List<BookingRoomDto>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<BookingRoomDto>();
                    }
                    catch
                    {
                        // If that fails, try to deserialize as object with $values property
                        try
                        {
                            var wrapper = JsonSerializer.Deserialize<JsonElement>(content);
                            if (wrapper.TryGetProperty("$values", out var valuesElement))
                            {
                                Bookings = JsonSerializer.Deserialize<List<BookingRoomDto>>(valuesElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                }) ?? new List<BookingRoomDto>();
                            }
                            else
                            {
                                Bookings = new List<BookingRoomDto>();
                            }
                        }
                        catch
                        {
                            Bookings = new List<BookingRoomDto>();
                        }
                    }
                    
                    Console.WriteLine($"Loaded {Bookings.Count} bookings for user {currentUserId}");

                    // Apply filters
                    ApplyFilters();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Booking history API error: {errorContent}");
                    ErrorMessage = "Không thể tải lịch sử đặt phòng";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in OnGetAsync: {ex.Message}");
                ErrorMessage = $"Lỗi: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelBookingAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                
                // Thay vì xóa, chúng ta sẽ cập nhật trạng thái thành Cancelled
                var updateDto = new { Status = BookingStatus.Cancelled };
                var json = JsonSerializer.Serialize(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.PutAsync($"/api/BookingRoom/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Đã hủy đặt phòng thành công";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Không thể hủy đặt phòng: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetGetBookingDetailAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync($"/api/BookingRoom/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var booking = JsonSerializer.Deserialize<BookingRoomDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (booking != null)
                    {
                        var detailHtml = $@"
                            <div class='booking-detail'>
                                <div class='row'>
                                    <div class='col-md-6'>
                                        <h6><strong>Thông tin đặt phòng</strong></h6>
                                        <p><strong>Mã đặt phòng:</strong> #{booking.BookingId}</p>
                                        <p><strong>Ngày check-in:</strong> {booking.CheckInDate:dd/MM/yyyy HH:mm}</p>
                                        <p><strong>Ngày check-out:</strong> {booking.CheckOutDate:dd/MM/yyyy HH:mm}</p>
                                        <p><strong>Trạng thái:</strong> 
                                                                                         {(booking.Status == BookingStatus.Confirmed ? "<span class='badge badge-success'>Đã xác nhận</span>" : 
                                               booking.Status == BookingStatus.CheckedIn ? "<span class='badge badge-info'>Đã nhận phòng</span>" : 
                                               booking.Status == BookingStatus.CheckedOut ? "<span class='badge badge-secondary'>Đã trả phòng</span>" : 
                                               booking.Status == BookingStatus.Cancelled ? "<span class='badge badge-danger'>Đã hủy</span>" : 
                                               "<span class='badge badge-warning'>Đang chờ</span>")}
                                        </p>
                                    </div>
                                    <div class='col-md-6'>
                                        <h6><strong>Thông tin phòng</strong></h6>
                                        <p><strong>Tên phòng:</strong> {booking.Room.RoomName}</p>
                                        <p><strong>Mô tả:</strong> {booking.Room.RoomDescription}</p>
                                        <p><strong>Giá:</strong> {booking.Room.PricePerNight:N0} VNĐ</p>
                                        <p><strong>Trạng thái:</strong> {(booking.Room.IsAvailable ? "Có sẵn" : "Không có sẵn")}</p>
                                    </div>
                                </div>
                                <div class='row mt-3'>
                                    <div class='col-12'>
                                        <h6><strong>Thông tin người dùng</strong></h6>
                                        <p><strong>Tên người dùng:</strong> {booking.UserName}</p>
                                    </div>
                                </div>
                            </div>";

                        return Content(detailHtml, "text/html");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Lỗi: {ex.Message}</div>", "text/html");
            }

            return Content("<div class='alert alert-warning'>Không tìm thấy thông tin đặt phòng</div>", "text/html");
        }

        private void ApplyFilters()
        {
            FilteredBookings = Bookings.ToList();

            // Apply date filters
            if (StartDate.HasValue)
            {
                FilteredBookings = FilteredBookings.Where(b => b.CheckInDate.Date >= StartDate.Value.Date).ToList();
            }

            if (EndDate.HasValue)
            {
                FilteredBookings = FilteredBookings.Where(b => b.CheckInDate.Date <= EndDate.Value.Date).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "all")
            {
                switch (StatusFilter.ToLower())
                {
                    case "active":
                        FilteredBookings = FilteredBookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
                        break;
                    case "completed":
                        FilteredBookings = FilteredBookings.Where(b => b.Status == BookingStatus.CheckedOut).ToList();
                        break;
                    case "cancelled":
                        FilteredBookings = FilteredBookings.Where(b => b.Status == BookingStatus.Cancelled).ToList();
                        break;
                }
            }

            FilteredBookings = FilteredBookings.OrderByDescending(b => b.CheckInDate).ToList();
        }
    }
}
