using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BookCategory
{
    // Khóa chính kết hợp
    [Key]
    [Column(Order = 0)]
    public int BookId { get; set; }
    [Key]
    [Column(Order = 1)]
    public int CategoryId { get; set; }

    // Navigation properties
    public Book Book { get; set; }
    public Category Category { get; set; }
}