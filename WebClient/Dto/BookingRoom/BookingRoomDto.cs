using WebClient.Dto.Room;

namespace WebClient.Dto.BookingRoom
{
    public class BookingRoomDto
    {
        public int BookingId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public BookingStatus Status { get; set; }
        public int RoomId { get; set; }
        public string UserId { get; set; }
        public RoomDto Room { get; set; }
        public string UserName { get; set; }
    }

    public class CreateBookingRoomDto
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int RoomId { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateBookingRoomDto
    {
        public BookingStatus Status { get; set; }
    }

    public enum BookingStatus
    {
        Pending,     // Đang chờ xác nhận
        Confirmed,   // Đã xác nhận
        Cancelled,   // Đã hủy
        CheckedIn,   // Đã nhận phòng
        CheckedOut   // Đã trả phòng
    }
}
