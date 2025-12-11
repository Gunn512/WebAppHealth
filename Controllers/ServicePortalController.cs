using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

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

        // 2 =================== Đăng ký khám thường ======================= 
        // GET: Hiển thị form đăng ký khám thường
        public ActionResult RegisterStandardExam()
        {
            // Lấy danh sách khoa để đổ vào dropdown
            ViewBag.Departments = new SelectList(db.Departments.Where(d => d.Name.Contains("Khoa")).ToList(), "DepartmentID", "Name");
            return View();
        }


        // Xử lý lưu dữ liệu (Đã chỉnh sửa để hỗ trợ Đổi lịch)
        [HttpPost]
        public JsonResult RegisterStandardExam(RegisterExamViewModel model, int? OldAppointmentId)
        {
            try
            {
                // Validate cơ bản server-side
                if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc." });
                }

                Appointment appt;
                bool isUpdate = false;

                // --- 1. KIỂM TRA: LÀ CẬP NHẬT HAY TẠO MỚI? ---
                if (OldAppointmentId.HasValue && OldAppointmentId.Value > 0)
                {
                    // --- TRƯỜNG HỢP CẬP NHẬT ---
                    appt = db.Appointments.Find(OldAppointmentId.Value);
                    if (appt == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy lịch hẹn cũ để cập nhật." });
                    }

                    // Kiểm tra quyền (VD: chỉ cho sửa khi đang Pending/Confirmed, không cho sửa khi đã khám xong/hủy)
                    if (appt.Status == "Completed" || appt.Status == "Cancelled")
                    {
                        return Json(new { success = false, message = "Lịch hẹn này không thể thay đổi được nữa." });
                    }

                    isUpdate = true;
                    // *QUAN TRỌNG*: Không tạo AppointmentCode mới, giữ nguyên mã cũ để bảo lưu thanh toán
                }
                else
                {
                    // --- TRƯỜNG HỢP TẠO MỚI (Logic cũ) ---
                    appt = new Appointment();
                    appt.CreatedDate = DateTime.Now;
                    appt.AppointmentCode = "TBHKT-" + DateTime.Now.ToString("yyyyMMddHHmmss"); // Mã mới
                    appt.Type = "Khám thường";
                    appt.Status = "Pending";
                    appt.IsConfirm = false;
                    db.Appointments.Add(appt); // Chỉ Add khi tạo mới
                }

                // --- 2. CẬP NHẬT THÔNG TIN (DÙNG CHUNG CHO CẢ 2) ---
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

                // 3. XỬ LÝ CHUỖI LỊCH HẸN: "Thứ 2_17/11_07:00-08:00"
                if (!string.IsNullOrEmpty(model.LichHen))
                {
                    var parts = model.LichHen.Split('_');
                    if (parts.Length >= 3)
                    {
                        string datePart = parts[1]; // "17/11"
                        string timeSlot = parts[2]; // "07:00-08:00"

                        // Parse ngày (Thêm năm hiện tại vào)
                        string fullDateStr = $"{datePart}/{DateTime.Now.Year}";
                        DateTime parsedDate;
                        // Thử parse theo định dạng dd/MM/yyyy
                        if (DateTime.TryParseExact(fullDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                        {
                            // Logic xử lý qua năm mới (nếu cần): Ví dụ đang là tháng 12, đặt lịch tháng 1 năm sau
                            if (parsedDate < DateTime.Now.Date.AddDays(-1))
                            {
                                parsedDate = parsedDate.AddYears(1);
                            }
                            appt.AppointmentDate = parsedDate;
                        }
                        appt.TimeSlot = timeSlot;
                    }
                }

                // 4. Nếu người dùng đang đăng nhập, gán PatientID (nếu tìm thấy)
                if (User.Identity.IsAuthenticated && !isUpdate) // Chỉ gán khi tạo mới, hoặc cập nhật nếu chưa có
                {
                    var userEmail = User.Identity.Name;
                    var patient = db.Patients.FirstOrDefault(p => p.User.Email == userEmail);
                    if (patient != null && appt.PatientID == null)
                    {
                        appt.PatientID = patient.PatientID;
                    }
                }

                // Nếu là cập nhật, báo cho EF biết đối tượng đã thay đổi
                if (isUpdate)
                {
                    db.Entry(appt).State = EntityState.Modified;
                }

                // 5. Lưu vào DB
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = isUpdate ? "Cập nhật lịch hẹn thành công!" : "Đăng ký thành công!",
                    bookingCode = appt.AppointmentCode // Trả về mã (cũ hoặc mới)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }





        // ====================== 3. ĐĂNG KÝ KHÁM DỊCH VỤ =======================

        // ============================================================
        // API 1: LẤY BÁC  SĨ THEO CHUYÊN KHOA
        // ============================================================

        [HttpGet] 
        public JsonResult GetDoctorsByDepartment(int departmentId)
        {
            db.Configuration.ProxyCreationEnabled = false; // Tránh lỗi vòng lặp JSON
            var doctors = db.Doctors
                            .Where(d => d.DepartmentID == departmentId)
                            .Select(d => new { DoctorID = d.DoctorID, FullName = d.FullName })
                            .ToList();

            return Json(doctors, JsonRequestBehavior.AllowGet); // AllowGet là bắt buộc cho GET request
        }

        // ============================================================
        // API 2: LẤY LỊCH KHÁM
        // ============================================================
        [HttpGet]
        public JsonResult GetDoctorSchedule(int doctorId, string startDate)
        {
            try
            {
                // 1. Chuyển đổi chuỗi ngày từ client (yyyy-MM-dd) sang DateTime
                DateTime start;
                if (!DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
                {
                    return Json(new { success = false, message = "Định dạng ngày không hợp lệ." }, JsonRequestBehavior.AllowGet);
                }

                DateTime end = start.AddDays(6);

                db.Configuration.ProxyCreationEnabled = false;

                // 2. Truy vấn lịch "Available" trong khoảng thời gian
                // Lưu ý: Join bảng Schedules và TimeSlots
                var query = from s in db.Schedules
                            join t in db.TimeSlots on s.SlotID equals t.SlotID
                            where s.DoctorID == doctorId
                                  && s.DateWork >= start
                                  && s.DateWork <= end
                                  && s.Status == "Available" // Chỉ lấy lịch còn trống
                            select new
                            {
                                Date = s.DateWork,
                                Start = t.StartTime,
                                End = t.EndTime,
                                SlotID = t.SlotID
                            };

                var rawData = query.ToList();

                // 3. Format dữ liệu trả về cho Client
                var result = rawData.Select(x => new
                {
                    // Trả về dạng yyyy-MM-dd để Javascript so sánh
                    DateISO = x.Date.HasValue ? x.Date.Value.ToString("yyyy-MM-dd") : "",
                    // Tạo label giờ: 07:00 - 08:00
                    SlotLabel = string.Format("{0:hh\\:mm} - {1:hh\\:mm}", x.Start, x.End),
                    SlotID = x.SlotID
                }).ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi tại đây nếu cần
                return Json(new { success = false, message = "Lỗi server: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Hiển thị form đăng ký khám dịch vụ
        [HttpGet]
        public ActionResult RegisterServiceExam(int? rescheduleId)
        {
            // 1. Load danh sách Khoa
            ViewBag.Departments = new SelectList(db.Departments.Where(d => d.Name.Contains("Khoa")).ToList(), "DepartmentID", "Name");

            var model = new RegisterServiceViewModel();

            // 2. LOGIC PRE-FILL (Nếu là Đổi lịch)
            if (rescheduleId.HasValue)
            {
                var oldAppt = db.Appointments.Find(rescheduleId);
                if (oldAppt != null)
                {
                    // Map dữ liệu cũ
                    model.FullName = oldAppt.FullName;
                    model.Phone = oldAppt.Phone;
                    model.CCCD = oldAppt.CCCD;
                    model.DOB = oldAppt.DOB;
                    model.Gender = oldAppt.Gender;
                    model.Address = oldAppt.Address;
                    model.Symptoms = oldAppt.Symptoms;
                    model.DepartmentID = oldAppt.DepartmentID ?? 0;
                    model.DoctorID = oldAppt.DoctorID ?? 0;

                    // Quan trọng: Load danh sách Bác sĩ tương ứng với Khoa cũ để Dropdown không bị rỗng
                    if (oldAppt.DepartmentID.HasValue)
                    {
                        var doctors = db.Doctors.Where(d => d.DepartmentID == oldAppt.DepartmentID).ToList();
                        ViewBag.Doctors = new SelectList(doctors, "DoctorID", "FullName", oldAppt.DoctorID);
                    }

                    // Truyền ID và cờ hiệu sang View
                    ViewBag.RescheduleId = rescheduleId;
                    ViewBag.IsReschedule = true;
                }
            }

            return View(model);
        }

        // POST: Xử lý lưu dữ liệu (Tạo mới hoặc Cập nhật)
        [HttpPost]
        public JsonResult RegisterServiceExam(RegisterServiceViewModel model, int? OldAppointmentId)
        {
            try
            {
                // Validate cơ bản
                if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc." });
                }

                Appointment appt;
                bool isUpdate = false;

                // --- 1. KIỂM TRA: LÀ CẬP NHẬT HAY TẠO MỚI? ---
                if (OldAppointmentId.HasValue && OldAppointmentId.Value > 0)
                {
                    // --- UPDATE ---
                    appt = db.Appointments.Find(OldAppointmentId.Value);
                    if (appt == null) return Json(new { success = false, message = "Không tìm thấy lịch hẹn cũ." });

                    if (appt.Status == "Completed" || appt.Status == "Cancelled")
                        return Json(new { success = false, message = "Lịch hẹn này không thể thay đổi được nữa." });

                    isUpdate = true;
                    // Giữ nguyên AppointmentCode, CreatedDate
                }
                else
                {
                    // --- CREATE ---
                    appt = new Appointment();
                    appt.CreatedDate = DateTime.Now;
                    appt.AppointmentCode = "TBHDV-" + DateTime.Now.ToString("yyyyMMddHHmmss"); // Mã Dịch Vụ
                    appt.Type = "Dịch vụ";
                    appt.Status = "Pending";
                    appt.IsConfirm = false;
                    db.Appointments.Add(appt);
                }

                // --- 2. MAP DỮ LIỆU ---
                appt.FullName = model.FullName;
                appt.DOB = model.DOB;
                appt.Gender = model.Gender;
                appt.CCCD = model.CCCD;
                appt.Phone = model.Phone;
                appt.Address = model.Address;

                // Khám dịch vụ mặc định không dùng BHYT (hoặc tùy logic của bạn)
                appt.HasHealthInsurance = false;

                appt.DepartmentID = model.DepartmentID;
                appt.DoctorID = model.DoctorID; // Cập nhật Bác sĩ
                appt.Symptoms = model.Symptoms;

                // --- 3. XỬ LÝ LỊCH HẸN (NGÀY & GIỜ) ---
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

                // --- 4. GÁN PATIENT ID (Nếu chưa có) ---
                if (User.Identity.IsAuthenticated && !isUpdate)
                {
                    var userEmail = User.Identity.Name;
                    var patient = db.Patients.FirstOrDefault(p => p.User.Email == userEmail);
                    if (patient != null) appt.PatientID = patient.PatientID;
                }

                // Đánh dấu update
                if (isUpdate) db.Entry(appt).State = EntityState.Modified;

                // --- 5. LƯU ---
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = isUpdate ? "Cập nhật lịch khám dịch vụ thành công!" : "Đăng ký khám dịch vụ thành công!",
                    bookingCode = appt.AppointmentCode
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }




        // ================== 3. Tra cứu lịch hẹn =======================
        public ActionResult LookupAppointment()
        {
            return View();
        }

        // 4. Tra cứu kết quả xét nghiệm
        [Authorize]
        public ActionResult LookupExamResults()
        {
            return View();
        }

        // 5. Hóa đơn
        [Authorize]
        public ActionResult LookupInvoice()
        {
            return View();
        }

        // Trang hồ sơ cá nhân
        [Authorize]
        public ActionResult UserProfile()
        {
            return View("Profile");
        }
    }
}