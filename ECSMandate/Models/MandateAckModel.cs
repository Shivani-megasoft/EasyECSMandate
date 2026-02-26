using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSMandate.Models
{
    public class MandateAckModel
    {
        public string Type { get; set; }
        public string MsgId { get; set; }
        public string UMRNo { get; set; }
        public DateTime UMRDt { get; set; }
        public string FileName { get; set; }
        public string IsAccept { get; set; }
        public string Reason { get; set; }
        public string AcNo { get; set; }
        public string CreditorsName { get; set; }       
        public string CrBrnCd { get; set; }
        public string CrGlCd { get; set; }
        public string CrAcNo { get; set; }
        public string Mobile { get; set; }
        public string Amt { get; set; }
        public string DebtrName { get; set; }

        public string Status { get; set; }

    }
}
