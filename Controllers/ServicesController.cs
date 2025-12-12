using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class ServicesController : Controller
    {
        private HealthDBContext db = new HealthDBContext();
        public ActionResult CustomerService()
        {
            var services = db.Services.ToList();
            return View(services);
        }

        // Trang hướng dẫn - quy trình
        public ActionResult PatientGuide()
        {
            var list = new List<PatientGuideModel>
            {
                new PatientGuideModel { Id = "kham-benh", Title = "Quy trình khám bệnh", ImageName = "medical-background.jpg", IsActive = true },
                new PatientGuideModel { Id = "xet-nghiem", Title = "Quy trình xét nghiệm", ImageName = "banner_04.jpg", IsActive = false },
                new PatientGuideModel { Id = "kham-yeu-cau", Title = "Quy trình khám bệnh theo yêu cầu", ImageName = "banner_05.jpg", IsActive = false },
                new PatientGuideModel { Id = "kham-suc-khoe", Title = "Quy trình khám sức khỏe", ImageName = "banner_06.jpg", IsActive = false }
            };

            return View(list);
        }

        // Trang giá viện phí
        public ActionResult HospitalFees()
        {
            var documents = new List<ServiceModel>
            {
                new ServiceModel { Title = "Bảng giá Viện phí", FileName = "dichvu_01.pdf", IsActive = true },
                new ServiceModel { Title = "Bảng giá Dịch vụ - Viện phí", FileName = "dichvu_02.pdf", IsActive = false },
                new ServiceModel { Title = "Bảng giá Bảo hiểm y tế", FileName = "dichvu_03.pdf", IsActive = false },
                new ServiceModel { Title = "Bảng giá thuốc (BHYT)", FileName = "dichvu_04.pdf", IsActive = false },
                new ServiceModel { Title = "Bảng giá thuốc (Nhà thuốc BV)", FileName = "dichvu_05.pdf", IsActive = false },
                new ServiceModel { Title = "Bảng giá vật tư y tế", FileName = "dichvu_06.pdf", IsActive = false },
                new ServiceModel { Title = "Bảng giá cấp cứu 115", FileName = "dichvu_07.pdf", IsActive = false }
            };

            return View(documents);
        }

    }
}