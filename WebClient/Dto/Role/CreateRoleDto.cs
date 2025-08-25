using System.ComponentModel.DataAnnotations;

namespace WebClient.Pages.Dto.Role
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Tên vai trò là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên vai trò không được quá 50 ký tự")]
        public string Name { get; set; } = string.Empty;
    }
}
