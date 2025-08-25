using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public RoomController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/Room
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
        {
            var rooms = await _context.Rooms
                .Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    RoomDescription = r.RoomDescription,
                    PricePerNight = r.PricePerNight,
                    IsAvailable = r.IsAvailable
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // GET: api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Where(r => r.RoomId == id)
                .Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    RoomDescription = r.RoomDescription,
                    PricePerNight = r.PricePerNight,
                    IsAvailable = r.IsAvailable
                })
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        // POST: api/Room
        [HttpPost]
        public async Task<ActionResult<RoomDto>> CreateRoom(CreateRoomDto createRoomDto)
        {
            var room = new Room
            {
                RoomName = createRoomDto.RoomName,
                RoomDescription = createRoomDto.RoomDescription,
                PricePerNight = createRoomDto.PricePerNight,
                IsAvailable = createRoomDto.IsAvailable
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var roomDto = new RoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                RoomDescription = room.RoomDescription,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable
            };

            return CreatedAtAction(nameof(GetRoom), new { id = room.RoomId }, roomDto);
        }

        // PUT: api/Room/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, UpdateRoomDto updateRoomDto)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.RoomName = updateRoomDto.RoomName;
            room.RoomDescription = updateRoomDto.RoomDescription;
            room.PricePerNight = updateRoomDto.PricePerNight;
            room.IsAvailable = updateRoomDto.IsAvailable;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
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

        // DELETE: api/Room/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
    }
}
