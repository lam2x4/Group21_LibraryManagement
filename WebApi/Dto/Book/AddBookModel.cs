using System.ComponentModel.DataAnnotations;

public class AddBookModel
{
    [Required(ErrorMessage = "Tiêu đề sách là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "ISBN13 là bắt buộc.")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "ISBN13 phải có đúng 13 ký tự.")]
    public string? ISBN13 { get; set; }

    [Required(ErrorMessage = "Năm xuất bản là bắt buộc.")]
    public int PublicationYear { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "ID của nhà xuất bản là bắt buộc.")]
    public int PublisherId { get; set; }

    [Required(ErrorMessage = "Danh sách ID tác giả là bắt buộc.")]
    public List<int>? AuthorIds { get; set; }

    [Required(ErrorMessage = "Danh sách ID thể loại là bắt buộc.")]
    public List<int>? CategoryIds { get; set; }
}