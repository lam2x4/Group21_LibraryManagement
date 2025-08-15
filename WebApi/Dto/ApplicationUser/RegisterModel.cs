using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto.ApplicationUser
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
        [StringLength(256, ErrorMessage = "Tên người dùng không được quá 256 ký tự.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(256, ErrorMessage = "Mật khẩu không được quá 256 ký tự.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }
      
    }
}