using System.ComponentModel.DataAnnotations;
using WebApi.Models;

namespace WebApi.Dto
{
    public class BookingRoomDto
    {
        public int BookingId { get; set; }
        
        [Required]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        public DateTime CheckOutDate { get; set; }
        
        [Required]
        public BookingStatus Status { get; set; }
        
        public int RoomId { get; set; }
        public string UserId { get; set; }
        
        // Navigation properties
        public RoomDto Room { get; set; }
        public string UserName { get; set; }
    }

    public class CreateBookingRoomDto
    {
        [Required]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        public DateTime CheckOutDate { get; set; }
        
        [Required]
        public int RoomId { get; set; }
        
        [Required]
        public string UserId { get; set; }
    }

    public class UpdateBookingRoomDto
    {
        [Required]
        public BookingStatus Status { get; set; }
    }
}
