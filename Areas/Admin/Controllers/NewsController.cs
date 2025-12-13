using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebAppHealth.Models.DataModel;
using WebAppHealth.Areas.Admin.Models;

namespace WebAppHealth.Areas.Admin.Controllers
{
    public class NewsController : BaseAdminController
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: Admin/News
        public ActionResult Index(string search)
        {
            var query = db.News.Include(n => n.NewsCategory).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(n => n.Title.Contains(search));
            }

            var model = query.OrderByDescending(n => n.PublishDate).Select(n => new NewsViewModel
            {
                NewsID = n.NewsID,
                Title = n.Title,
                CategoryName = n.NewsCategory.CategoryName,
                PublishDate = n.PublishDate ?? DateTime.Now,
                Image = n.Image,
                Author = n.Author
            }).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        // GET: Admin/News/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.NewsCategories.Where(c => c.IsActive == true), "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/News/Create
        [HttpPost]
        [ValidateInput(false)] // QUAN TRỌNG: Cho phép gửi HTML
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload ảnh
                    string imageFilename = "default-news.jpg";
                    if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                        string extension = Path.GetExtension(model.ImageFile.FileName);
                        imageFilename = fileName + "_" + DateTime.Now.Ticks + extension;
                        string path = Path.Combine(Server.MapPath("~/Uploads/News/"), imageFilename);

                        if (!Directory.Exists(Server.MapPath("~/Uploads/News/")))
                            Directory.CreateDirectory(Server.MapPath("~/Uploads/News/"));

                        model.ImageFile.SaveAs(path);
                    }

                    var news = new News
                    {
                        Title = model.Title,
                        CategoryID = model.CategoryID,
                        Content = model.Content,
                        Image = imageFilename,
                        PublishDate = DateTime.Now,
                        Author = Session["UserName"] != null ? Session["UserName"].ToString() : "Admin"
                    };

                    db.News.Add(news);
                    db.SaveChanges();
                    SetAlert("Đăng bài viết thành công!", "success");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            ViewBag.CategoryID = new SelectList(db.NewsCategories.Where(c => c.IsActive == true), "CategoryID", "CategoryName", model.CategoryID);
            return View(model);
        }

        // POST: Admin/News/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var news = db.News.Find(id);
                if (news != null)
                {
                    // Xóa ảnh cũ nếu không phải ảnh mặc định (Tùy chọn)
                    if (!string.IsNullOrEmpty(news.Image) && news.Image != "default-news.jpg")
                    {
                        string path = Path.Combine(Server.MapPath("~/Uploads/News/"), news.Image);
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }

                    db.News.Remove(news);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa bài viết." });
                }
                return Json(new { success = false, message = "Không tìm thấy bài viết." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}