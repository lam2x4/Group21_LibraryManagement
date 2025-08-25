using System.ComponentModel.DataAnnotations;

namespace WebClient.Pages.Dto.Rating
{
    public class AddRatingDto
    {
        public int BookId { get; set; }
        public string? UserId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn điểm đánh giá")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public int Score { get; set; }
    }
}
