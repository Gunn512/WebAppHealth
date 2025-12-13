using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Models
{
    public class IntroViewModel
    {
        // Danh sách bác sĩ
        public List<Doctor> Directors { get; set; }

        // Danh sách Phòng chức năng
        public List<Department> FunctionalDepartments { get; set; }

        public Dictionary<int, string> PositionMap { get; set; }
    }
}