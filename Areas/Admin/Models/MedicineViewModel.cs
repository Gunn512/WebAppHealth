using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WebAppHealth.Areas.Admin.Models
{
    public class MedicineViewModel
    {
        public int MedicineID { get; set; }

        [Display(Name = "Tên thuốc")]
        [Required(ErrorMessage = "Vui lòng nhập tên thuốc")]
        public string Name { get; set; }

        [Display(Name = "Đơn vị tính")]
        [Required(ErrorMessage = "Vui lòng nhập đơn vị (viên, hộp, vỉ...)")]
        public string Unit { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Display(Name = "Hoạt chất chính")]
        public string ActiveIngredient { get; set; }

        [Display(Name = "Hàm lượng")]
        public string Concentration { get; set; } // Ví dụ: 500mg

        [Display(Name = "Nhóm thuốc")]
        public string DrugGroup { get; set; } // Ví dụ: Kháng sinh, Giảm đau

        [Display(Name = "Dạng bào chế")]
        public string DosageForm { get; set; } // Ví dụ: Viên nén, Siro

        [Display(Name = "Nhà sản xuất")]
        public string Manufacturer { get; set; }

        [Display(Name = "Quy cách đóng gói")]
        public string Packaging { get; set; } // Ví dụ: Hộp 10 vỉ x 10 viên

        // --- Xử lý Upload ---

        [Display(Name = "Hình ảnh")]
        public string Image { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }

        [Display(Name = "File Hướng dẫn (PDF/Doc)")]
        public string InstructionFile { get; set; }
        public HttpPostedFileBase InstructionDoc { get; set; }
    }
}