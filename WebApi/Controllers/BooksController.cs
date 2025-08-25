using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ODataController
{
    private readonly LibraryDbContext _context;

    public BooksController(LibraryDbContext context)
    {
        _context = context;
    }

    // ===================== ODATA GET =====================
    // GET /odata/Books
    [EnableQuery]
    [HttpGet] 
    [Route("/odata/Books")]
    public IActionResult GetOData()
    {
        return Ok(_context.Books.Include(x => x.Publisher));
    }

    // ===================== API CRUD =====================
    // POST /api/Books
    // Trong BooksController.cs

    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] AddBookModel bookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Tạo một đối tượng Book từ DTO
        var book = new Book
        {
            Title = bookDto.Title,
            ISBN13 = bookDto.ISBN13,
            PublicationYear = bookDto.PublicationYear,
            Description = bookDto.Description,
            ImageUrl = bookDto.ImageUrl,
            PublisherId = bookDto.PublisherId
        };

        // Thêm sách vào context
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Thêm các mối quan hệ nhiều-nhiều (Authors và Categories)
        foreach (var authorId in bookDto.AuthorIds)
        {
            var bookAuthor = new BookAuthor { BookId = book.BookId, AuthorId = authorId };
            _context.BookAuthors.Add(bookAuthor);
        }

        foreach (var categoryId in bookDto.CategoryIds)
        {
            var bookCategory = new BookCategory { BookId = book.BookId, CategoryId = categoryId };
            _context.BookCategories.Add(bookCategory);
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(AddBook), new { id = book.BookId }, book);
    }

    // PUT /api/Books/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Book updatedBook)
    {
        if (id != updatedBook.BookId)
            return BadRequest("Id không khớp với BookId");

        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        _context.Entry(book).CurrentValues.SetValues(updatedBook);
        await _context.SaveChangesAsync();
        return Ok(book);
    }

    // DELETE /api/Books/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
