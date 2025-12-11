using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class CareersController : Controller
    {
        private HealthDBContext db = new HealthDBContext();
        // Trang tuyển dụng
        public ActionResult JobOpenings()
        {
            var jobs = db.Recruitments
                         .Where(x => x.IsActive == true)
                         .OrderByDescending(x => x.CreatedDate)
                         .ToList();

            return View(jobs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }


        [HttpGet]
        public ActionResult JobForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JobForm(JobFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string cvPath = "";
                    string certPaths = "";

                    // --- 1. XỬ LÝ UPLOAD CV ---
                    if (model.CVFile != null && model.CVFile.ContentLength > 0)
                    {
                        // Đổi tên file để tránh trùng: GUID_TenFileGoc
                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CVFile.FileName);
                        string folderPath = Server.MapPath("~/Uploads/CVs/");

                        // Tạo thư mục nếu chưa có
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fullPath = Path.Combine(folderPath, fileName);
                        model.CVFile.SaveAs(fullPath);

                        // Lưu đường dẫn tương đối vào biến
                        cvPath = "/Uploads/CVs/" + fileName;
                    }

                    // --- 2. XỬ LÝ UPLOAD ẢNH BẰNG CẤP ---
                    if (model.CertificateImages != null && model.CertificateImages.Length > 0)
                    {
                        List<string> uploadedPaths = new List<string>();
                        string folderPath = Server.MapPath("~/Uploads/Certs/");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        foreach (var file in model.CertificateImages)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                                string fullPath = Path.Combine(folderPath, fileName);
                                file.SaveAs(fullPath);
                                uploadedPaths.Add("/Uploads/Certs/" + fileName);
                            }
                        }
                        // Nối các đường dẫn bằng dấu chấm phẩy
                        certPaths = string.Join(";", uploadedPaths);
                    }

                    // --- 3. LƯU VÀO DATABASE ---
                    Candidate ungVien = new Candidate();

                    // Map dữ liệu từ ViewModel -> Entity
                    ungVien.FullName = model.FullName;
                    ungVien.BirthDate = model.DateOfBirth;
                    ungVien.Gender = model.Gender;
                    ungVien.IdentityCard = model.IdCard;
                    ungVien.IssueDate = model.IssueDate;
                    ungVien.IssuePlace = model.IssuePlace;
                    ungVien.Hometown = model.Hometown;
                    ungVien.Phone = model.Phone;
                    ungVien.Address = model.Address;

                    ungVien.Qualification = model.Qualification;
                    ungVien.School = model.University;
                    ungVien.TrainingSystem = model.TrainingSystem;
                    ungVien.GraduationYear = model.GradYear;
                    ungVien.Rank = model.GradRank;

                    ungVien.LanguageCert = model.LanguageCert;
                    ungVien.InformaticsCert = model.InformaticsCert;
                    ungVien.Experience = model.Experience;

                    // Lưu đường dẫn file
                    ungVien.CvUrl = cvPath;
                    ungVien.CertUrls = certPaths;

                    // Các trường tự động
                    ungVien.CreatedDate = DateTime.Now;
                    ungVien.Status = 0; // 0 = Mới nộp

                    // Add và Save
                    db.Candidates.Add(ungVien);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Nộp hồ sơ thành công! Chúng tôi sẽ liên hệ sớm.";
                    return RedirectToAction("JobForm"); // Hoặc redirect về trang chủ
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu cần
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message);
                }
            }

            // Nếu model lỗi hoặc có exception, trả về view cũ để user nhập lại
            return View(model);
        }

    }
}