using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


public class Author
{
    [Key]
    public int AuthorId { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    public string? Bio { get; set; }

    // Navigation property for the many-to-many relationship with Book
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}