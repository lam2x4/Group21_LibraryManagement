using System;
using System.ComponentModel.DataAnnotations;
namespace WebApi.Models
{

    public class BookingRoom
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public BookingStatus Status { get; set; }

        public int RoomId { get; set; }

        public string UserId { get; set; }

        public Room Room { get; set; }
        public ApplicationUser User { get; set; } // Liên kết với ApplicationUser
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
