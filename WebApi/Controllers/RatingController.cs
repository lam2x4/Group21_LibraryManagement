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
    public class RatingController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingController(LibraryDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateRating([FromBody] AddRatingDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ratingCount = await _context.Ratings
                .CountAsync(c => c.BookId == model.BookId && c.UserId == model.UserId);

            if (ratingCount >= 3)
            {
                return BadRequest(new { message = "Bạn chỉ được phép rating tối đa 3 lần cho mỗi cuốn sách." });
            }

            var rating = new Rating
            {
                BookId = model.BookId,
                UserId = model.UserId,
                Score = model.Score,
                RatingDate = DateTime.Now,
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateRating), new { id = rating.RatingId }, rating);
        }


        [HttpGet("GetByRating/{bookId}")]
        public async Task<IActionResult> GetCommentsByBook(int bookId)
        {
            var comments = await _context.Ratings
                .Where(c => c.BookId == bookId)
                .OrderByDescending(c => c.RatingDate)
                .ToListAsync();

            var result = new List<RatingDto>();

            foreach (var c in comments)
            {
                var user = await _userManager.FindByIdAsync(c.UserId);

                result.Add(new RatingDto
                {
                    RatingId = c.RatingId,
                    BookId = c.BookId,
                    UserId = c.UserId,
                    UserName = user?.UserName, 
                    Score = c.Score,
                    RatingDate = c.RatingDate
                });
            }

            return Ok(result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateRating([FromBody] UpdateRatingDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rating = await _context.Ratings.FindAsync(model.RatingId);
            if (rating == null)
            {
                return NotFound("Đánh giá không tồn tại.");
            }

            // Kiểm tra thời gian tạo rating (15 phút)
            var timeSinceCreation = DateTime.Now - rating.RatingDate;
            if (timeSinceCreation.TotalMinutes > 15)
            {
                return BadRequest(new { message = "Đánh giá chỉ có thể chỉnh sửa trong vòng 15 phút sau khi tạo." });
            }

            rating.Score = model.Score;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật đánh giá thành công." });
        }

        [HttpDelete("Delete/{ratingId}")]
        public async Task<IActionResult> DeleteRating(int ratingId)
        {
            var rating = await _context.Ratings.FindAsync(ratingId);
            if (rating == null)
            {
                return NotFound("Đánh giá không tồn tại.");
            }

            // Kiểm tra thời gian tạo rating (15 phút)
            var timeSinceCreation = DateTime.Now - rating.RatingDate;
            if (timeSinceCreation.TotalMinutes > 15)
            {
                return BadRequest(new { message = "Đánh giá chỉ có thể xóa trong vòng 15 phút sau khi tạo." });
            }

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa đánh giá thành công." });
        }
    }
}
