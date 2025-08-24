namespace WebClient.Pages.Dto.Comment
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
}
