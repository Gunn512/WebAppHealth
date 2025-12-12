using System.Collections.Generic;

namespace WebAppHealth.Models.ViewModels
{
    public class UserProfileVM
    {
        public int PatientID { get; set; }
        public string PatientCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; } 
        public string PhoneNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string AvatarUrl { get; set; }

        // --- ĐỊNH DANH & BHYT ---
        public string IdentityCard { get; set; }
        public string InsuranceNumber { get; set; }

        // --- ĐỊA CHỈ ---
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }


        // 1. Lịch khám sắp tới (Lấy cái gần nhất)
        public AppointmentShortInfo UpcomingAppointment { get; set; }

        // 2. Danh sách người thân
        public List<RelativeInfo> Relatives { get; set; }

        public UserProfileVM()
        {
            Relatives = new List<RelativeInfo>();
        }
    }

    // Class con: Lịch hẹn ngắn gọn
    public class AppointmentShortInfo
    {
        public string TimeStr { get; set; }       // VD: 14:00
        public string DateStr { get; set; }       // VD: 21/10/2025
        public string DepartmentName { get; set; }
        public string DoctorName { get; set; }
    }

    // Class con: Người thân
    public class RelativeInfo
    {
        public string FullName { get; set; }
        public string Relation { get; set; }
        public string Phone { get; set; }
    }
}