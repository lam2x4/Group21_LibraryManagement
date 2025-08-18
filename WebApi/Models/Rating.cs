using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5.")]
        public int Score { get; set; }

        [Required]
        public DateTime RatingDate { get; set; }

        // Navigation properties
        public Book? Book { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
