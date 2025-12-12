//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Mvc;
//using WebAppHealth.Models.DataModel;
//using WebAppHealth.Models.ViewModels;

//namespace WebAppHealth.Controllers
//{
//    [Authorize] // Bắt buộc đăng nhập
//    public class InvoiceController : Controller
//    {
//        private HealthDBContext db = new HealthDBContext();

//        // 1. Màn hình danh sách hóa đơn
//        public ActionResult Index()
//        {
//            var userEmail = User.Identity.Name;
//            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
//            if (user == null) return RedirectToAction("Login", "Account");

//            var patient = db.Patients.FirstOrDefault(p => p.UserID == user.UserID);
//            if (patient == null)
//            {
//                ViewBag.Message = "Không tìm thấy thông tin bệnh nhân.";
//                return View(new List<InvoiceHistoryItem>()); // Trả về list rỗng tránh lỗi null
//            }

//            // --- BƯỚC 1: TRUY VẤN DỮ LIỆU THÔ (RAW DATA) ---
//            var rawData = (from inv in db.Invoices
//                           join app in db.Appointments on inv.AppointmentID equals app.AppointmentID
//                           where app.PatientID == patient.PatientID
//                           orderby inv.CreatedDate descending
//                           select new
//                           {
//                               inv.InvoiceID,
//                               inv.InvoiceCode,
//                               CreatedDate = inv.CreatedDate,
//                               Note = inv.Note,
//                               TotalAmount = inv.TotalAmount,
//                               PaymentStatus = inv.PaymentStatus
//                           }).ToList();

//            // --- BƯỚC 2: XỬ LÝ FORMAT TRÊN RAM (MEMORY) ---
//            var list = rawData.Select(item => new InvoiceHistoryItem
//            {
//                InvoiceID = item.InvoiceID,
//                InvoiceCode = item.InvoiceCode,

//                // Format ngày tháng tại đây
//                CreatedDate = item.CreatedDate.HasValue ? item.CreatedDate.Value.ToString("dd/MM/yyyy") : "",

//                Content = string.IsNullOrEmpty(item.Note) ? "Thanh toán viện phí & dịch vụ" : item.Note,

//                TotalAmount = item.TotalAmount ?? 0,

//                // Format tiền tệ tại đây luôn cho gọn
//                TotalAmountStr = (item.TotalAmount ?? 0).ToString("#,##0") + " VNĐ",

//                Status = item.PaymentStatus ?? "Đã thanh toán"
//            }).ToList();

//            return View(list);
//        }

//        // 2. API Lấy chi tiết hóa đơn (Gọi qua AJAX)
//        [HttpGet]
//        public JsonResult GetDetail(int invoiceId)
//        {
//            // A. Lấy thông tin Header
//            var invoice = db.Invoices.FirstOrDefault(i => i.InvoiceID == invoiceId);
//            if (invoice == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn" }, JsonRequestBehavior.AllowGet);

//            // Lấy thông tin Bệnh nhân (để in tên lên hóa đơn)
//            var appointment = db.Appointments.FirstOrDefault(a => a.AppointmentID == invoice.AppointmentID);
//            var patient = db.Patients.FirstOrDefault(p => p.PatientID == appointment.PatientID);

//            // B. Lấy chi tiết (QUERY TRỰC TIẾP TỪ BẢNG MỚI - Rất đơn giản)
//            var dbItems = db.InvoiceDetails.Where(d => d.InvoiceID == invoiceId).ToList();

//            var listItems = new List<InvoiceLineItem>();
//            int stt = 1;
//            foreach (var item in dbItems)
//            {
//                listItems.Add(new InvoiceLineItem
//                {
//                    Stt = stt++,
//                    Name = item.ItemName,     // Tên đã lưu cứng
//                    Unit = item.Unit,         // Đơn vị đã lưu cứng
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    TotalPrice = item.TotalPrice
//                });
//            }

//            // C. Chuẩn bị dữ liệu trả về
//            // Xử lý ngày tháng hiển thị dạng chữ
//            DateTime date = invoice.CreatedDate ?? DateTime.Now;
//            string dateStr = $"Ngày {date.Day} tháng {date.Month} năm {date.Year}";

