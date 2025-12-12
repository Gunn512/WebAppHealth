using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebAppHealth.Models
{
    public class RegisterExamViewModel
    {
        public string FullName { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string CCCD { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public string HasHealthInsurance { get; set; }
        public string HealthInsuranceNumber { get; set; }

        public int DepartmentID { get; set; }

        public string Symptoms { get; set; }

        public string LichHen { get; set; }
    }
}