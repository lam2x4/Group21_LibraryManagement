using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using WebApi.Models;

public class ApplicationUser : IdentityUser
{
    // Navigation properties
    public ICollection<Loan> LoansAsUser { get; set; }
    public ICollection<Loan> LoansAsLibrarian { get; set; }  
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}