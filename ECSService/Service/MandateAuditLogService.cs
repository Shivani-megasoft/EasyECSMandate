using System.Text;
using System.Reflection;
using EasyBank.Framework.EasyAttributes;

namespace ECSService.Service
{
    public class MandateAuditLogService
    {
        public string BuildData(object orgObj, object logObj)
        {
            object[] attributes = null;
            object orgVal = null, logVal = null;
            StringBuilder sb = new StringBuilder();

            PropertyInfo[] pInfo = orgObj.GetType().GetProperties();
            foreach (PropertyInfo pi in pInfo)
            {
                //[CR23]: Initialsed to empty to avoid duplicate data.
                orgVal = logVal = string.Empty;
                attributes = pi.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (attributes.GetLength(0) == 0 || pi.Name == "Id" || pi.Name == "DtStamp")
                    continue;

                if (pi.GetValue(orgObj, null) != null)
                    orgVal = pi.GetValue(orgObj, null);

                if (pi.GetValue(logObj, null) != null)
                    logVal = pi.GetValue(logObj, null);

                if (orgVal.ToString() == "")
                    continue;


                if (!orgVal.Equals(logVal))
                    sb.Append(pi.Name + " : " + orgVal + " | " + logVal + " ^ ");
            }

            return sb.ToString();
        }



    }

}