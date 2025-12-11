//using System;
//using System.Data.Entity;
//using System.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;
using System.Data.Entity;

namespace WebAppHealth.Controllers
{
    public class ServicePortalController : Controller
    {
        private HealthDBContext db = new HealthDBContext();

        // ====================== 1. Bảng Patient Dashboard =======================
        public ActionResult PatientDashboard()
        {
            return View();
        }

        // ====================== 2. ĐĂNG KÝ KHÁM THƯỜNG ======================= 

        // GET: Hiển thị form đăng ký (ĐÃ SỬA: Thêm logic lấy dữ liệu cũ)
        [HttpGet]
        public ActionResult RegisterStandardExam(int? rescheduleId)
        {
            // 1. Load danh sách khoa
            ViewBag.Departments = new SelectList(db.Departments.Where(d => d.Name.Contains("Khoa")).ToList(), "DepartmentID", "Name");

            var model = new RegisterExamViewModel();

            // 2. LOGIC ĐỔI LỊCH (Nếu có ID truyền vào)
            if (rescheduleId.HasValue)
            {
                var oldAppt = db.Appointments.Find(rescheduleId);
                if (oldAppt != null)
                {
                    // Map dữ liệu từ DB sang ViewModel để hiển thị lên form
                    model.FullName = oldAppt.FullName;
                    model.Phone = oldAppt.Phone;
                    model.CCCD = oldAppt.CCCD;
                    model.DOB = oldAppt.DOB;
                    model.Gender = oldAppt.Gender;
                    model.Address = oldAppt.Address;

                    // Xử lý BHYT (Trong DB là bool, ViewModel là string "1"/"0")
                    model.HasHealthInsurance = (oldAppt.HasHealthInsurance == true) ? "1" : "0";
                    model.HealthInsuranceNumber = oldAppt.HealthInsuranceNumber;

                    model.DepartmentID = oldAppt.DepartmentID ?? 0;
                    model.Symptoms = oldAppt.Symptoms;

                    // Truyền ID cũ sang View
                    ViewBag.RescheduleId = rescheduleId;
                    ViewBag.IsReschedule = true;
                }
            }

            return View(model);
        }

