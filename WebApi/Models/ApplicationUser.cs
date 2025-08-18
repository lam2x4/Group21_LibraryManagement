using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{
    // Navigation properties
    public ICollection<Loan> LoansAsUser { get; set; }
    public ICollection<Loan> LoansAsLibrarian { get; set; }
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}