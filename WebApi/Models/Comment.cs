using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        [MaxLength(2000, ErrorMessage = "Nội dung bình luận không được vượt quá 2000 ký tự.")]
        public string? Content { get; set; }

        [Required]
        public DateTime CommentDate { get; set; }

        // Tùy chọn: Dùng cho bình luận trả lời (reply)
        public int? ParentCommentId { get; set; }

        // Navigation properties
        public Book? Book { get; set; }
        public ApplicationUser? User { get; set; }

        // Tùy chọn: Navigation properties cho bình luận trả lời
        [ForeignKey("ParentCommentId")]
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
