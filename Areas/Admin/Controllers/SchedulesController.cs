using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using WebAppHealth.Models.DataModel;
using WebAppHealth.Areas.Admin.Models;

namespace WebAppHealth.Areas.Admin.Controllers
{
    public class SchedulesController : BaseAdminController
    {
        private HealthDBContext db = new HealthDBContext();

        // GET: Admin/Schedules
        public ActionResult Index(DateTime? date, int? doctorId)
        {
            var query = db.Schedules
                .Include(s => s.Doctor)
                .Include(s => s.Doctor.Department)
                .Include(s => s.TimeSlot)
                .AsQueryable();

            // Filter
            if (date.HasValue)
            {
                query = query.Where(s => s.DateWork == date.Value);
            }
            if (doctorId.HasValue)
            {
                query = query.Where(s => s.DoctorID == doctorId.Value);
            }

            // Sắp xếp: Ngày mới nhất -> Bác sĩ -> Giờ
            query = query.OrderByDescending(s => s.DateWork)
                         .ThenBy(s => s.Doctor.FullName)
                         .ThenBy(s => s.TimeSlot.StartTime);

            var model = query.Select(s => new ScheduleViewModel
            {
                ScheduleID = s.ScheduleID,
                DoctorName = s.Doctor.FullName,
                DepartmentName = s.Doctor.Department.Name,
                DateWork = s.DateWork ?? DateTime.Now,
                // Format giờ từ TimeSpan sang chuỗi hh:mm
                TimeSlot = s.TimeSlot.StartTime.ToString().Substring(0, 5) + " - " + s.TimeSlot.EndTime.ToString().Substring(0, 5),
                Status = s.Status
            }).ToList();

            ViewBag.Doctors = new SelectList(db.Doctors, "DoctorID", "FullName", doctorId);
            ViewBag.CurrentDate = date?.ToString("yyyy-MM-dd");

            return View(model);
        }

        // GET: Admin/Schedules/Create
        public ActionResult Create()
        {
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FullName");

            // Lấy danh sách tất cả TimeSlots đang hoạt động để hiển thị checkbox
            ViewBag.TimeSlots = db.TimeSlots.Where(t => t.IsActive == true)
                                            .OrderBy(t => t.StartTime)
                                            .ToList();

            return View(new CreateScheduleViewModel { DateWork = DateTime.Now.Date.AddDays(1) });
        }

        // POST: Admin/Schedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateScheduleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra danh sách Slot được chọn
                if (model.SelectedSlotIDs == null || !model.SelectedSlotIDs.Any())
                {
                    ModelState.AddModelError("", "Bạn phải chọn ít nhất một khung giờ.");
                }
                else
                {
                    int countAdded = 0;
                    foreach (var slotId in model.SelectedSlotIDs)
                    {
                        // Kiểm tra trùng lặp: Bác sĩ này + Ngày này + Slot này đã có chưa?
                        bool exists = db.Schedules.Any(s => s.DoctorID == model.DoctorID
                                                         && s.DateWork == model.DateWork
                                                         && s.SlotID == slotId);
                        if (!exists)
                        {
                            var schedule = new Schedule
                            {
                                DoctorID = model.DoctorID,
                                DateWork = model.DateWork,
                                SlotID = slotId,
                                Status = "Available"
                            };
                            db.Schedules.Add(schedule);
                            countAdded++;
                        }
                    }

                    if (countAdded > 0)
                    {
                        db.SaveChanges();
                        SetAlert($"Đã thêm thành công {countAdded} ca làm việc.", "success");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        SetAlert("Các ca làm việc này đã tồn tại, không có gì thay đổi.", "warning");
                    }
                }
            }

            // Nếu lỗi, load lại dữ liệu để hiện View
            ViewBag.DoctorID = new SelectList(db.Doctors, "DoctorID", "FullName", model.DoctorID);
            ViewBag.TimeSlots = db.TimeSlots.Where(t => t.IsActive == true).OrderBy(t => t.StartTime).ToList();
            return View(model);
        }

        // POST: Admin/Schedules/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                // Logic: Chỉ cho xóa khi chưa có ai đặt (Appointment)
                // Nhưng theo yêu cầu MVP, ta cho xóa đơn giản trước
                var schedule = db.Schedules.Find(id);
                if (schedule != null)
                {
                    // Kiểm tra ràng buộc (nếu cần): if (db.Appointments.Any(a => a.ScheduleID == id)) return Json...

                    db.Schedules.Remove(schedule);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa lịch làm việc." });
                }
                return Json(new { success = false, message = "Không tìm thấy lịch." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}