using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models.DataModel; // Namespace chứa Entity Framework generated class
using WebAppHealth.Areas.Admin.Models;

namespace WebAppHealth.Areas.Admin.Controllers
{
    public class MedicinesController : BaseAdminController
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: Admin/Medicines
        public ActionResult Index(string search)
        {
            var query = db.Medicines.AsQueryable();

            // Tìm kiếm theo Tên hoặc Hoạt chất
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.ActiveIngredient.Contains(search));
            }

            var model = query.OrderBy(m => m.Name).Select(m => new MedicineViewModel
            {
                MedicineID = m.MedicineID,
                Name = m.Name,
                Unit = m.Unit,
                Price = m.Price ?? 0,
                Image = m.Image,
                ActiveIngredient = m.ActiveIngredient,
                Manufacturer = m.Manufacturer,
                Packaging = m.Packaging
            }).ToList();

            ViewBag.Search = search;
            return View(model);
        }

        // GET: Admin/Medicines/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Medicines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MedicineViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string uploadFolder = Server.MapPath("~/Uploads/Medicines/");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    // 1. Xử lý Ảnh
                    string imgName = "default-drug.png";
                    if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(model.ImageFile.FileName);
                        imgName = "drug_" + DateTime.Now.Ticks + ext;
                        model.ImageFile.SaveAs(Path.Combine(uploadFolder, imgName));
                    }

                    // 2. Xử lý File Hướng dẫn (nếu có)
                    string docName = null;
                    if (model.InstructionDoc != null && model.InstructionDoc.ContentLength > 0)
                    {
                        string docFolder = Path.Combine(uploadFolder, "Docs");
                        if (!Directory.Exists(docFolder)) Directory.CreateDirectory(docFolder);

                        string ext = Path.GetExtension(model.InstructionDoc.FileName);
                        docName = "guide_" + DateTime.Now.Ticks + ext;
                        model.InstructionDoc.SaveAs(Path.Combine(docFolder, docName));
                    }

                    // 3. Lưu vào DB
                    var medicine = new Medicine
                    {
                        Name = model.Name,
                        Unit = model.Unit,
                        Price = model.Price,
                        ActiveIngredient = model.ActiveIngredient,
                        Concentration = model.Concentration,
                        DrugGroup = model.DrugGroup,
                        DosageForm = model.DosageForm,
                        Manufacturer = model.Manufacturer,
                        Packaging = model.Packaging,
                        Image = imgName,
                        InstructionFile = docName
                    };

                    db.Medicines.Add(medicine);
                    db.SaveChanges();
                    SetAlert("Thêm thuốc mới thành công!", "success");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            return View(model);
        }

        // POST: Admin/Medicines/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var medicine = db.Medicines.Find(id);
                if (medicine != null)
                {
                    // Xóa ảnh cũ (trừ ảnh mặc định)
                    if (!string.IsNullOrEmpty(medicine.Image) && medicine.Image != "default-drug.png")
                    {
                        string path = Server.MapPath("~/Uploads/Medicines/" + medicine.Image);
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }

                    // Xóa file hướng dẫn
                    if (!string.IsNullOrEmpty(medicine.InstructionFile))
                    {
                        string path = Server.MapPath("~/Uploads/Medicines/Docs/" + medicine.InstructionFile);
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }

                    db.Medicines.Remove(medicine);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa thuốc thành công." });
                }
                return Json(new { success = false, message = "Không tìm thấy thuốc." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}