using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BookAuthor
{
    // Khóa chính kết hợp
    [Key]
    [Column(Order = 0)]
    public int BookId { get; set; }

    [Key]
    [Column(Order = 1)]
    public int AuthorId { get; set; }

    // Navigation properties
    public Book Book { get; set; }
    public Author Author { get; set; }
}