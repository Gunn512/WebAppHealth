using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; // Cần cái này để dùng .Include()
using WebAppHealth.Models.DataModel;
using WebAppHealth.Areas.Admin.Models;

namespace WebAppHealth.Areas.Admin.Controllers
{
    public class AppointmentsController : BaseAdminController
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: Admin/Appointments
        public ActionResult Index(DateTime? date, int? doctorId, string status)
        {
            // 1. Khởi tạo query, Include các bảng liên quan để lấy tên
            var query = db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.Department)
                .AsQueryable();

            // 2. Áp dụng bộ lọc (Filter)
            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate == date.Value);
            }

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorID == doctorId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // 3. Sắp xếp: Mới nhất lên đầu
            query = query.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.TimeSlot);

            // 4. Chuyển đổi sang ViewModel
            var model = query.Select(a => new AppointmentViewModel
            {
                AppointmentID = a.AppointmentID,
                PatientName = a.FullName ?? a.Patient.FullName, // Ưu tiên tên nhập lúc đặt, nếu null lấy tên trong hồ sơ
                PatientCode = a.Patient.PatientCode,
                Phone = a.Phone,
                DoctorName = a.Doctor.FullName,
                DepartmentName = a.Doctor.Department.Name,
                AppointmentDate = a.AppointmentDate,
                TimeSlot = a.TimeSlot,
                Status = a.Status ?? "Pending",
                Type = a.Type
            }).ToList();

            // 5. Chuẩn bị dữ liệu cho Dropdown Filter ở View
            ViewBag.Doctors = new SelectList(db.Doctors, "DoctorID", "FullName", doctorId);
            ViewBag.CurrentDate = date?.ToString("yyyy-MM-dd");
            ViewBag.CurrentStatus = status;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(int id)
        {
            try
            {
                var appointment = db.Appointments.Find(id);
                if (appointment == null) return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });

                appointment.Status = "Confirmed";

                db.SaveChanges();
                return Json(new { success = true, message = "Đã xác nhận lịch hẹn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            try
            {
                var appointment = db.Appointments.Find(id);
                if (appointment == null) return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });

                appointment.Status = "Canceled";
                // Logic hoàn tiền hoặc gửi mail hủy

                db.SaveChanges();
                return Json(new { success = true, message = "Đã hủy lịch hẹn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}