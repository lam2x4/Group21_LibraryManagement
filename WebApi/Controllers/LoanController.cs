using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto.Loan;
using WebApi.Models;

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

        [HttpPost]
        public async Task<ActionResult<LoanDto>> CreateLoan(CreateLoanDto createLoanDto)
        {
            try
            {
                Console.WriteLine($"CreateLoan called with ItemId: {createLoanDto.ItemId}, UserId: {createLoanDto.UserId}, LibrarianId: {createLoanDto.LibrarianId}");
                Console.WriteLine($"LoanDate: {createLoanDto.LoanDate}, DueDate: {createLoanDto.DueDate}");

                // Kiểm tra BookItem có tồn tại và available không
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

                // Kiểm tra User có tồn tại không
                var user = await _context.Users.FindAsync(createLoanDto.UserId);
                if (user == null)
                {
                    Console.WriteLine($"User not found for UserId: {createLoanDto.UserId}");
                    return NotFound("User not found");
                }

                Console.WriteLine($"User found: {user.UserName}");

                // Kiểm tra Librarian có tồn tại không
                var librarian = await _context.Users.FindAsync(createLoanDto.LibrarianId);
                if (librarian == null)
                {
                    Console.WriteLine($"Librarian not found for LibrarianId: {createLoanDto.LibrarianId}");
                    return NotFound("Librarian not found");
                }

                Console.WriteLine($"Librarian found: {librarian.UserName}");

                // Kiểm tra trùng lịch mượn sách
                var existingLoans = await _context.Loans
                    .Where(l => l.ItemId == createLoanDto.ItemId && !l.IsReturned)
                    .ToListAsync();

                Console.WriteLine($"Found {existingLoans.Count} existing loans for ItemId: {createLoanDto.ItemId}");

                // Kiểm tra xem có loan nào trùng thời gian không
                var hasConflict = existingLoans.Any(existingLoan => 
                    (createLoanDto.LoanDate <= existingLoan.DueDate && createLoanDto.DueDate >= existingLoan.LoanDate));

                if (hasConflict)
                {
                    var conflictingLoan = existingLoans.First(l => 
                        (createLoanDto.LoanDate <= l.DueDate && createLoanDto.DueDate >= l.LoanDate));
                    
                    Console.WriteLine($"Time conflict detected with loan: {conflictingLoan.LoanId}");
                    return BadRequest($"Sách này đã được mượn từ {conflictingLoan.LoanDate:dd/MM/yyyy} đến {conflictingLoan.DueDate:dd/MM/yyyy}. Vui lòng chọn thời gian khác.");
                }

                // Tạo loan mới
                var loan = new Loan
                {
                    ItemId = createLoanDto.ItemId,
                    UserId = createLoanDto.UserId,
                    LibrarianId = createLoanDto.LibrarianId,
                    LoanDate = createLoanDto.LoanDate, // Sử dụng ngày mượn từ request
                    DueDate = createLoanDto.DueDate,
                    IsReturned = false
                };

                _context.Loans.Add(loan);

                // Cập nhật status của BookItem thành "Loaned"
                bookItem.Status = "Loaned";

                await _context.SaveChangesAsync();

                Console.WriteLine($"Loan created successfully with LoanId: {loan.LoanId}");

                // Tạo response DTO
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
                    LibrarianName = librarian.UserName
                };

                return CreatedAtAction(nameof(GetLoan), new { id = loan.LoanId }, loanDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateLoan: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LoanDto>> GetLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.BookItem)
                .ThenInclude(bi => bi.Book)
                .Include(l => l.User)
                .Include(l => l.Librarian)
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
                LibrarianName = loan.Librarian.UserName
            };

            return Ok(loanDto);
        }

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
                    LibrarianName = loan.Librarian.UserName
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

        [HttpGet("check-availability/{bookId}")]
        public async Task<ActionResult<object>> CheckAvailability(int bookId, [FromQuery] DateTime loanDate, [FromQuery] DateTime dueDate)
        {
            try
            {
                // Kiểm tra Book có tồn tại không
                var book = await _context.Books
                    .FirstOrDefaultAsync(b => b.BookId == bookId);

                if (book == null)
                {
                    return NotFound("Book not found");
                }

                // Lấy tất cả BookItems của cuốn sách này
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

                // Kiểm tra xem có item nào available không
                var availableItems = bookItems.Where(bi => bi.Status == "Available").ToList();
                
                if (!availableItems.Any())
                {
                    return Ok(new
                    {
                        IsAvailable = false,
                        Message = "Tất cả bản sao của cuốn sách này đều đã được mượn"
                    });
                }

                // Kiểm tra trùng lịch cho tất cả các items
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

        [HttpPost("return/{loanId}")]
        public async Task<ActionResult<LoanDto>> ReturnBook(int loanId)
        {
            try
            {
                Console.WriteLine($"ReturnBook called for LoanId: {loanId}");

                var loan = await _context.Loans
                    .Include(l => l.BookItem)
                    .ThenInclude(bi => bi.Book)
                    .Include(l => l.User)
                    .Include(l => l.Librarian)
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

                // Cập nhật loan
                loan.IsReturned = true;
                loan.ReturnDate = DateTime.Now;

                // Cập nhật BookItem status
                loan.BookItem.Status = "Available";

                await _context.SaveChangesAsync();

                Console.WriteLine($"Book returned successfully for LoanId: {loanId}");

                // Trả về thông tin loan đã cập nhật
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
                    LibrarianName = loan.Librarian.UserName
                };

                return Ok(loanDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ReturnBook: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
