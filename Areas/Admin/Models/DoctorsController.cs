using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity; // Quan trọng để dùng .Include
using WebAppHealth.Models.DataModel;
using WebAppHealth.Areas.Admin.Models;

namespace WebAppHealth.Areas.Admin.Controllers
{
    public class DoctorsController : BaseAdminController
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: Admin/Doctors
        public ActionResult Index(string search)
        {
            // 1. Join bảng Doctors -> Users và Doctors -> Departments
            var query = db.Doctors
                .Include(d => d.User)
                .Include(d => d.Department)
                .AsQueryable();

            // 2. Tìm kiếm theo tên (nếu có)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.FullName.Contains(search) || d.User.Email.Contains(search));
            }

            // 3. Map sang ViewModel để hiển thị
            var model = query.Select(d => new DoctorViewModel
            {
                DoctorID = d.DoctorID,
                FullName = d.FullName,
                Avatar = d.Avatar,
                Degree = d.Degree,
                DepartmentName = d.Department.Name,
                Email = d.User.Email,
                Phone = d.User.PhoneNumber, // Lưu ý check lại tên cột trong DB của bạn (Phone hay PhoneNumber)
                IsActive = d.User.IsActive ?? true
            }).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        // GET: Admin/Doctors/Create
        public ActionResult Create()
        {
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name");
            return View();
        }

        // POST: Admin/Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // B1: Tạo User mới cho Bác sĩ trước
                        var user = new User
                        {
                            Email = model.Email,
                            PhoneNumber = model.Phone,
                            PasswordHash = "e10adc3949ba59abbe56e057f20f883e", // Mặc định pass 123456
                            Role = "Doctor",
                            IsActive = true,
                            // CreatedAt = DateTime.Now // Nếu bảng User có cột này
                        };
                        db.Users.Add(user);
                        db.SaveChanges(); // Lưu để lấy UserID

                        // B2: Xử lý Upload ảnh
                        string avatarFilename = "default-doctor.png";
                        if (model.AvatarFile != null && model.AvatarFile.ContentLength > 0)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(model.AvatarFile.FileName);
                            string extension = Path.GetExtension(model.AvatarFile.FileName);
                            avatarFilename = fileName + "_" + DateTime.Now.Ticks + extension;
                            string path = Path.Combine(Server.MapPath("~/Uploads/Doctors/"), avatarFilename);

                            // Tạo thư mục nếu chưa có
                            if (!Directory.Exists(Server.MapPath("~/Uploads/Doctors/")))
                                Directory.CreateDirectory(Server.MapPath("~/Uploads/Doctors/"));

                            model.AvatarFile.SaveAs(path);
                        }

                        // B3: Tạo thông tin Bác sĩ
                        var doctor = new Doctor
                        {
                            UserID = user.UserID, // Link với User vừa tạo
                            FullName = model.FullName,
                            DepartmentID = model.DepartmentID,
                            Degree = model.Degree,
                            Bio = model.Bio,
                            Avatar = avatarFilename
                        };

                        db.Doctors.Add(doctor);
                        db.SaveChanges();

                        transaction.Commit(); // Xác nhận transaction
                        SetAlert("Thêm mới bác sĩ thành công!", "success");
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    }
                }
            }

            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", model.DepartmentID);
            return View(model);
        }

        // Bạn có thể thêm Action Edit và Delete tương tự
    }
}