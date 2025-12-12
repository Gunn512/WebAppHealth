using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class ContactUsController : Controller
    {

        private HealthDBContext db = new HealthDBContext();
        // Trang liên hệ
        public ActionResult Contact()
        {
            return View();
        }
        // Trang FAQ
        public ActionResult FAQ(int page = 1)
        {
            int pageSize = 5;

            // 1. Lấy danh sách câu hỏi ĐÃ DUYỆT và ĐÃ CÓ TRẢ LỜI
            var query = db.Faqs.Where(f => f.IsPublished == true && f.Answer != null)
                               .OrderByDescending(f => f.CreatedAt);

            // 2. Tính toán phân trang
            int totalRecords = query.Count();
            var listFaq = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 3. Đưa dữ liệu vào ViewModel
            var model = new FaqViewModel
            {
                ListFaq = listFaq,
                NewQuestion = new Faq(), // Tạo mới object rỗng cho form
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };

            return View(model);
        }

        // POST: Xử lý khi người dùng Gửi câu hỏi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitQuestion(FaqViewModel model)
        {
            // Kiểm tra tính hợp lệ (Nó sẽ tự check file Metadata bạn vừa tạo)
            if (ModelState.IsValid)
            {
                try
                {
                    var faq = model.NewQuestion;

                    // Gán các giá trị mặc định hệ thống
                    faq.IsPublished = false;       // Chưa duyệt
                    faq.Answer = null;             // Chưa trả lời
                    faq.CreatedAt = DateTime.Now;    // Ngày đặt câu hỏi

                    db.Faqs.Add(faq);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Gửi câu hỏi thành công! Chúng tôi sẽ sớm phản hồi.";
                    return RedirectToAction("FAQ");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            // --- NẾU LỖI (VALIDATION SAI) ---
            // Phải load lại danh sách câu hỏi cũ để trang web không bị trắng trơn bên trái
            int page = 1;
            int pageSize = 5;
            var query = db.Faqs.Where(f => f.IsPublished == true && f.Answer != null)
                               .OrderByDescending(f => f.CreatedAt);

            model.ListFaq = query.Take(pageSize).ToList();
            model.PageNumber = 1;
            model.TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize);

            return View("FAQ", model); // Trả về View kèm thông báo lỗi đỏ
        }
    }
}