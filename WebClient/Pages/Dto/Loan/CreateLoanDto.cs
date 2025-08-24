using System.ComponentModel.DataAnnotations;

namespace WebClient.Pages.Dto.Loan
{
    public class CreateLoanDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string LibrarianId { get; set; }

        [Required]
        public DateTime LoanDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}
