using System.Linq;
using System.Web;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Models
{
    public class ConfigHelper
    {
        public static string Get(string key)
        {
            // 1. Nếu chưa có dữ liệu trong bộ nhớ tạm (Application State) thì load từ DB lên
            if (HttpContext.Current.Application["SystemConfigs"] == null)
            {
                RefreshCache();
            }

            // 2. Lấy danh sách từ bộ nhớ
            var configs = HttpContext.Current.Application["SystemConfigs"] as System.Collections.Generic.Dictionary<string, string>;

            // 3. Tìm giá trị, nếu không thấy thì trả về chuỗi rỗng để không lỗi web
            if (configs != null && configs.ContainsKey(key))
            {
                return configs[key];
            }

            return "Đang cập nhật";
        }

        public static void RefreshCache()
        {
            using (var db = new HealthDBContext())
            {
                // Load toàn bộ bảng Config đưa vào Dictionary để tra cứu cho nhanh
                var data = db.SystemConfigs.ToDictionary(k => k.ConfigKey, v => v.ConfigValue);
                HttpContext.Current.Application["SystemConfigs"] = data;
            }
        }
    }
}