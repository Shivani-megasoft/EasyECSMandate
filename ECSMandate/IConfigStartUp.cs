using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECSMandate
{
    public interface IConfigStartUp
    {
        string APBS_AC_NO { get; set; }
        string APBS_GL_CD { get; set; }
        string INWARD_FILE_PATH { get; set; }
        string INWARD_IMAGE_PATH { get; set; }
        string INWARD_RESPONSE_PATH { get; set; }
        string INWARD_PROCESSED_PATH { get; set; }
        string MANDATE_SCAN_PATH { get; set; }
        string INWARD_OUTPUT_PATH { get; set; }
        string H2H_SHARE_PATH { get; set; }
        string ACKNOWLEDGE_PATH { get; set; }

    }
}
