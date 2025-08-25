using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RoomName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RoomDescription { get; set; }
        
        [Required]
        public decimal PricePerNight { get; set; }
        
        [Required]
        public bool IsAvailable { get; set; }
    }

    public class CreateRoomDto
    {
        [Required]
        [StringLength(50)]
        public string RoomName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RoomDescription { get; set; }
        
        [Required]
        public decimal PricePerNight { get; set; }
        
        [Required]
        public bool IsAvailable { get; set; }
    }

    public class UpdateRoomDto
    {
        [Required]
        [StringLength(50)]
        public string RoomName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RoomDescription { get; set; }
        
        [Required]
        public decimal PricePerNight { get; set; }
        
        [Required]
        public bool IsAvailable { get; set; }
    }
}
