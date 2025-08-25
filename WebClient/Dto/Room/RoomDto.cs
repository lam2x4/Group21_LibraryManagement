namespace WebClient.Dto.Room
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomDescription { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateRoomDto
    {
        public string RoomName { get; set; }
        public string RoomDescription { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class UpdateRoomDto
    {
        public string RoomName { get; set; }
        public string RoomDescription { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
    }
}
