using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto.Loan
{
    public class CreateLoanDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string LibrarianName { get; set; }

        [Required]
        public DateTime LoanDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}
