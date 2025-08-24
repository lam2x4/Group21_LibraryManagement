using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Dto;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookItemsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BookItemsController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookItemDto>> GetBookItem(int id)
        {
            var bookItem = await _context.BookItems
                .Include(bi => bi.Book)
                .ThenInclude(b => b.Publisher)
                .FirstOrDefaultAsync(bi => bi.ItemId == id);

            if (bookItem == null)
            {
                return NotFound();
            }

            var bookItemDto = new BookItemDto
            {
                ItemId = bookItem.ItemId,
                BookId = bookItem.BookId,
                Barcode = bookItem.Barcode,
                Status = bookItem.Status,
                ShelfLocation = bookItem.ShelfLocation,
                BookTitle = bookItem.Book.Title,
                BookISBN13 = bookItem.Book.ISBN13,
                BookPublicationYear = bookItem.Book.PublicationYear,
                BookImageUrl = bookItem.Book.ImageUrl,
                BookDescription = bookItem.Book.Description,
                PublisherName = bookItem.Book.Publisher?.Name ?? "Unknown"
            };

            return Ok(bookItemDto);
        }

        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<List<BookItemDto>>> GetBookItemsByBookId(int bookId)
        {
            var bookItems = await _context.BookItems
                .Include(bi => bi.Book)
                .ThenInclude(b => b.Publisher)
                .Where(bi => bi.BookId == bookId)
                .ToListAsync();

            var bookItemDtos = bookItems.Select(bi => new BookItemDto
            {
                ItemId = bi.ItemId,
                BookId = bi.BookId,
                Barcode = bi.Barcode,
                Status = bi.Status,
                ShelfLocation = bi.ShelfLocation,
                BookTitle = bi.Book.Title,
                BookISBN13 = bi.Book.ISBN13,
                BookPublicationYear = bi.Book.PublicationYear,
                BookImageUrl = bi.Book.ImageUrl,
                BookDescription = bi.Book.Description,
                PublisherName = bi.Book.Publisher?.Name ?? "Unknown"
            }).ToList();

            return Ok(bookItemDtos);
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<BookItemDto>>> GetAvailableBookItems()
        {
            var availableItems = await _context.BookItems
                .Include(bi => bi.Book)
                .ThenInclude(b => b.Publisher)
                .Where(bi => bi.Status == "Available")
                .ToListAsync();

            var bookItemDtos = availableItems.Select(bi => new BookItemDto
            {
                ItemId = bi.ItemId,
                BookId = bi.BookId,
                Barcode = bi.Barcode,
                Status = bi.Status,
                ShelfLocation = bi.ShelfLocation,
                BookTitle = bi.Book.Title,
                BookISBN13 = bi.Book.ISBN13,
                BookPublicationYear = bi.Book.PublicationYear,
                BookImageUrl = bi.Book.ImageUrl,
                BookDescription = bi.Book.Description,
                PublisherName = bi.Book.Publisher?.Name ?? "Unknown"
            }).ToList();

            return Ok(bookItemDtos);
        }

        [HttpGet("test")]
        public async Task<ActionResult<object>> Test()
        {
            try
            {
                var totalItems = await _context.BookItems.CountAsync();
                var availableItems = await _context.BookItems.Where(bi => bi.Status == "Available").CountAsync();
                var books = await _context.Books.CountAsync();
                
                return Ok(new
                {
                    TotalBookItems = totalItems,
                    AvailableBookItems = availableItems,
                    TotalBooks = books,
                    Message = "Database connection successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
