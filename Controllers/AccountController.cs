using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class AccountController : Controller
    {
        private HealthDBContext db = new HealthDBContext();

        private string GetMD5(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            using (MD5 md5 = MD5.Create())
            {
                byte[] fromData = Encoding.UTF8.GetBytes(str);
                byte[] targetData = md5.ComputeHash(fromData);

                StringBuilder byte2String = new StringBuilder();
                for (int i = 0; i < targetData.Length; i++)
                {
                    byte2String.Append(targetData[i].ToString("x2"));
                }
                return byte2String.ToString();
            }
        }

        // ================ Đăng nhập ==================
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string loginInput, string password, string returnUrl)
        {

            if (ModelState.IsValid)
            {
                // B1: Mã hóa mật khẩu
                string f_passwordHash = GetMD5(password);

                // B2: Tìm User có Email or Phone = ipnput, khớp pass và còn hoạt động
                var user = db.Users.FirstOrDefault(u =>
                    (u.Email == loginInput || u.PhoneNumber == loginInput) &&
                    u.PasswordHash == f_passwordHash &&
                    u.IsActive == true
                );

                if (user != null)
                {
                    // B3: Tạo Cookie xác thực (Lưu Username vào cookie để định danh)
                    FormsAuthentication.SetAuthCookie(user.Email, false);

                    // B4: Điều hướng
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    // Mặc định về trang Dashboard
                    return RedirectToAction("PatientDashboard", "ServicePortal");
                }
                else
                {
                    ModelState.AddModelError("", "Email/SĐT hoặc mật khẩu không đúng");
                }
            }
            return View();
        }


        // ===================== ĐĂNG KÝ ====================== 
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User _user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Email hoặc SĐT đã tồn tại chưa
                var check = db.Users.FirstOrDefault(s => s.Email == _user.Email || s.PhoneNumber == _user.PhoneNumber);

                if (check == null)
                {
                    // 1. Gán Role mặc định
                    _user.Role = "Patient";

                    // 2. Kích hoạt tài khoản ngay lập tức
                    _user.IsActive = true;

                    // 3. Mã hóa mật khẩu trước khi lưu
                    _user.PasswordHash = GetMD5(_user.PasswordHash);

                    db.Users.Add(_user);
                    db.SaveChanges();

                    // Đăng ký xong chuyển qua đăng nhập
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc SĐT đã được đăng ký.");
                    return View();
                }
            }
            return View();
        }

        // ================= Đăng xuất ==================
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("PatientDashboard", "ServicePortal");
        }
    }
}