using System;
using System.ComponentModel.DataAnnotations;

namespace WebAppHealth.Models
{
    // Dữ liệu nhận từ Form
    public class LookupRequestVM
    {
        [Required]
        public string Phone { get; set; }
        [Required]
        public string CCCD { get; set; }
        [Required]
        public DateTime? DOB { get; set; }
        public string Captcha { get; set; }
    }

    // Dữ liệu hiển thị danh sách
    public class AppointmentListItemVM
    {
        public int AppointmentID { get; set; }
        public string AppointmentCode { get; set; }
        public string AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Cancelled, Completed
        public string StatusVN { get; set; } // Hiển thị tiếng Việt
        public bool AllowCancel { get; set; } // Cho phép hủy không
    }

    // Dữ liệu hiển thị chi tiết (Vé khám bên phải)
    public class AppointmentDetailVM
    {
        public int AppointmentID { get; set; }
        public string MaBN { get; set; }
        public string MaKCB { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string DoiTuong { get; set; }
        public string NgayKham { get; set; }
        public string GioKham { get; set; }
        public string ChuyenKhoa { get; set; }
        public string BacSi { get; set; }
        public string PhongKham { get; set; }
        public string Status { get; set; }
        public string TrieuChung { get; set; }
        public string BarcodeData { get; set; }
    }
}