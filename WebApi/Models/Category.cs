using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    // Navigation property for the many-to-many relationship with Book
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}