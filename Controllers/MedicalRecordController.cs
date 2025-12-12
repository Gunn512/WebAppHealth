using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebAppHealth.Models.DataModel;
using WebAppHealth.Models.ViewModels;

namespace WebAppHealth.Controllers
{
    [Authorize]
    public class MedicalRecordController : Controller
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: MedicalRecord
        public ActionResult Index()
        {
            // 1. Lấy thông tin User đang đăng nhập
            var userEmail = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null) return RedirectToAction("Login", "Account");

            // 2. Lấy thông tin Bệnh nhân (Patient) từ User
            var patient = db.Patients.FirstOrDefault(p => p.UserID == user.UserID);

            if (patient == null)
            {
                ViewBag.Message = "Không tìm thấy hồ sơ bệnh nhân liên kết với tài khoản này.";
                return View(new List<MedicalHistoryItem>());
            }

            // 3. Truy vấn dữ liệu thô (Raw Data) từ Database trước
            var rawData = (from a in db.Appointments
                               // 1. ĐIỀU KIỆN; LÀ BỆNH NHÂN (CÓ PATIENDID) VÀ CÓ HỒ SƠ BỆNH ÁN (CÓ RECORDID)
                           join r in db.MedicalRecords on a.AppointmentID equals r.AppointmentID

                           join d in db.Doctors on r.DoctorID equals d.DoctorID into dGroup
                           from doc in dGroup.DefaultIfEmpty()

                               // Cố gắng lấy tên khoa
                           join dep in db.Departments on (doc != null ? doc.DepartmentID : 0) equals dep.DepartmentID into depGroup
                           from department in depGroup.DefaultIfEmpty()

                               // 3. ĐIỀU KIỆN LỌC DUY NHẤT: Của chính mình
                           where a.PatientID == patient.PatientID

                           orderby a.AppointmentDate descending
                           select new
                           {
                               a.AppointmentID,
                               r.RecordID,
                               AppointmentDate = a.AppointmentDate,
                               // Nếu tìm thấy bác sĩ thì hiện tên, không thì hiện mặc định
                               DoctorName = doc != null ? doc.FullName : "-",
                               DepartmentName = department != null ? department.Name : "Bệnh viện Đa Khoa",
                               HasHealthInsurance = a.HasHealthInsurance
                           }).ToList();

            // 4. Chuyển đổi sang ViewModel (Xử lý trên RAM)
            // Lúc này dữ liệu đã nằm trong List C#, nên bạn dùng ToString() thoải mái
            var history = rawData.Select(item => new MedicalHistoryItem
            {
                AppointmentID = item.AppointmentID,
                RecordID = item.RecordID,
                DateExam = item.AppointmentDate.HasValue ? item.AppointmentDate.Value.ToString("dd/MM/yyyy") : "",
                DepartmentName = item.DepartmentName,
                DoctorName = item.DoctorName,
                Type = item.HasHealthInsurance == true ? "Khám thường" : "Khám dịch vụ"
            }).ToList();

            return View(history);
        }

        // API Lấy chi tiết (Cập nhật logic lấy bác sĩ từ Record)
        [HttpGet]
        public JsonResult GetDetail(int recordId)
        {
            var record = db.MedicalRecords.FirstOrDefault(r => r.RecordID == recordId);
            if (record == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ" }, JsonRequestBehavior.AllowGet);

            var appointment = db.Appointments.FirstOrDefault(a => a.AppointmentID == record.AppointmentID);

            // SỬA: Lấy bác sĩ dựa trên RecordID trước, nếu null mới tìm về Appointment
            // (Ưu tiên người thực tế khám)
            var doctorId = record.DoctorID ?? appointment.DoctorID;
            var doctor = db.Doctors.FirstOrDefault(d => d.DoctorID == doctorId);
            var dept = db.Departments.FirstOrDefault(dep => dep.DepartmentID == doctor.DepartmentID);

            // Lấy danh sách thuốc (Giữ nguyên)
            var prescriptions = (from pd in db.PrescriptionDetails
                                 join m in db.Medicines on pd.MedicineID equals m.MedicineID
                                 where pd.RecordID == record.RecordID
                                 select new PrescriptionItem
                                 {
                                     MedicineName = m.Name + " (" + m.Concentration + ")",
                                     Quantity = pd.Quantity.ToString(),
                                     Unit = m.Unit,
                                     Usage = pd.UsageInstruction
                                 }).ToList();

            for (int i = 0; i < prescriptions.Count; i++) prescriptions[i].Stt = i + 1;

            var result = new MedicalRecordDetailVM
            {
                RecordID = record.RecordID,
                DateExam = appointment.AppointmentDate.HasValue ? appointment.AppointmentDate.Value.ToString("dd/MM/yyyy") : "",
                DepartmentName = dept?.Name,
                DoctorName = doctor?.FullName ?? "Chưa xác định",
                PatientCode = "HS" + record.RecordID.ToString("D6"),
                Diagnosis = record.Diagnosis,
                DoctorNotes = record.DoctorNotes,
                ReExamDate = "Theo chỉ định bác sĩ",
                Prescriptions = prescriptions,
                LabResults = new List<string>()
            };

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }
    }
}