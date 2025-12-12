using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class DoctorsController : Controller
    {
        // GET: Doctors
        public ActionResult Index()
        {
            return View();
        }
    }
}