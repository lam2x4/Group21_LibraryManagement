using System;

namespace WebClient.Pages.Dto.Loan
{
    public class LoanDto
    {
        public int LoanId { get; set; }
        public int ItemId { get; set; }
        public string UserId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public string LibrarianId { get; set; }
        
        // Book information
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookImageUrl { get; set; }
        public string Barcode { get; set; }
        public string Status { get; set; }
        
        // User information
        public string UserName { get; set; }
        public string LibrarianName { get; set; }
    }
}
