using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class HomeController : Controller
    {

        private HealthDBContext db = new HealthDBContext();
        public ActionResult Index()
        {
            var model = new HomeViewModel();

            model.LatestNews = db.News
                                 .OrderByDescending(x => x.PublishDate)
                                 .Take(4)
                                 .ToList();

            // 2. Lấy Video
            model.ListVideos = db.Videos.ToList();

            // 3. Lấy Góc tri ân
            model.ListTriAns = db.TriAns.ToList();

            // 4. Lấy Thông tin Sở Y tế (Sidebar)
            var allDocs = db.Documents.ToList();

            model.VanBans = allDocs.Where(x => x.Category == "VanBan").ToList();
            model.ThongBaos = allDocs.Where(x => x.Category == "ThongBao").ToList();
            model.ThuMois = allDocs.Where(x => x.Category == "ThuMoi").ToList();
            model.LichCongTacs = allDocs.Where(x => x.Category == "LichCongTac").ToList();

            return View(model);
        }

    }
}