//            var result = new InvoiceDetailVM
//            {
//                InvoiceCode = invoice.InvoiceCode,
//                CreatedDateString = dateStr,
//                PatientName = patient?.FullName?.ToUpper() ?? "KHÁCH VÃNG LAI",
//                Address = patient?.Address ?? "Chưa cập nhật",
//                PaymentMethod = invoice.PaymentMethod ?? "Tiền mặt",
//                Note = invoice.Note,

//                // Lấy các số tổng từ bảng Invoice (vì bảng này đã được tính toán lúc tạo)
//                SubTotal = invoice.SubTotal ?? invoice.TotalAmount ?? 0,
//                VAT = invoice.TaxAmount ?? 0,
//                GrandTotal = invoice.TotalAmount ?? 0,

//                Items = listItems
//            };

//            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebAppHealth.Models.DataModel;
using WebAppHealth.Models.ViewModels;

namespace WebAppHealth.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private HealthDBContext db = new HealthDBContext();

        public ActionResult Index()
        {
            var userEmail = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return RedirectToAction("Login", "Account");

            var patient = db.Patients.FirstOrDefault(p => p.UserID == user.UserID);
            if (patient == null) return View(new List<InvoiceHistoryItem>());

            // --- BƯỚC 1: Lấy dữ liệu THÔ (Không dùng ToString ở đây) ---
            var rawData = (from inv in db.Invoices
                           join app in db.Appointments on inv.AppointmentID equals app.AppointmentID
                           where app.PatientID == patient.PatientID
                           orderby inv.CreatedDate descending
                           select new
                           {
                               inv.InvoiceID,
                               inv.InvoiceCode,
                               inv.CreatedDate,
                               inv.Note,
                               inv.TotalAmount
                           }).ToList(); // Lấy về RAM ngay lập tức

            // --- BƯỚC 2: Xử lý hiển thị trên RAM (Dùng ToString thoải mái) ---
            var list = rawData.Select(item => new InvoiceHistoryItem
            {
                InvoiceID = item.InvoiceID,
                InvoiceCode = item.InvoiceCode,

                // Format ngày tháng
                CreatedDate = item.CreatedDate.HasValue ? item.CreatedDate.Value.ToString("dd/MM/yyyy") : "",

                // Nội dung
                Content = string.IsNullOrEmpty(item.Note) ? "Viện phí ngoại trú" : item.Note,

                // Format tiền tệ sẵn sàng để hiển thị
                TotalAmountStr = (item.TotalAmount ?? 0).ToString("#,##0") + " VNĐ"
            }).ToList();

            return View(list);
        }

        [HttpGet]
        public JsonResult GetDetail(int invoiceId)
        {
            var invoice = db.Invoices.FirstOrDefault(i => i.InvoiceID == invoiceId);
            if (invoice == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn" }, JsonRequestBehavior.AllowGet);

            var appointment = db.Appointments.FirstOrDefault(a => a.AppointmentID == invoice.AppointmentID);
            var patient = db.Patients.FirstOrDefault(p => p.PatientID == appointment.PatientID);

            var dbItems = db.InvoiceDetails.Where(d => d.InvoiceID == invoiceId).ToList();
            var listItems = new List<InvoiceLineItem>();
            int stt = 1;
            foreach (var item in dbItems)
            {
                listItems.Add(new InvoiceLineItem
                {
                    Stt = stt++,
                    Name = item.ItemName,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                });
            }

            DateTime date = invoice.CreatedDate ?? DateTime.Now;

            var result = new InvoiceDetailVM
            {
                InvoiceCode = invoice.InvoiceCode,
                CreatedDateString = $"Ngày {date.Day} tháng {date.Month} năm {date.Year}",
                PatientName = patient?.FullName?.ToUpper(),
                PhoneNumber = !string.IsNullOrEmpty(appointment.Phone) ? appointment.Phone : "-",
                Address = patient?.Address ?? "-",
                PaymentMethod = invoice.PaymentMethod ?? "Tiền mặt",
                SubTotal = invoice.SubTotal ?? invoice.TotalAmount ?? 0,
                VAT = invoice.TaxAmount ?? 0,
                GrandTotal = invoice.TotalAmount ?? 0,
                Items = listItems
            };

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }
    }
}