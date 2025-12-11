using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebAppHealth.Models
{
    public class RegisterServiceViewModel
    {
        public string FullName { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string CCCD { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên khoa")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên khoa trước")]
        public int DoctorID { get; set; }

        public string Symptoms { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên khoa và bác sĩ trước")]
        public string LichHen { get; set; }
    }
}