using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Security;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebAppHealth.Models;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Controllers
{
    public class StaffController : Controller
    {

        private HealthDBContext db = new HealthDBContext();


        // GET: Staff
        public ActionResult StaffLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StaffLogin(PacsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu nhập vào sang MD5
                string passwordSubmit = GetMD5(model.Password);

                // So sánh với dữ liệu trong Database
                // Điều kiện: Đúng User, Đúng Pass (đã mã hóa), Đúng Role Doctor, và phải Active
                var user = db.Users.FirstOrDefault(u =>
                    (u.Email == model.Username || u.PhoneNumber == model.Username) &&
                    u.PasswordHash == passwordSubmit &&
                    u.Role == "Doctor" &&
                    u.IsActive == true
                );

                if (user != null)
                {
                    Session["UserID"] = user.UserID;
                    Session["Username"] = user.Email;
                    Session["Role"] = user.Role;

                    return RedirectToAction("Dashboard", "Staff");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập, mật khẩu không đúng hoặc bạn không có quyền truy cập.");
                }
            }
            return View(model);
        }

        public ActionResult Dashboard()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            return View();
        }

        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                // x2 để in ra ký tự thường (e10adc...)
                byte2String += targetData[i].ToString("x2");
            }
            return byte2String;
        }
    }
}