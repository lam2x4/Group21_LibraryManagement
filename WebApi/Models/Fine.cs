using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

public class Fine
{
    [Key]
    public int FineId { get; set; }

    public int LoanId { get; set; }
    [ForeignKey("LoanId")]
    public Loan Loan { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public string? Reason { get; set; }

    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } // e.g., "Paid", "Unpaid", "Waived"

    public DateTime? PaidDate { get; set; }
}