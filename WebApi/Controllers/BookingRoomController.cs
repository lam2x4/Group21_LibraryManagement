using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingRoomController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BookingRoomController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/BookingRoom
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingRoomDto>>> GetBookingRooms()
        {
            var bookingRooms = await _context.BookingRooms
                .Include(br => br.Room)
                .Include(br => br.User)
                .Select(br => new BookingRoomDto
                {
                    BookingId = br.BookingId,
                    CheckInDate = br.CheckInDate,
                    CheckOutDate = br.CheckOutDate,
                    Status = br.Status,
                    RoomId = br.RoomId,
                    UserId = br.UserId,
                    Room = new RoomDto
                    {
                        RoomId = br.Room.RoomId,
                        RoomName = br.Room.RoomName,
                        RoomDescription = br.Room.RoomDescription,
                        PricePerNight = br.Room.PricePerNight,
                        IsAvailable = br.Room.IsAvailable
                    },
                    UserName = br.User.UserName
                })
                .ToListAsync();

            return Ok(bookingRooms);
        }

        // GET: api/BookingRoom/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingRoomDto>> GetBookingRoom(int id)
        {
            var bookingRoom = await _context.BookingRooms
                .Include(br => br.Room)
                .Include(br => br.User)
                .Where(br => br.BookingId == id)
                .Select(br => new BookingRoomDto
                {
                    BookingId = br.BookingId,
                    CheckInDate = br.CheckInDate,
                    CheckOutDate = br.CheckOutDate,
                    Status = br.Status,
                    RoomId = br.RoomId,
                    UserId = br.UserId,
                    Room = new RoomDto
                    {
                        RoomId = br.Room.RoomId,
                        RoomName = br.Room.RoomName,
                        RoomDescription = br.Room.RoomDescription,
                        PricePerNight = br.Room.PricePerNight,
                        IsAvailable = br.Room.IsAvailable
                    },
                    UserName = br.User.UserName
                })
                .FirstOrDefaultAsync();

            if (bookingRoom == null)
            {
                return NotFound();
            }

            return Ok(bookingRoom);
        }

        // GET: api/BookingRoom/check-availability
        [HttpGet("check-availability")]
        public async Task<ActionResult<object>> CheckAvailability(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            // Kiểm tra xem có booking nào trùng thời gian không
            var conflictingBooking = await _context.BookingRooms
                .Where(br => br.RoomId == roomId &&
                           br.Status != BookingStatus.Cancelled &&
                           ((br.CheckInDate <= checkInDate && br.CheckOutDate > checkInDate) ||
                            (br.CheckInDate < checkOutDate && br.CheckOutDate >= checkOutDate) ||
                            (br.CheckInDate >= checkInDate && br.CheckOutDate <= checkOutDate)))
                .FirstOrDefaultAsync();

            if (conflictingBooking != null)
            {
                return Ok(new
                {
                    IsAvailable = false,
                    Message = $"Phòng đã được đặt từ {conflictingBooking.CheckInDate:dd/MM/yyyy} đến {conflictingBooking.CheckOutDate:dd/MM/yyyy}. Vui lòng chọn khung giờ khác!",
                    ConflictingBooking = new
                    {
                        CheckInDate = conflictingBooking.CheckInDate,
                        CheckOutDate = conflictingBooking.CheckOutDate,
                        Status = conflictingBooking.Status
                    }
                });
            }

            return Ok(new
            {
                IsAvailable = true,
                Message = "Khung giờ này có sẵn để đặt phòng!"
            });
        }

        // GET: api/BookingRoom/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BookingRoomDto>>> GetUserBookingRooms(string userId)
        {
            var bookingRooms = await _context.BookingRooms
                .Include(br => br.Room)
                .Include(br => br.User)
                .Where(br => br.UserId == userId)
                .Select(br => new BookingRoomDto
                {
                    BookingId = br.BookingId,
                    CheckInDate = br.CheckInDate,
                    CheckOutDate = br.CheckOutDate,
                    Status = br.Status,
                    RoomId = br.RoomId,
                    UserId = br.UserId,
                    Room = new RoomDto
                    {
                        RoomId = br.Room.RoomId,
                        RoomName = br.Room.RoomName,
                        RoomDescription = br.Room.RoomDescription,
                        PricePerNight = br.Room.PricePerNight,
                        IsAvailable = br.Room.IsAvailable
                    },
                    UserName = br.User.UserName
                })
                .ToListAsync();

            return Ok(bookingRooms);
        }

        // POST: api/BookingRoom
        [HttpPost]
        public async Task<ActionResult<BookingRoomDto>> CreateBookingRoom(CreateBookingRoomDto createBookingRoomDto)
        {
            // Kiểm tra xem phòng có sẵn không
            var room = await _context.Rooms.FindAsync(createBookingRoomDto.RoomId);
            if (room == null)
            {
                return BadRequest("Room not found");
            }

            if (!room.IsAvailable)
            {
                return BadRequest("Room is not available");
            }

            // Kiểm tra xem có booking nào trùng thời gian không
            var conflictingBooking = await _context.BookingRooms
                .Where(br => br.RoomId == createBookingRoomDto.RoomId &&
                           br.Status != BookingStatus.Cancelled &&
                           ((br.CheckInDate <= createBookingRoomDto.CheckInDate && br.CheckOutDate > createBookingRoomDto.CheckInDate) ||
                            (br.CheckInDate < createBookingRoomDto.CheckOutDate && br.CheckOutDate >= createBookingRoomDto.CheckOutDate) ||
                            (br.CheckInDate >= createBookingRoomDto.CheckInDate && br.CheckOutDate <= createBookingRoomDto.CheckOutDate)))
                .FirstOrDefaultAsync();

            if (conflictingBooking != null)
            {
                return BadRequest($"Phòng đã được đặt từ {conflictingBooking.CheckInDate:dd/MM/yyyy} đến {conflictingBooking.CheckOutDate:dd/MM/yyyy}. Vui lòng chọn khung giờ khác!");
            }

            var bookingRoom = new BookingRoom
            {
                CheckInDate = createBookingRoomDto.CheckInDate,
                CheckOutDate = createBookingRoomDto.CheckOutDate,
                Status = BookingStatus.Pending,
                RoomId = createBookingRoomDto.RoomId,
                UserId = createBookingRoomDto.UserId
            };

            _context.BookingRooms.Add(bookingRoom);
            await _context.SaveChangesAsync();

            // Load related data for response
            await _context.Entry(bookingRoom)
                .Reference(br => br.Room)
                .LoadAsync();
            await _context.Entry(bookingRoom)
                .Reference(br => br.User)
                .LoadAsync();

            var bookingRoomDto = new BookingRoomDto
            {
                BookingId = bookingRoom.BookingId,
                CheckInDate = bookingRoom.CheckInDate,
                CheckOutDate = bookingRoom.CheckOutDate,
                Status = bookingRoom.Status,
                RoomId = bookingRoom.RoomId,
                UserId = bookingRoom.UserId,
                Room = new RoomDto
                {
                    RoomId = bookingRoom.Room.RoomId,
                    RoomName = bookingRoom.Room.RoomName,
                    RoomDescription = bookingRoom.Room.RoomDescription,
                    PricePerNight = bookingRoom.Room.PricePerNight,
                    IsAvailable = bookingRoom.Room.IsAvailable
                },
                UserName = bookingRoom.User.UserName
            };

            return CreatedAtAction(nameof(GetBookingRoom), new { id = bookingRoom.BookingId }, bookingRoomDto);
        }

        // PUT: api/BookingRoom/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBookingRoom(int id, UpdateBookingRoomDto updateBookingRoomDto)
        {
            var bookingRoom = await _context.BookingRooms.FindAsync(id);
            if (bookingRoom == null)
            {
                return NotFound();
            }

            bookingRoom.Status = updateBookingRoomDto.Status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingRoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/BookingRoom/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookingRoom(int id)
        {
            var bookingRoom = await _context.BookingRooms.FindAsync(id);
            if (bookingRoom == null)
            {
                return NotFound();
            }

            _context.BookingRooms.Remove(bookingRoom);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingRoomExists(int id)
        {
            return _context.BookingRooms.Any(e => e.BookingId == id);
        }
    }
}
