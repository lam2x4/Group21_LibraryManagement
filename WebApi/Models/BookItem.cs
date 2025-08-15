using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
public class BookItem
{
    [Key]
    public int ItemId { get; set; }

    public int BookId { get; set; }
    [ForeignKey("BookId")]
    public Book Book { get; set; }

    [Required]
    [MaxLength(50)]
    public string Barcode { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } // e.g., "Available", "Loaned", "Reserved", "Damaged", "Lost"

    [MaxLength(50)]
    public string? ShelfLocation { get; set; }

    // Navigation property for the one-to-many relationship with Loan
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}