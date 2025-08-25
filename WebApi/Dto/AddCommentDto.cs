using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class AddCommentDto
    {
        public int BookId { get; set; }
        public string? UserId { get; set; }
        public string? Content { get; set; }
    }
    public class AddRatingDto
    {
        public int BookId { get; set; }
        public string? UserId { get; set; }
        public int Score { get; set; }
    }
}
