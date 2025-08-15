using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
public class Publisher
{
    [Key]
    public int PublisherId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    // Navigation property for the one-to-many relationship with Book
    public ICollection<Book> Books { get; set; } = new List<Book>();
}