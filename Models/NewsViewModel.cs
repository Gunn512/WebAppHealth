using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Models
{
    public class NewsViewModel
    {
        // Bài viết nổi bật
        public News FeaturedArticle { get; set; }

        // Tin tức chung
        public List<News> GeneralNewsList { get; set; }

        // Sidebar phải
        public List<News> SidebarNewsList { get; set; }

        // Phân trang
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}