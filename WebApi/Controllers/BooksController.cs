using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ODataController
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET /odata/Books
        // Cho phép truy vấn OData trên tập hợp Book
        [EnableQuery]
        [HttpGet("/odata/Books")]
        public IActionResult Get()
        {
            return Ok(_context.Books);
        }

        // POST /odata/Books
        // Thêm một cuốn sách mới
        [HttpPost("/api/Books")]
        public async Task<IActionResult> Post([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return Created(book);
        }

        // PUT /odata/Books(1)
        // Cập nhật thông tin của một cuốn sách
        [HttpPut("{key}")]
        public async Task<IActionResult> Put([FromODataUri] int key, [FromBody] Book updatedBook)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var book = await _context.Books.FindAsync(key);
            if (book == null)
            {
                return NotFound();
            }

            _context.Entry(book).CurrentValues.SetValues(updatedBook);
            await _context.SaveChangesAsync();
            return Updated(book);
        }

        // DELETE /odata/Books(1)
        // Xóa một cuốn sách
        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var book = await _context.Books.FindAsync(key);
            if (book == null)
            {
                return NotFound();
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
