using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSMandate.Models
{
    public class MandateAccountModel
    {
        public string BrnCd { get; set; }
        public string GLCd { get; set; }
        public string AccNo { get; set; }
        public string AcNo { get; set; }
        public string AcName { get; set; }
        public string FName { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public decimal ClrBal { get; set; }

        public string PANNo { get; set; }

        public string AadhaarNo { get; set; }
        public string KYCStatus { get; set; }

        public string CrdtrName { get; set; }

    }
}