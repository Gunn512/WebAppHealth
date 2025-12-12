using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models;
using System.Data.Entity;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class IntroController : Controller
    {
        private HealthDBContext db = new HealthDBContext();
        public ActionResult Index()
        {
            var model = new IntroViewModel();
            model.Directors = db.Doctors
                                .Include("Department")
                                .Where(d => d.PositionID == 1 || d.PositionID == 2)
                                .OrderBy(d => d.PositionID)
                                .ThenBy(d => d.DoctorID)
                                .ToList();

            model.FunctionalDepartments = db.Departments
                                            .Where(d => d.Name.Contains("Phòng"))
                                            .ToList();

            model.PositionMap = db.Positions
                          .ToDictionary(p => p.PositionID, p => p.PositionName);

            return View(model);
        }
        


        public ActionResult PhongChucNang()
        {
            var listPhongBan = db.Departments
                                 .Where(d => d.Name.Contains("Phòng"))
                                 .OrderBy(d => d.DepartmentID)
                                 .ToList();

            return View(listPhongBan);
        }
    }
}