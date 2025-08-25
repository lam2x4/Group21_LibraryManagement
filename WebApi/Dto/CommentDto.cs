using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public int BookId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }  
        public string? Content { get; set; }
        public DateTime CommentDate { get; set; }
    }
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
