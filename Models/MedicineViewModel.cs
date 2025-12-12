using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAppHealth.Models.DataModel;

namespace WebAppHealth.Models
{
    public class MedicineViewModel
    {
        public List<Medicine> Medicines { get; set; }
        public string SearchKeyword { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}