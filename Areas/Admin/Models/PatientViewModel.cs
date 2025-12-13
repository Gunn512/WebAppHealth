using System;
using System.ComponentModel.DataAnnotations;

namespace WebAppHealth.Areas.Admin.Models
{
    public class PatientViewModel
    {
        public int PatientID { get; set; }

        [Display(Name = "Mã Bệnh nhân")]
        public string PatientCode { get; set; } // Ví dụ: BN000123

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [Display(Name = "Ngày sinh")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } // Có thể ghép từ Address + Ward + District + City

        public bool IsActive { get; set; } // Trạng thái tài khoản (từ bảng Users)
        public int UserID { get; set; }    // Để phục vụ việc khóa tài khoản
    }
}