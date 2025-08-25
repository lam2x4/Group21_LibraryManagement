using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto.Loan;
using WebApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public LoanController(LibraryDbContext context)
        {
            _context = context;
        }

        // Confirm loan request
        [HttpPost]
        public async Task<ActionResult<LoanDto>> CreateLoan(CreateLoanDto createLoanDto)
        {
            // Check if user has Librarian role
            if (Request.Cookies["UserRoles"] != "Librarian")
            {
                Console.WriteLine("Unauthorized: Only Librarians can create loans");
                return Unauthorized("Only Librarians can create loans");
            }

            try
            {
                Console.WriteLine($"CreateLoan called with ItemId: {createLoanDto.ItemId}, UserName: {createLoanDto.UserName}, LibrarianName: {createLoanDto.LibrarianName}");
                Console.WriteLine($"LoanDate: {createLoanDto.LoanDate}, DueDate: {createLoanDto.DueDate}");

                var bookItem = await _context.BookItems
                    .Include(bi => bi.Book)
                    .FirstOrDefaultAsync(bi => bi.ItemId == createLoanDto.ItemId);

                if (bookItem == null)
                {
                    Console.WriteLine($"BookItem not found for ItemId: {createLoanDto.ItemId}");
                    return NotFound("Book item not found");
                }

                Console.WriteLine($"BookItem found: {bookItem.Barcode} - Status: {bookItem.Status}");

                if (bookItem.Status != "Available")
                {
                    Console.WriteLine($"BookItem is not available. Status: {bookItem.Status}");
                    return BadRequest("Book item is not available for loan");
                }

                var user = await _context.Users.FindAsync(createLoanDto.UserName);
                if (user == null)
                {
                    Console.WriteLine($"User not found for UserId: {createLoanDto.UserName}");
                    return NotFound("User not found");
                }

                Console.WriteLine($"User found: {user.UserName}");

                var librarian = await _context.Users.FindAsync(createLoanDto.LibrarianName);
                if (librarian == null)
                {
                    Console.WriteLine($"Librarian not found for LibrarianId: {createLoanDto.UserName}");
                    return NotFound("Librarian not found");
                }

                Console.WriteLine($"Librarian found: {librarian.UserName}");

                var existingLoans = await _context.Loans
                    .Where(l => l.ItemId == createLoanDto.ItemId && !l.IsReturned)
                    .ToListAsync();

                Console.WriteLine($"Found {existingLoans.Count} existing loans for ItemId: {createLoanDto.ItemId}");

                var hasConflict = existingLoans.Any(existingLoan =>
                    (createLoanDto.LoanDate <= existingLoan.DueDate && createLoanDto.DueDate >= existingLoan.LoanDate));

                if (hasConflict)
                {
                    var conflictingLoan = existingLoans.First(l =>
                        (createLoanDto.LoanDate <= l.DueDate && createLoanDto.DueDate >= l.LoanDate));

                    Console.WriteLine($"Time conflict detected with loan: {conflictingLoan.LoanId}");
                    return BadRequest($"Sách này đã được mượn từ {conflictingLoan.LoanDate:dd/MM/yyyy} đến {conflictingLoan.DueDate:dd/MM/yyyy}. Vui lòng chọn thời gian khác.");
                }

                var loan = new Loan
                {
                    ItemId = createLoanDto.ItemId,
                    UserId = createLoanDto.UserName,
                    LibrarianId = createLoanDto.LibrarianName,
                    LoanDate = createLoanDto.LoanDate,
                    DueDate = createLoanDto.DueDate,
                    IsReturned = false
                };

                _context.Loans.Add(loan);
                bookItem.Status = "Loaned";

                await _context.SaveChangesAsync();

                Console.WriteLine($"Loan created successfully with LoanId: {loan.LoanId}");

                var loanDto = new LoanDto
                {
                    LoanId = loan.LoanId,
                    ItemId = loan.ItemId,
                    UserId = loan.UserId,
                    LoanDate = loan.LoanDate,
                    DueDate = loan.DueDate,
                    ReturnDate = loan.ReturnDate,
                    IsReturned = loan.IsReturned,
                    LibrarianId = loan.LibrarianId,
                    BookId = bookItem.Book.BookId,
                    BookTitle = bookItem.Book.Title,
                    BookImageUrl = bookItem.Book.ImageUrl,
                    Barcode = bookItem.Barcode,
                    Status = bookItem.Status,
                    UserName = user.UserName,
                    LibrarianName = librarian.UserName,
                    FineAmount = 0,
                    FineStatus = null
                };

                return CreatedAtAction(nameof(GetLoan), new { id = loan.LoanId }, loanDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateLoan: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get loan by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<LoanDto>> GetLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.BookItem)
                .ThenInclude(bi => bi.Book)
                .Include(l => l.User)
                .Include(l => l.Librarian)
                .Include(l => l.Fine)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
            {
                return NotFound();
            }

            var loanDto = new LoanDto
            {
                LoanId = loan.LoanId,
                ItemId = loan.ItemId,
                UserId = loan.UserId,
                LoanDate = loan.LoanDate,
                DueDate = loan.DueDate,
                ReturnDate = loan.ReturnDate,
                IsReturned = loan.IsReturned,
                LibrarianId = loan.LibrarianId,
                BookId = loan.BookItem.Book.BookId,
                BookTitle = loan.BookItem.Book.Title,
                BookImageUrl = loan.BookItem.Book.ImageUrl,
                Barcode = loan.BookItem.Barcode,
                Status = loan.BookItem.Status,
                UserName = loan.User.UserName,
                LibrarianName = loan.Librarian.UserName,
                FineAmount = loan.Fine?.Amount ?? 0,
                FineStatus = loan.Fine?.PaymentStatus
            };

            return Ok(loanDto);
        }

        // Get loans by user ID
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<LoanDto>>> GetUserLoans(string userId)
        {
            try
            {
                Console.WriteLine($"GetUserLoans called for UserId: {userId}");

                var loans = await _context.Loans
                    .Include(l => l.BookItem)
                    .ThenInclude(bi => bi.Book)
                    .Include(l => l.User)
                    .Include(l => l.Librarian)
                    .Include(l => l.Fine)
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.LoanDate)
                    .ToListAsync();

                Console.WriteLine($"Found {loans.Count} loans for user {userId}");

                var loanDtos = loans.Select(loan => new LoanDto
                {
                    LoanId = loan.LoanId,
                    ItemId = loan.ItemId,
                    UserId = loan.UserId,
                    LoanDate = loan.LoanDate,
                    DueDate = loan.DueDate,
                    ReturnDate = loan.ReturnDate,
                    IsReturned = loan.IsReturned,
                    LibrarianId = loan.LibrarianId,
                    BookId = loan.BookItem.Book.BookId,
                    BookTitle = loan.BookItem.Book.Title,
                    BookImageUrl = loan.BookItem.Book.ImageUrl,
                    Barcode = loan.BookItem.Barcode,
                    Status = loan.BookItem.Status,
                    UserName = loan.User.UserName,
                    LibrarianName = loan.Librarian.UserName,
                    FineAmount = loan.Fine?.Amount ?? 0,
                    FineStatus = loan.Fine?.PaymentStatus
                }).ToList();

                Console.WriteLine($"Returning {loanDtos.Count} loan DTOs");

                return Ok(loanDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUserLoans: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Check book availability
        [HttpGet("check-availability/{bookId}")]
        public async Task<ActionResult<object>> CheckAvailability(int bookId, [FromQuery] DateTime loanDate, [FromQuery] DateTime dueDate)
        {
            try
            {
                var book = await _context.Books
                    .FirstOrDefaultAsync(b => b.BookId == bookId);

                if (book == null)
                {
                    return NotFound("Book not found");
                }

                var bookItems = await _context.BookItems
                    .Where(bi => bi.BookId == bookId)
                    .ToListAsync();

                if (!bookItems.Any())
                {
                    return Ok(new
                    {
                        IsAvailable = false,
                        Message = "Không có bản sao nào của cuốn sách này"
                    });
                }

                var availableItems = bookItems.Where(bi => bi.Status == "Available").ToList();

                if (!availableItems.Any())
                {
                    return Ok(new
                    {
                        IsAvailable = false,
                        Message = "Tất cả bản sao của cuốn sách này đều đã được mượn"
                    });
                }

                var allItemIds = bookItems.Select(bi => bi.ItemId).ToList();
                var existingLoans = await _context.Loans
                    .Where(l => allItemIds.Contains(l.ItemId) && !l.IsReturned)
                    .ToListAsync();

                var hasConflict = existingLoans.Any(existingLoan =>
                    (loanDate <= existingLoan.DueDate && dueDate >= existingLoan.LoanDate));

                if (hasConflict)
                {
                    var conflictingLoan = existingLoans.First(l =>
                        (loanDate <= l.DueDate && dueDate >= l.LoanDate));

                    return Ok(new
                    {
                        IsAvailable = false,
                        Message = $"Cuốn sách này đã được mượn từ {conflictingLoan.LoanDate:dd/MM/yyyy} đến {conflictingLoan.DueDate:dd/MM/yyyy}",
                        ConflictingPeriod = new
                        {
                            StartDate = conflictingLoan.LoanDate,
                            EndDate = conflictingLoan.DueDate
                        }
                    });
                }

                return Ok(new
                {
                    IsAvailable = true,
                    Message = "Sách có thể mượn trong thời gian này",
                    BookInfo = new
                    {
                        Title = book.Title,
                        AvailableItems = availableItems.Count,
                        TotalItems = bookItems.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Record book return and calculate late fees
        [HttpPost("return/{loanId}")]
        public async Task<ActionResult<LoanDto>> ReturnBook(int loanId)
        {
            // Check if user has Librarian role
            if (Request.Cookies["UserRoles"] != "Librarian")
            {
                Console.WriteLine("Unauthorized: Only Librarians can process book returns");
                return Unauthorized("Only Librarians can process book returns");
            }

            try
            {
                Console.WriteLine($"ReturnBook called for LoanId: {loanId}");

                var loan = await _context.Loans
                    .Include(l => l.BookItem)
                    .ThenInclude(bi => bi.Book)
                    .Include(l => l.User)
                    .Include(l => l.Librarian)
                    .Include(l => l.Fine)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId);

                if (loan == null)
                {
                    Console.WriteLine($"Loan not found for LoanId: {loanId}");
                    return NotFound("Loan not found");
                }

                if (loan.IsReturned)
                {
                    Console.WriteLine($"Loan {loanId} is already returned");
                    return BadRequest("Sách đã được trả trước đó");
                }

                // Update loan
                loan.IsReturned = true;
                loan.ReturnDate = DateTime.Now;

                // Calculate late fee if applicable
                decimal lateFee = 0;
                const decimal dailyFine = 5.0m; // $5 per day late
                if (loan.ReturnDate > loan.DueDate)
                {
                    var daysLate = (loan.ReturnDate.Value - loan.DueDate).Days;
                    if (daysLate > 0)
                    {
                        lateFee = daysLate * dailyFine;
                        Console.WriteLine($"Late return detected. Days late: {daysLate}, Fine: {lateFee}");

                        // Create or update fine record
                        if (loan.Fine == null)
                        {
                            var fine = new Fine
                            {
                                LoanId = loan.LoanId,
                                Amount = lateFee,
                                Reason = $"Late return by {daysLate} days",
                                PaymentStatus = "Unpaid",
                                PaidDate = null
                            };
                            _context.Fines.Add(fine);
                            loan.Fine = fine;
                        }
                        else
                        {
                            loan.Fine.Amount = lateFee;
                            loan.Fine.Reason = $"Late return by {daysLate} days";
                            loan.Fine.PaymentStatus = "Unpaid";
                            loan.Fine.PaidDate = null;
                        }
                    }
                }

                // Update BookItem status
                loan.BookItem.Status = "Available";

                await _context.SaveChangesAsync();

                Console.WriteLine($"Book returned successfully for LoanId: {loanId}. Late fee: {lateFee}");

                var loanDto = new LoanDto
                {
                    LoanId = loan.LoanId,
                    ItemId = loan.ItemId,
                    UserId = loan.UserId,
                    LoanDate = loan.LoanDate,
                    DueDate = loan.DueDate,
                    ReturnDate = loan.ReturnDate,
                    IsReturned = loan.IsReturned,
                    LibrarianId = loan.LibrarianId,
                    BookId = loan.BookItem.Book.BookId,
                    BookTitle = loan.BookItem.Book.Title,
                    BookImageUrl = loan.BookItem.Book.ImageUrl,
                    Barcode = loan.BookItem.Barcode,
                    Status = loan.BookItem.Status,
                    UserName = loan.User.UserName,
                    LibrarianName = loan.Librarian.UserName,
                    FineAmount = loan.Fine?.Amount ?? 0,
                    FineStatus = loan.Fine?.PaymentStatus
                };

                return Ok(loanDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ReturnBook: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Cancel loan/reservation
        [HttpDelete("cancel/{loanId}")]
        public async Task<ActionResult> CancelLoan(int loanId)
        {
            // Check if user has Librarian role
            if (Request.Cookies["UserRoles"] != "Librarian")
            {
                Console.WriteLine("Unauthorized: Only Librarians can cancel loans");
                return Unauthorized("Only Librarians can cancel loans");
            }

            try
            {
                Console.WriteLine($"CancelLoan called for LoanId: {loanId}");

                var loan = await _context.Loans
                    .Include(l => l.BookItem)
                    .Include(l => l.Fine)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId);

                if (loan == null)
                {
                    Console.WriteLine($"Loan not found for LoanId: {loanId}");
                    return NotFound("Loan not found");
                }

                if (loan.IsReturned)
                {
                    Console.WriteLine($"Loan {loanId} is already returned and cannot be canceled");
                    return BadRequest("Cannot cancel a returned loan");
                }

                // Remove associated fine if exists
                if (loan.Fine != null)
                {
                    _context.Fines.Remove(loan.Fine);
                }

                // Update BookItem status to Available
                loan.BookItem.Status = "Available";

                // Remove the loan record
                _context.Loans.Remove(loan);

                await _context.SaveChangesAsync();

                Console.WriteLine($"Loan canceled successfully for LoanId: {loanId}");

                return Ok(new { Message = $"Loan {loanId} has been canceled successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CancelLoan: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}