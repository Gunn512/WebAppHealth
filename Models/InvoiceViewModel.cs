using System.Collections.Generic;

namespace WebAppHealth.Models.ViewModels
{
    // Dùng cho vòng lặp danh sách (Index)
    public class InvoiceHistoryItem
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public string CreatedDate { get; set; }
        public string Content { get; set; }

        //public decimal TotalAmount { get; set; }

        //public decimal TaxAmount { get; set; }    

        //public string Status { get; set; }
        public string TotalAmountStr { get; set; }

        
    }

    // Dùng cho JSON trả về (Detail)
    public class InvoiceDetailVM
    {
        public string InvoiceCode { get; set; }
        public string CreatedDateString { get; set; }
        public string PatientName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }

        public decimal SubTotal { get; set; }
        public decimal VAT { get; set; }
        public decimal GrandTotal { get; set; }

        // Danh sách hàng hóa
        public List<InvoiceLineItem> Items { get; set; }

        public string Note { get; set; }
    }

    public class InvoiceLineItem
    {
        public int Stt { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}