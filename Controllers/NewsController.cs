using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class NewsController : Controller
    {
        private HealthDBContext db = new HealthDBContext();
        // Y học
        public ActionResult MedicalNews(int page = 1)
        {
            var viewModel = new NewsViewModel();
            var pageSize = 4; // Số bài viết trên mỗi trang

            var dbQuery = db.News.AsQueryable();

            var medicalNews = dbQuery
                              .Where(n => n.CategoryID == 2)
                              .OrderByDescending(n => n.PublishDate)
                              .ToList();


            viewModel.FeaturedArticle = medicalNews.FirstOrDefault();

            var remainingMedicalNews = medicalNews.Skip(1).ToList();

            viewModel.TotalPages = (int)Math.Ceiling((double)remainingMedicalNews.Count / pageSize);
            viewModel.CurrentPage = page;

            viewModel.GeneralNewsList = remainingMedicalNews
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            viewModel.SidebarNewsList = dbQuery
                                        .OrderByDescending(n => n.PublishDate)
                                        .Take(5)
                                        .ToList();

            return View(viewModel);
        }

        // Trang sự kiện
        public ActionResult Events(int page = 1)
        {
            var viewModel = new NewsViewModel();
            var pageSize = 4; // Số bài viết trên mỗi trang

            var dbQuery = db.News.AsQueryable();

            var medicalNews = dbQuery
                              .Where(n => n.CategoryID == 3)
                              .OrderByDescending(n => n.PublishDate)
                              .ToList();


            viewModel.FeaturedArticle = medicalNews.FirstOrDefault();

            var remainingMedicalNews = medicalNews.Skip(1).ToList();

            viewModel.TotalPages = (int)Math.Ceiling((double)remainingMedicalNews.Count / pageSize);
            viewModel.CurrentPage = page;

            viewModel.GeneralNewsList = remainingMedicalNews
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            viewModel.SidebarNewsList = dbQuery
                                        .OrderByDescending(n => n.PublishDate)
                                        .Take(5)
                                        .ToList();

            return View(viewModel);
        }

        // Trang thông tin dược
        public ActionResult PharmacyInfo(string keyword = "", int page = 1)
        {
            int pageSize = 5; // Số thuốc mỗi trang

            // 1. Lấy Query ban đầu
            var query = db.Medicines.AsQueryable();

            // 2. Xử lý Tìm kiếm (nếu có keyword)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(m => m.Name.Contains(keyword));
            }

            // 3. Xử lý Phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy dữ liệu trang hiện tại (Sắp xếp theo tên)
            var medicines = query.OrderBy(m => m.Name)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

            // 4. Đóng gói vào ViewModel
            var viewModel = new MedicineViewModel
            {
                Medicines = medicines,
                SearchKeyword = keyword,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // Trang chi tiết hướng dẫn sử dụng thuốc
        public ActionResult ViewInstruction(int id)
        {
            var medicine = db.Medicines.Find(id);
            if (medicine == null || string.IsNullOrEmpty(medicine.InstructionFile))
            {
                return HttpNotFound("Không tìm thấy tài liệu cho thuốc này.");
            }

            return View(medicine);
        }

        // Trang cải cách hành chính
        public ActionResult AdministrativeReform(int page = 1)
        {
            var viewModel = new NewsViewModel();
            var pageSize = 4; // Số bài viết trên mỗi trang

            var dbQuery = db.News.AsQueryable();

            var medicalNews = dbQuery
                              .Where(n => n.CategoryID == 4)
                              .OrderByDescending(n => n.PublishDate)
                              .ToList();


            viewModel.FeaturedArticle = medicalNews.FirstOrDefault();

            var remainingMedicalNews = medicalNews.Skip(1).ToList();

            viewModel.TotalPages = (int)Math.Ceiling((double)remainingMedicalNews.Count / pageSize);
            viewModel.CurrentPage = page;

            viewModel.GeneralNewsList = remainingMedicalNews
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            viewModel.SidebarNewsList = dbQuery
                                        .OrderByDescending(n => n.PublishDate)
                                        .Take(5)
                                        .ToList();

            return View(viewModel);
        }

        // Hàm chi tiết bài viết
        public ActionResult ArticleDetail(int id)
        {
            var article = db.News.Find(id);

            if (article == null)
            {
                return HttpNotFound();
            }

            ViewBag.SidebarNews = db.News
                                    .OrderByDescending(n => n.PublishDate)
                                    .Take(5)
                                    .ToList();

            return View(article);
        }

        public ActionResult DownloadInstruction(int id)
        {
            // 1. Tìm thông tin thuốc trong DB
            var medicine = db.Medicines.Find(id);

            if (medicine == null || string.IsNullOrEmpty(medicine.InstructionFile))
            {
                return HttpNotFound("Không tìm thấy thông tin tài liệu.");
            }

            string filePath = Server.MapPath("~/asset/" + medicine.InstructionFile);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("File tài liệu gốc không tồn tại trên máy chủ.");
            }

            // 4. Xử lý tên file
            string cleanName = medicine.Name.Replace(" ", "_").Replace("/", "-");
            string downloadName = $"HuongDan_{cleanName}.pdf";

            return File(filePath, "application/pdf", downloadName);
        }

        // Hàm Dispose để giải phóng kết nôi CSDL sau khi xong việc 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}