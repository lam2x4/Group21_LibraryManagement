namespace WebClient.Pages.Dto.Rating
{
    public class RatingDto
    {
        public int RatingId { get; set; }
        public int BookId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public int Score { get; set; }
        public DateTime RatingDate { get; set; }
    }
}
