using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    public int BookId { get; set; }
    [ForeignKey("BookId")]
    public Book Book { get; set; }

    [Required]
    public string UserId { get; set; } // Liên kết với AspNetUsers.Id
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public DateTime ReservationDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } // e.g., "Pending", "Ready", "Canceled", "Fulfilled"
}