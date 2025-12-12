using System;
using System.Collections.Generic;

namespace WebAppHealth.Models.ViewModels
{
    // Dùng cho danh sách lịch sử khám
    public class MedicalHistoryItem
    {
        public int AppointmentID { get; set; }
        public int RecordID { get; set; }
        public string DateExam { get; set; }
        public string DepartmentName { get; set; }
        public string DoctorName { get; set; }
        public string Type { get; set; }
    }

    // Dùng cho chi tiết (trả về JSON khi click vào dòng)
    public class MedicalRecordDetailVM
    {
        public int RecordID { get; set; }
        public string DateExam { get; set; }
        public string DepartmentName { get; set; }
        public string DoctorName { get; set; }
        public string PatientCode { get; set; }
        public string Diagnosis { get; set; }
        public string DoctorNotes { get; set; }
        public string ReExamDate { get; set; }

        public List<PrescriptionItem> Prescriptions { get; set; }
        public List<string> LabResults { get; set; }
    }

    public class PrescriptionItem
    {
        public int Stt { get; set; }
        public string MedicineName { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
        public string Usage { get; set; }
    }
}