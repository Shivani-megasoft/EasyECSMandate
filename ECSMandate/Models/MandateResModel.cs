using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSMandate.Models
{
    public class MandateResModel
    {
        public DateTime CBSDate { get; set; }
        public string UMRNNo { get; set; }
        public string DebtrName { get; set; }
        public string BenAcNo { get; set; }
        public string BenAcName { get; set; }
        public string InputFile { get; set; }
        public DateTime InputDate { get; set; }
        public decimal Amt { get; set; }
        public string ResCd { get; set; }
        public string Status { get; set; }
      
    }
}
