using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc; // Dùng cho [AllowHtml]

namespace WebAppHealth.Areas.Admin.Models
{
    public class NewsViewModel
    {
        public int NewsID { get; set; }

        [Display(Name = "Tiêu đề bài viết")]
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        [Display(Name = "Nội dung")]
        [AllowHtml] // Cho phép chứa thẻ HTML (để dùng CKEditor sau này)
        public string Content { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string Image { get; set; }
        public HttpPostedFileBase ImageFile { get; set; } // Để upload ảnh

        [Display(Name = "Ngày đăng")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime PublishDate { get; set; }

        [Display(Name = "Tác giả")]
        public string Author { get; set; }
    }
}