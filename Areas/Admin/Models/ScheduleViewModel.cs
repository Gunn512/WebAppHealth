using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppHealth.Areas.Admin.Models
{
    // Dùng để hiển thị ra danh sách
    public class ScheduleViewModel
    {
        public int ScheduleID { get; set; }

        [Display(Name = "Bác sĩ")]
        public string DoctorName { get; set; }

        [Display(Name = "Khoa")]
        public string DepartmentName { get; set; }

        [Display(Name = "Ngày làm việc")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateWork { get; set; }

        [Display(Name = "Khung giờ")]
        public string TimeSlot { get; set; } // Ví dụ: "07:00 - 08:00"

        public string Status { get; set; } // Available, Full...
    }

    // Dùng để tạo mới (Chọn 1 ngày, check nhiều slot)
    public class CreateScheduleViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày làm việc")]
        [DataType(DataType.Date)]
        public DateTime DateWork { get; set; }

        // Danh sách ID các slot được chọn
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một khung giờ")]
        public List<int> SelectedSlotIDs { get; set; }
    }
}