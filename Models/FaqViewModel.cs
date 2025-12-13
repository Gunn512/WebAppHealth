using System.Collections.Generic;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Models
{
    public class FaqViewModel
    {
        // 1. Danh sách câu hỏi đã trả lời (để hiển thị bên trái)
        public List<Faq> ListFaq { get; set; }

        // 2. Đối tượng dùng cho Form đặt câu hỏi (để nhập liệu bên phải)
        public Faq NewQuestion { get; set; }

        // 3. Thông tin phân trang (để vẽ cái 1, 2, 3...)
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}