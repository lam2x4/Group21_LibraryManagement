using System.ComponentModel.DataAnnotations;

namespace WebClient.Pages.Dto.Comment
{
    public class AddCommentDto
    {
        public int BookId { get; set; }
        public string? UserId { get; set; }
        
        [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        public string Content { get; set; } = string.Empty;
    }
}
