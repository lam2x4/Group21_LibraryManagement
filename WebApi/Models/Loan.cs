using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
public class Loan
{
    [Key]
    public int LoanId { get; set; }

    public int ItemId { get; set; }
    [ForeignKey("ItemId")]
    public BookItem BookItem { get; set; }

    [Required]
    public string UserId { get; set; } // Liên kết với AspNetUsers.Id
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool IsReturned { get; set; }

    public string LibrarianId { get; set; } // Người thực hiện mượn/trả
    [ForeignKey("LibrarianId")]
    public ApplicationUser Librarian { get; set; }

    // Navigation property for the one-to-one relationship with Fine
    public Fine? Fine { get; set; }
}