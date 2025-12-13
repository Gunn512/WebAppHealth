using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; // Để dùng .Include()
using WebAppHealth.Models.DataModel;
using WebAppHealth.Areas.Admin.Models;

namespace WebAppHealth.Areas.Admin.Controllers
{
    public class PatientsController : BaseAdminController
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: Admin/Patients
        public ActionResult Index(string search)
        {
            // 1. Lấy dữ liệu Patients và Join sang bảng Users
            var query = db.Patients.Include(p => p.User).AsQueryable();

            // 2. Tìm kiếm theo Tên, Mã BN hoặc SĐT
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.FullName.Contains(search) ||
                                         p.PatientCode.Contains(search) ||
                                         p.User.PhoneNumber.Contains(search));
            }

            // 3. Map sang ViewModel
            var model = query.Select(p => new PatientViewModel
            {
                PatientID = p.PatientID,
                UserID = p.User.UserID,
                PatientCode = p.PatientCode,
                FullName = p.FullName,
                Gender = p.Gender,
                DateOfBirth = p.DateOfBirth,
                PhoneNumber = p.User.PhoneNumber, // Lấy SĐT từ bảng Users (hoặc bảng Patients tùy DB của bạn lưu ở đâu)
                Email = p.User.Email,
                Address = p.Address, // Tạm thời lấy địa chỉ chi tiết
                IsActive = p.User.IsActive ?? true
            }).OrderByDescending(p => p.PatientID).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        // GET: Admin/Patients/Details/5
        public ActionResult Details(int id)
        {
            var patient = db.Patients.Include(p => p.User).FirstOrDefault(p => p.PatientID == id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            // Trả về View Details (bạn có thể tự tạo View này sau nếu cần xem kỹ hồ sơ bệnh án)
            return View(patient);
        }

        // POST: Admin/Patients/ToggleLock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleLock(int id)
        {
            try
            {
                // Tìm bệnh nhân
                var patient = db.Patients.Include(p => p.User).FirstOrDefault(p => p.PatientID == id);
                if (patient == null || patient.User == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản liên kết." });
                }

                // Đảo ngược trạng thái khóa
                bool currentStatus = patient.User.IsActive ?? true;
                patient.User.IsActive = !currentStatus;

                db.SaveChanges();

                string msg = !currentStatus ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản bệnh nhân.";
                return Json(new { success = true, message = msg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}