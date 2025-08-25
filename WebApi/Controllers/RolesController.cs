using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Dto.Role;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                var roleDtos = new List<RoleDto>();

                foreach (var role in roles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                    roleDtos.Add(new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name ?? "",
                        NormalizedName = role.NormalizedName ?? "",
                        UserCount = usersInRole.Count
                    });
                }

                return Ok(roleDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách vai trò: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound("Không tìm thấy vai trò");
                }

                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? "",
                    NormalizedName = role.NormalizedName ?? "",
                    UserCount = usersInRole.Count
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy thông tin vai trò: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDto createRoleDto)
        {
            try
            {
                // Kiểm tra role đã tồn tại chưa
                var existingRole = await _roleManager.FindByNameAsync(createRoleDto.Name);
                if (existingRole != null)
                {
                    return BadRequest("Vai trò đã tồn tại");
                }

                var role = new IdentityRole
                {
                    Name = createRoleDto.Name,
                    NormalizedName = createRoleDto.Name.ToUpper()
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest($"Tạo vai trò thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                return Ok("Tạo vai trò thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo vai trò: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, UpdateRoleDto updateRoleDto)
        {
            try
            {
                if (id != updateRoleDto.Id)
                {
                    return BadRequest("ID không khớp");
                }

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound("Không tìm thấy vai trò");
                }

                // Không cho phép sửa các role hệ thống
                if (role.Name == "Admin" || role.Name == "User" || role.Name == "Librarian")
                {
                    return BadRequest("Không thể sửa vai trò hệ thống");
                }

                // Kiểm tra tên mới có trùng không
                var existingRole = await _roleManager.FindByNameAsync(updateRoleDto.Name);
                if (existingRole != null && existingRole.Id != id)
                {
                    return BadRequest("Tên vai trò đã được sử dụng");
                }

                role.Name = updateRoleDto.Name;
                role.NormalizedName = updateRoleDto.Name.ToUpper();

                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest($"Cập nhật vai trò thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                return Ok("Cập nhật vai trò thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật vai trò: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound("Không tìm thấy vai trò");
                }

                // Không cho phép xóa các role hệ thống
                if (role.Name == "Admin" || role.Name == "User" || role.Name == "Librarian")
                {
                    return BadRequest("Không thể xóa vai trò hệ thống");
                }

                // Kiểm tra có user nào đang sử dụng role này không
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                if (usersInRole.Any())
                {
                    return BadRequest($"Không thể xóa vai trò đang được sử dụng bởi {usersInRole.Count} người dùng");
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest($"Xóa vai trò thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                return Ok("Xóa vai trò thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa vai trò: {ex.Message}");
            }
        }

        [HttpGet("{id}/users")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersInRole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound("Không tìm thấy vai trò");
                }

                var users = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                var userList = users.Select(u => new
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                }).ToList();

                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách người dùng: {ex.Message}");
            }
        }
    }
}
