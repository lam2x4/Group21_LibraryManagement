using System.ComponentModel.DataAnnotations;

namespace WebClient.Pages.Dto.Rating
{
    public class UpdateRatingDto
    {
        public int RatingId { get; set; }
        public int BookId { get; set; }
        
        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public int Score { get; set; }
    }
}
