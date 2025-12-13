using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebAppHealth.Areas.Admin.Models
{
    public class AppointmentViewModel
    {
        public int AppointmentID { get; set; }
        public string PatientName { get; set; }
        public string PatientCode { get; set; }
        public string Phone { get; set; }

        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? AppointmentDate { get; set; }

        public string TimeSlot { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }

        // Dùng để tô màu badge trạng thái
        public string StatusClass
        {
            get
            {
                if (Status == "Confirmed") return "success";
                if (Status == "Canceled") return "danger";
                return "warning";
            }
        }
    }
}