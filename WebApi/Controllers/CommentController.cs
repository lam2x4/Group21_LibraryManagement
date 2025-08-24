using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(LibraryDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateComment([FromBody] AddCommentDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var commentCount = await _context.Comments
                .CountAsync(c => c.BookId == model.BookId && c.UserId == model.UserId);

            if (commentCount >= 3)
            {
                return BadRequest(new { message = "Bạn chỉ được phép bình luận tối đa 3 lần cho mỗi cuốn sách." });
            }

            var comment = new Comment
            {
                BookId = model.BookId,
                UserId = model.UserId,
                Content = model.Content,
                CommentDate = DateTime.Now,
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateComment), new { id = comment.CommentId }, comment);
        }


        [HttpGet("GetByBook/{bookId}")]
        public async Task<IActionResult> GetCommentsByBook(int bookId)
        {
            var comments = await _context.Comments
                .Where(c => c.BookId == bookId)
                .OrderByDescending(c => c.CommentDate)
                .ToListAsync();

            var result = new List<CommentDto>();

            foreach (var c in comments)
            {
                var user = await _userManager.FindByIdAsync(c.UserId);

                result.Add(new CommentDto
                {
                    CommentId = c.CommentId,
                    BookId = c.BookId,
                    UserId = c.UserId,
                    UserName = user?.UserName, // có thể null nếu user đã bị xóa
                    Content = c.Content,
                    CommentDate = c.CommentDate
                });
            }

            return Ok(result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _context.Comments.FindAsync(model.CommentId);
            if (comment == null)
            {
                return NotFound("Bình luận không tồn tại.");
            }

            // Kiểm tra thời gian tạo comment (15 phút)
            var timeSinceCreation = DateTime.Now - comment.CommentDate;
            if (timeSinceCreation.TotalMinutes > 15)
            {
                return BadRequest(new { message = "Bình luận chỉ có thể chỉnh sửa trong vòng 15 phút sau khi tạo." });
            }

            comment.Content = model.Content;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật bình luận thành công." });
        }

        [HttpDelete("Delete/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound("Bình luận không tồn tại.");
            }

            // Kiểm tra thời gian tạo comment (15 phút)
            var timeSinceCreation = DateTime.Now - comment.CommentDate;
            if (timeSinceCreation.TotalMinutes > 15)
            {
                return BadRequest(new { message = "Bình luận chỉ có thể xóa trong vòng 15 phút sau khi tạo." });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa bình luận thành công." });
        }
    }
}
