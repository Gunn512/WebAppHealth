using System;
using System.Linq;
using System.Web.Mvc;
using WebAppHealth.Models.DataModel;
using WebAppHealth.Models.ViewModels;
using System.Collections.Generic;

namespace WebAppHealth.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: Profile
        public ActionResult Index()
        {
            // 1. Xác thực User
            var userEmail = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return RedirectToAction("Login", "Account");

            var patient = db.Patients.FirstOrDefault(p => p.UserID == user.UserID);

            // Nếu là tài khoản mới chưa có Patient Profile -> Trả về form trống
            if (patient == null)
            {
                return View(new UserProfileVM
                {
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = "~/asset/avatar-10.jpg"
                });
            }

            // Bước 2a: Lấy dữ liệu THÔ của lịch khám (Raw Data)
            var rawNextApp = (from a in db.Appointments
                              join d in db.Doctors on a.DoctorID equals d.DoctorID into dGroup
                              from doc in dGroup.DefaultIfEmpty()
                              join dep in db.Departments on (doc != null ? doc.DepartmentID : 0) equals dep.DepartmentID into depGroup
                              from dept in depGroup.DefaultIfEmpty()
                              where a.PatientID == patient.PatientID
                                    && a.AppointmentDate > DateTime.Now
                              orderby a.AppointmentDate ascending
                              select new
                              {
                                  a.AppointmentDate,
                                  DoctorName = doc != null ? doc.FullName : "Chưa chỉ định",
                                  DepartmentName = dept != null ? dept.Name : "Phòng khám tổng quát"
                              }).FirstOrDefault();

            // Bước 2b: Xử lý Format trên RAM
            AppointmentShortInfo nextApp = null;
            if (rawNextApp != null)
            {
                nextApp = new AppointmentShortInfo
                {
                    TimeStr = rawNextApp.AppointmentDate.HasValue ? rawNextApp.AppointmentDate.Value.ToString("HH:mm") : "--:--",
                    DateStr = rawNextApp.AppointmentDate.HasValue ? rawNextApp.AppointmentDate.Value.ToString("dd/MM/yyyy") : "",
                    DoctorName = rawNextApp.DoctorName,
                    DepartmentName = rawNextApp.DepartmentName
                };
            }

            // 3. Lấy Danh sách người thân
            var relatives = db.PatientRelatives
                              .Where(r => r.PatientID == patient.PatientID)
                              .Select(r => new RelativeInfo
                              {
                                  FullName = r.FullName,
                                  Relation = r.Relationship,
                                  Phone = r.PhoneNumber
                              }).ToList();

            // 4. Map dữ liệu vào ViewModel
            var model = new UserProfileVM
            {
                PatientID = patient.PatientID,
                PatientCode = patient.PatientCode,
                FullName = patient.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.ToString("yyyy-MM-dd") : "",
                Gender = patient.Gender,
                IdentityCard = patient.IdentityCard,
                InsuranceNumber = patient.InsuranceNumber,
                Address = patient.Address,
                City = patient.City,
                District = patient.District,
                Ward = patient.Ward,
                AvatarUrl = string.IsNullOrEmpty(patient.AvatarUrl) ? "~/asset/avatar-10.jpg" : patient.AvatarUrl,

                UpcomingAppointment = nextApp,
                Relatives = relatives
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công CSRF
        public ActionResult Update(UserProfileVM model, string[] relative_name, string[] relative_relation, string[] relative_phone)
        {
            // 1. Lấy thông tin User đang đăng nhập (Để đảm bảo không sửa hồ sơ người khác)
            var userEmail = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });

            var patient = db.Patients.FirstOrDefault(p => p.UserID == user.UserID);
            if (patient == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ bệnh nhân." });

            // Sử dụng Transaction để đảm bảo an toàn dữ liệu (Update cả 2 bảng hoặc không bảng nào)
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 2. CẬP NHẬT BẢNG PATIENTS (Chỉ cập nhật các trường cho phép)
                    // Lưu ý: KHÔNG cập nhật FullName, DateOfBirth, IdentityCard, InsuranceNumber

                    patient.Address = model.Address;
                    // Ví dụ: patient.AvatarUrl = model.AvatarUrl; 
                    patient.City = model.City;
                    patient.District = model.District;
                    patient.Ward = model.Ward;

                    // B3.1: Xóa danh sách cũ
                    var oldRelatives = db.PatientRelatives.Where(r => r.PatientID == patient.PatientID).ToList();
                    db.PatientRelatives.RemoveRange(oldRelatives);

                    // B3.2: Thêm danh sách mới (Nếu có dữ liệu gửi lên)
                    if (relative_name != null && relative_name.Length > 0)
                    {
                        for (int i = 0; i < relative_name.Length; i++)
                        {
                            // Kiểm tra dữ liệu rỗng trước khi thêm
                            if (!string.IsNullOrWhiteSpace(relative_name[i]))
                            {
                                var newRel = new PatientRelative
                                {
                                    PatientID = patient.PatientID,
                                    FullName = relative_name[i],
                                    // Kiểm tra null an toàn cho các mảng khác (phòng trường hợp lỗi JS gửi thiếu)
                                    Relationship = (relative_relation != null && i < relative_relation.Length) ? relative_relation[i] : "Khác",
                                    PhoneNumber = (relative_phone != null && i < relative_phone.Length) ? relative_phone[i] : ""
                                };
                                db.PatientRelatives.Add(newRel);
                            }
                        }
                    }

                    // 4. LƯU VÀO DATABASE
                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true, message = "Cập nhật hồ sơ thành công!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
            }
        }
    }
}