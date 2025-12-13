using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class AppointmentManageController : Controller
    {
        private HealthDBContext db = new HealthDBContext();

        public ActionResult Index()
        {
            return View();
        }

        // 1. XỬ LÝ TÌM KIẾM -> CHUYỂN HƯỚNG
        [HttpPost]
        public async Task<ActionResult> Lookup(LookupRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
            }

            // Tìm kiếm
            var query = db.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.Phone == model.Phone && a.CCCD == model.CCCD);

            var listAppt = await query.OrderByDescending(a => a.AppointmentDate).ToListAsync();
            listAppt = listAppt.Where(a => a.DOB.HasValue && a.DOB.Value.Date == model.DOB.Value.Date).ToList();

            if (listAppt.Count == 0)
            {
                return Json(new { success = false, message = "Không tìm thấy lịch hẹn nào." });
            }

            // Map dữ liệu sang ViewModel
            var resultList = listAppt.Select(item => new AppointmentDetailVM
            {
                AppointmentID = item.AppointmentID,
                MaKCB = item.AppointmentCode,
                HoTen = item.FullName,
                NgaySinh = item.DOB.HasValue ? item.DOB.Value.ToString("dd/MM/yyyy") : "",
                DoiTuong = !string.IsNullOrEmpty(item.AppointmentCode) && item.AppointmentCode.StartsWith("TBHDV")
               ? "Khám dịch vụ"
               : "Khám thường",
                NgayKham = item.AppointmentDate.HasValue ? item.AppointmentDate.Value.ToString("dd/MM/yyyy") : "",
                GioKham = item.TimeSlot,
                ChuyenKhoa = item.DepartmentID.HasValue ? db.Departments.Find(item.DepartmentID)?.Name : "Khoa Khám Bệnh",
                BacSi = item.Doctor != null ? item.Doctor.FullName : "Đang sắp xếp",
                Status = item.Status,
                BarcodeData = item.AppointmentCode,
                TrieuChung = item.Symptoms
            }).ToList();

            // Lưu vào TempData để truyền sang trang Results
            TempData["SearchResults"] = resultList;

            return Json(new { success = true, redirectUrl = Url.Action("Results", "AppointmentManage") });
        }

        // 2. TRANG KẾT QUẢ (HIỂN THỊ LIST)
        [HttpGet]
        public ActionResult Results()
        {
            var model = TempData["SearchResults"] as List<AppointmentDetailVM>;
            if (model == null)
            {
                // Nếu F5 mất dữ liệu thì quay về trang tra cứu
                return RedirectToAction("Index");
            }
            TempData.Keep("SearchResults"); // Giữ lại data nếu user F5 1 lần
            return View(model);

            var ids = model.Select(x => x.AppointmentID).ToList();

            // Truy vấn trạng thái mới nhất từ Database
            var freshStatuses = db.Appointments
                                  .Where(a => ids.Contains(a.AppointmentID))
                                  .Select(a => new { a.AppointmentID, a.Status })
                                  .ToList();

            // Cập nhật lại Status vào model hiển thị
            foreach (var item in model)
            {
                var freshItem = freshStatuses.FirstOrDefault(x => x.AppointmentID == item.AppointmentID);
                if (freshItem != null)
                {
                    item.Status = freshItem.Status;
                }
            }
            // ================================================

            // Lưu ngược lại vào TempData để dùng cho lần reload sau (nếu có)
            TempData["SearchResults"] = model;

            return View(model);
        }


        [HttpGet]
        public async Task<ActionResult> RedirectToReschedule(int id)
        {
            var appt = await db.Appointments.FindAsync(id);
            if (appt == null) return HttpNotFound();

            // Kiểm tra trạng thái: Nếu đã hoàn thành hoặc hủy thì không cho đổi
            if (appt.Status == "Completed" || appt.Status == "Cancelled")
            {
                return RedirectToAction("Results"); // Hoặc trang báo lỗi
            }

            // LOGIC ĐỊNH TUYẾN DỰA VÀO MÃ
            // Cách 1: Dựa vào Prefix (TBHKT vs TBHDV)
            if (!string.IsNullOrEmpty(appt.AppointmentCode))
            {
                if (appt.AppointmentCode.StartsWith("TBHKT"))
                {
                    // Chuyển sang form Khám Thường
                    return RedirectToAction("RegisterStandardExam", "ServicePortal", new { rescheduleId = id });
                }
                else if (appt.AppointmentCode.StartsWith("TBHDV"))
                {
                    // Chuyển sang form Khám Dịch Vụ
                    return RedirectToAction("RegisterServiceExam", "ServicePortal", new { rescheduleId = id });
                }
            }

            // Fallback: Nếu không nhận diện được mã, dựa vào Type hoặc Department
            if (appt.Type == "Dịch vụ")
            {
                return RedirectToAction("RegisterServiceExam", "ServicePortal", new { rescheduleId = id });
            }

            // Mặc định về khám thường
            return RedirectToAction("RegisterStandardExam", "ServicePortal", new { rescheduleId = id });
        }

        // POST: /Results/CancelAppointment
        [HttpPost]
        public async Task<JsonResult> CancelAppointment(int id)
        {
            try
            {
                var appt = await db.Appointments.FindAsync(id);
                if (appt == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch hẹn." });
                }

                if (appt.Status == "Completed" || appt.Status == "Cancelled")
                {
                    return Json(new { success = false, message = "Trạng thái lịch hẹn không thể hủy." });
                }

                // Cập nhật trạng thái
                appt.Status = "Cancelled";
                // appt.IsActive = false; // Nếu có cột này
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Đã hủy lịch hẹn thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


    }
}