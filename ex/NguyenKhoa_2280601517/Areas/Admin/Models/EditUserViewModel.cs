using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NguyenKhoa_2280601517.Areas.Admin.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; }

        [Display(Name = "Vai trò")]
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string SelectedRole { get; set; }

        public List<SelectListItem> RoleList { get; set; } = new List<SelectListItem> { };
    }
}
