using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebAppHealth.Areas.Admin.Models
{
    public class DoctorViewModel
    {
        public int DoctorID { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Chuyên khoa")]
        public string DepartmentName { get; set; }
        public int? DepartmentID { get; set; } // Dùng cho Dropdown khi Create/Edit

        [Display(Name = "Ảnh đại diện")]
        public string Avatar { get; set; }
        public HttpPostedFileBase AvatarFile { get; set; } // Dùng để upload ảnh

        [Display(Name = "Giới thiệu")]
        public string Bio { get; set; }

        [Display(Name = "Học vị")]
        public string Degree { get; set; } // Ví dụ: Thạc sĩ, Tiến sĩ

        // Thông tin từ bảng Users
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }
}