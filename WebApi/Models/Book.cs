using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
public class Book
{
    [Key]
    public int BookId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; }

    public int PublisherId { get; set; }
    [ForeignKey("PublisherId")]
    public Publisher Publisher { get; set; }

    [Required]
    [MaxLength(13)]
    public string ISBN13 { get; set; }

    public int PublicationYear { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    // Navigation properties for relationships
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
    public ICollection<BookItem> BookItems { get; set; } = new List<BookItem>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}