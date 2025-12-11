using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppHealth.Models
{
    public class SidebarItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }
}