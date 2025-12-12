using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

//namespace WebAppHealth
namespace WebAppHealth.Models.DataModel
{
    [MetadataType(typeof(FaqMetadata))]
    public partial class Faq
    {
    }

    public class FaqMetadata
    {
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string FullName { get; set; }

        [Display(Name = "Tuổi")]
        [Range(1, 120, ErrorMessage = "Tuổi không hợp.")]
        public Nullable<int> Age { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập SĐT.")]
        [RegularExpression(@"^(03|05|07|08|09)[0-9]{8}$", ErrorMessage = "SĐT không đúng .")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Display(Name = "Câu hỏi")]
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi.")]
        [StringLength(1000, ErrorMessage = "Câu hỏi không được quá 1000 ký tự.")]
        public string Question { get; set; }

    }
}