        // POST: Xử lý lưu dữ liệu (Tạo mới hoặc Cập nhật)
        [HttpPost]
        public JsonResult RegisterStandardExam(RegisterExamViewModel model, int? OldAppointmentId)
        {
            try
            {
                if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc." });
                }

                Appointment appt;
                bool isUpdate = false;

                // --- KIỂM TRA: CẬP NHẬT HAY TẠO MỚI? ---
                if (OldAppointmentId.HasValue && OldAppointmentId.Value > 0)
                {
                    // --- UPDATE ---
                    appt = db.Appointments.Find(OldAppointmentId.Value);
                    if (appt == null) return Json(new { success = false, message = "Không tìm thấy lịch hẹn cũ." });

                    if (appt.Status == "Completed" || appt.Status == "Cancelled")
                        return Json(new { success = false, message = "Lịch hẹn này không thể thay đổi được nữa." });

                    isUpdate = true;
                }
                else
                {
                    // --- CREATE ---
                    appt = new Appointment();
                    appt.CreatedDate = DateTime.Now;
                    appt.AppointmentCode = "TBHKT-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    appt.Type = "Khám thường";
                    appt.Status = "Pending";
                    appt.IsConfirm = false;
                    db.Appointments.Add(appt);
                }

                // --- MAP DỮ LIỆU ---
                appt.FullName = model.FullName;
                appt.DOB = model.DOB;
                appt.Gender = model.Gender;
                appt.CCCD = model.CCCD;
                appt.Phone = model.Phone;
                appt.Address = model.Address;
                appt.HasHealthInsurance = (model.HasHealthInsurance == "1");
                appt.HealthInsuranceNumber = (model.HasHealthInsurance == "1") ? model.HealthInsuranceNumber : null;
                appt.DepartmentID = model.DepartmentID;
                appt.Symptoms = model.Symptoms;

                // Xử lý Lịch hẹn (Ngày & Giờ)
                if (!string.IsNullOrEmpty(model.LichHen))
                {
                    var parts = model.LichHen.Split('_');
                    if (parts.Length >= 3)
                    {
                        string datePart = parts[1]; // "17/11"
                        string timeSlot = parts[2]; // "07:00-08:00"
                        string fullDateStr = $"{datePart}/{DateTime.Now.Year}";
                        DateTime parsedDate;

                        if (DateTime.TryParseExact(fullDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                        {
                            if (parsedDate < DateTime.Now.Date.AddDays(-1)) parsedDate = parsedDate.AddYears(1);
                            appt.AppointmentDate = parsedDate;
                        }
                        appt.TimeSlot = timeSlot;
                    }
                }

                // Gán PatientID nếu đã đăng nhập và chưa có
                if (User.Identity.IsAuthenticated && !isUpdate)
                {
                    var userEmail = User.Identity.Name;
                    var patient = db.Patients.FirstOrDefault(p => p.User.Email == userEmail);
                    if (patient != null) appt.PatientID = patient.PatientID;
                }

                if (isUpdate) db.Entry(appt).State = EntityState.Modified;

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = isUpdate ? "Cập nhật lịch hẹn thành công!" : "Đăng ký thành công!",
                    bookingCode = appt.AppointmentCode,
                    isUpdate = isUpdate
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ====================== 3. ĐĂNG KÝ KHÁM DỊCH VỤ =======================

        // GET: Hiển thị form đăng ký dịch vụ (ĐÃ SỬA: Load bác sĩ cũ)
        [HttpGet]
        public ActionResult RegisterServiceExam(int? rescheduleId)
        {
            ViewBag.Departments = new SelectList(db.Departments.Where(d => d.Name.Contains("Khoa")).ToList(), "DepartmentID", "Name");

            var model = new RegisterServiceViewModel();

            // LOGIC ĐỔI LỊCH DỊCH VỤ
            if (rescheduleId.HasValue)
            {
                var oldAppt = db.Appointments.Find(rescheduleId);
                if (oldAppt != null)
                {
                    model.FullName = oldAppt.FullName;
                    model.Phone = oldAppt.Phone;
                    model.CCCD = oldAppt.CCCD;
                    model.DOB = oldAppt.DOB;
                    model.Gender = oldAppt.Gender;
                    model.Address = oldAppt.Address;
                    model.Symptoms = oldAppt.Symptoms;
                    model.DepartmentID = oldAppt.DepartmentID ?? 0;
                    model.DoctorID = oldAppt.DoctorID ?? 0;

                    // Load danh sách Bác sĩ của khoa cũ để dropdown hiển thị đúng
                    if (oldAppt.DepartmentID.HasValue)
                    {
                        var doctors = db.Doctors.Where(d => d.DepartmentID == oldAppt.DepartmentID).ToList();
                        ViewBag.Doctors = new SelectList(doctors, "DoctorID", "FullName", oldAppt.DoctorID);
                    }

                    ViewBag.RescheduleId = rescheduleId;
                    ViewBag.IsReschedule = true;
                }
            }

            return View(model);
        }

        // POST: Lưu đăng ký dịch vụ (ĐÃ SỬA: Thêm logic Update)
        [HttpPost]
        public JsonResult RegisterServiceExam(RegisterServiceViewModel model, int? OldAppointmentId)
        {
            try
            {
                if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc." });
                }

                Appointment appt;
                bool isUpdate = false;

                // --- KIỂM TRA UPDATE ---
                if (OldAppointmentId.HasValue && OldAppointmentId.Value > 0)
                {
                    appt = db.Appointments.Find(OldAppointmentId.Value);
                    if (appt == null) return Json(new { success = false, message = "Không tìm thấy lịch hẹn cũ." });

                    if (appt.Status == "Completed" || appt.Status == "Cancelled")
                        return Json(new { success = false, message = "Lịch hẹn này không thể thay đổi." });

                    isUpdate = true;
                }
                else
                {
                    // --- CREATE ---
                    appt = new Appointment();
                    appt.CreatedDate = DateTime.Now;
                    appt.AppointmentCode = "TBHDV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    appt.Type = "Dịch vụ";
                    appt.Status = "Pending";
                    appt.IsConfirm = false;
                    db.Appointments.Add(appt);
                }

                // --- MAP DỮ LIỆU ---
                appt.FullName = model.FullName;
                appt.DOB = model.DOB;
                appt.Gender = model.Gender;
                appt.CCCD = model.CCCD;
                appt.Phone = model.Phone;
                appt.Address = model.Address;
                appt.HasHealthInsurance = false; // Dịch vụ ko BHYT
                appt.DepartmentID = model.DepartmentID;
                appt.DoctorID = model.DoctorID;
                appt.Symptoms = model.Symptoms;

                // Xử lý Lịch hẹn
                if (!string.IsNullOrEmpty(model.LichHen))
                {
                    var parts = model.LichHen.Split('_');
                    if (parts.Length >= 3)
                    {
                        string datePart = parts[1];
                        string timeSlot = parts[2];
                        string fullDateStr = $"{datePart}/{DateTime.Now.Year}";
                        DateTime parsedDate;
                        if (DateTime.TryParseExact(fullDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                        {
                            if (parsedDate < DateTime.Now.Date.AddDays(-1)) parsedDate = parsedDate.AddYears(1);
                            appt.AppointmentDate = parsedDate;
                        }
                        appt.TimeSlot = timeSlot;
                    }
                }

                if (User.Identity.IsAuthenticated && !isUpdate)
                {
                    var userEmail = User.Identity.Name;
                    var patient = db.Patients.FirstOrDefault(p => p.User.Email == userEmail);
                    if (patient != null) appt.PatientID = patient.PatientID;
                }

                if (isUpdate) db.Entry(appt).State = EntityState.Modified;

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = isUpdate ? "Cập nhật thành công!" : "Đăng ký thành công!",
                    bookingCode = appt.AppointmentCode,
                    isUpdate = isUpdate
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ====================== CÁC API HỖ TRỢ (QUAN TRỌNG) =======================

        [HttpGet]
        public JsonResult GetDoctorsByDepartment(int departmentId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var doctors = db.Doctors.Where(d => d.DepartmentID == departmentId)
                                    .Select(d => new { DoctorID = d.DoctorID, FullName = d.FullName })
                                    .ToList();
            return Json(doctors, JsonRequestBehavior.AllowGet);
        }

        // ============================================================
        // API 2: LẤY LỊCH KHÁM (Đã lọc slot Đã đặt/Chờ duyệt)
        // ============================================================
        [HttpGet]
        public JsonResult GetDoctorSchedule(int doctorId, string startDate)
        {
            try
            {
                DateTime start;
                if (!DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
                {
                    return Json(new { success = false, message = "Định dạng ngày không hợp lệ." }, JsonRequestBehavior.AllowGet);
                }
                DateTime end = start.AddDays(6);

                db.Configuration.ProxyCreationEnabled = false;

                // 1. Lấy lịch làm việc cơ bản của Bác sĩ (Bảng Schedules)
                var rawSchedules = (from s in db.Schedules
                                    join t in db.TimeSlots on s.SlotID equals t.SlotID
                                    where s.DoctorID == doctorId
                                          && s.DateWork >= start
                                          && s.DateWork <= end
                                          && s.Status == "Available"
                                    select new
                                    {
                                        Date = s.DateWork,
                                        Start = t.StartTime,
                                        End = t.EndTime,
                                        SlotID = t.SlotID
                                    }).ToList();

                // 2. Lấy danh sách các slot ĐÃ CÓ NGƯỜI ĐẶT (Pending, Confirmed, Completed)
                // Loại bỏ những cái đã Hủy (Cancelled)
                var bookedSlots = db.Appointments
                                    .Where(a => a.DoctorID == doctorId
                                             && a.AppointmentDate >= start
                                             && a.AppointmentDate <= end
                                             && a.Status != "Cancelled") // Quan trọng: Pending cũng coi là đã mất slot
                                    .Select(a => new
                                    {
                                        Date = a.AppointmentDate,
                                        Slot = a.TimeSlot
                                    })
                                    .ToList();

                // 3. Lọc và Format dữ liệu
                var result = new List<object>();

                foreach (var item in rawSchedules)
                {
                    string dateStr = item.Date.HasValue ? item.Date.Value.ToString("yyyy-MM-dd") : "";
                    string slotLabel = string.Format("{0:hh\\:mm} - {1:hh\\:mm}", item.Start, item.End);

                    // Kiểm tra xem slot này có nằm trong danh sách đã đặt không
                    bool isBooked = bookedSlots.Any(b =>
                        b.Date.HasValue &&
                        b.Date.Value.ToString("yyyy-MM-dd") == dateStr &&
                        b.Slot == slotLabel
                    );

                    // Nếu CHƯA đặt thì mới thêm vào danh sách trả về
                    if (!isBooked)
                    {
                        result.Add(new
                        {
                            DateISO = dateStr,
                            SlotLabel = slotLabel,
                            SlotID = item.SlotID
                        });
                    }
                }

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult UserProfile() { return View("Profile"); }
    }
}