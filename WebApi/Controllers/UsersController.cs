using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto.ApplicationUser;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            try
            {
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return BadRequest("Email đã được sử dụng");
                }

                // Kiểm tra username đã tồn tại chưa
                existingUser = await _userManager.FindByNameAsync(createUserDto.UserName);
                if (existingUser != null)
                {
                    return BadRequest("Tên đăng nhập đã được sử dụng");
                }

                // Tạo user mới
                var user = new ApplicationUser
                {
                    UserName = createUserDto.UserName,
                    Email = createUserDto.Email,
                    PhoneNumber = createUserDto.PhoneNumber,
                    EmailConfirmed = true, // Admin tạo sẽ confirmed luôn
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(user, createUserDto.Password);
                if (!result.Succeeded)
                {
                    return BadRequest($"Tạo tài khoản thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Thêm roles
                if (createUserDto.Roles.Any())
                {
                    await _userManager.AddToRolesAsync(user, createUserDto.Roles);
                }
                else
                {
                    // Mặc định là User nếu không chọn role
                    await _userManager.AddToRoleAsync(user, "User");
                }

                return Ok("Tạo tài khoản thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo tài khoản: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        PhoneNumber = user.PhoneNumber ?? "",
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                        LockoutEnabled = user.LockoutEnabled,
                        LockoutEnd = user.LockoutEnd,
                        Roles = roles.ToList(),
                        CreatedDate = DateTime.Now // Identity doesn't have created date by default
                    });
                }

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách người dùng: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    Roles = roles.ToList(),
                    CreatedDate = DateTime.Now
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy thông tin người dùng: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto updateUserDto)
        {
            try
            {
                if (id != updateUserDto.Id)
                {
                    return BadRequest("ID không khớp");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Cập nhật thông tin cơ bản
                user.UserName = updateUserDto.UserName;
                user.Email = updateUserDto.Email;
                user.PhoneNumber = updateUserDto.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest($"Cập nhật thông tin thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Cập nhật roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(updateUserDto.Roles).ToList();
                var rolesToAdd = updateUserDto.Roles.Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                return Ok("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật người dùng: {ex.Message}");
            }
        }



        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Không cho phép khóa chính mình
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && currentUser.Id == id)
                {
                    return BadRequest("Không thể khóa chính mình");
                }

                // Bật chức năng lockout và khóa tài khoản trong 100 năm
                await _userManager.SetLockoutEnabledAsync(user, true);
                var lockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

                if (!result.Succeeded)
                {
                    return BadRequest($"Khóa tài khoản thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                return Ok("Khóa tài khoản thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi khóa tài khoản: {ex.Message}");
            }
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Mở khóa tài khoản
                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (!result.Succeeded)
                {
                    return BadRequest($"Mở khóa tài khoản thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                return Ok("Mở khóa tài khoản thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi mở khóa tài khoản: {ex.Message}");
            }
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách vai trò: {ex.Message}");
            }
        }
    }
}
