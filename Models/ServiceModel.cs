using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppHealth.Models
{
    // Dùng cho trang Giá viện phí
    public class ServiceModel
    {
        public string Title { get; set; } 
        public string FileName { get; set; } 
        public bool IsActive { get; set; }
    }

    // Dùng cho trang Hướng dẫn - Quy trình
    public class PatientGuideModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
        public bool IsActive { get; set; }
    }
}