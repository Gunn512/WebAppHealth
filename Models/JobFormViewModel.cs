using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WebAppHealth.Models
{
    public class JobFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập CCCD")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "CCCD phải đúng 12 số")]
        public string IdCard { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày cấp")]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nơi cấp")]
        public string IssuePlace { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập quê quán")]
        public string Hometown { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập trình độ chuyên môn")]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập trường đào tạo")]
        public string University { get; set; }

        public string TrainingSystem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm tốt nghiệp")]
        public int GradYear { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn xếp loại")]
        public string GradRank { get; set; }

        public string LanguageCert { get; set; }
        public string InformaticsCert { get; set; }
        public string Experience { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên hồ sơ")]
        public HttpPostedFileBase CVFile { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên ảnh bằng cấp")]
        public HttpPostedFileBase[] CertificateImages { get; set; }
    }
}