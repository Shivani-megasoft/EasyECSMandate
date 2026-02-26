using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSMandate.Models
{
    public class SignatureModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Src { get; set; }
        public DateTime SignFileDt { get; set; }
    }
}