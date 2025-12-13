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
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string f_passwordHash = GetMD5(model.Password);
                var user = db.Users.FirstOrDefault(u =>
                    (u.Email == model.UserName || u.PhoneNumber == model.UserName) &&
                    u.PasswordHash == f_passwordHash &&
                    u.IsActive == true
                );

                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Email, model.RememberMe);
                    Session["UserID"] = user.UserID;
                    Session["Role"] = user.Role;  
                    Session["UserName"] = user.Email;

                    // LOGIC ĐIỀU HƯỚNG

                    if (user.Role == "Admin" || user.Role == "Staff" || user.Role == "Doctor")
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Contains("Admin"))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                    else
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("Admin"))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("PatientDashboard", "ServicePortal");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
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
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}