using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class DepartmentsController : Controller
    {
        private HealthDBContext db = new HealthDBContext();

        // Trang lịch khám chữa bệnh
        public ActionResult Calender()
        {
            return View();
        }

        // Trang nội khoa
        public ActionResult InternalMedicine()
        {
            var model = db.Departments
                  .Include("Doctors")
                  .Where(d => d.DepartmentID >= 1 && d.DepartmentID <= 14)
                  .OrderBy(d => d.DepartmentID)
                  .ToList();

            ViewBag.PositionMap = db.Positions.ToDictionary(p => p.PositionID, p => p.PositionName);


            return View(model);
        }

        // Trang ngoại khoa
        public ActionResult Surgery()
        {
            var model = db.Departments
                  .Include("Doctors")
                  .Where(d => d.DepartmentID >= 15 && d.DepartmentID <= 23)
                  .OrderBy(d => d.DepartmentID)
                  .ToList();

            ViewBag.PositionMap = db.Positions.ToDictionary(p => p.PositionID, p => p.PositionName);


            return View(model);
        }

        // Trang Cận lâm sàng
        public ActionResult Paraclinical()
        {
            var model = db.Departments
                  .Include("Doctors")
                  .Where(d => d.DepartmentID >= 23 && d.DepartmentID <= 28)
                  .OrderBy(d => d.DepartmentID)
                  .ToList();

            ViewBag.PositionMap = db.Positions.ToDictionary(p => p.PositionID, p => p.PositionName);


            return View(model);
        }
    }
}