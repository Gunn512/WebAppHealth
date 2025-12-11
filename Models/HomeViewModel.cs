using System.Collections.Generic;
using WebAppHealth;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Models
{
    public class HomeViewModel
    {
        public List<News> LatestNews { get; set; }
        public List<Video> ListVideos { get; set; }
        public List<TriAn> ListTriAns { get; set; }

        // Sidebar Data
        public List<Document> VanBans { get; set; }
        public List<Document> ThongBaos { get; set; }
        public List<Document> ThuMois { get; set; }
        public List<Document> LichCongTacs { get; set; }
    }
}