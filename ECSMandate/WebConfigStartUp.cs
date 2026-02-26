using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EasyBank.Framework.EasyCaching;
using EasyBank.Framework.EasySession;
using EASYECS.BussinessLayer.ECSService.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSMandate
{
    public class WebConfigStartUp : IConfigStartUp
    {

        //public bool SYS_ADMIN { get; set; }
        //public string USER_CATEGORY { get; set; }

        public string APBS_AC_NO
        {
            get
            {
                var data = CacheManager.Cache["APBSSetACNo"] as string;
                if (null == data)
                {
                    SysParameter sysParam = new SysParameterService().GetByField("APBSSetACNo");
                    data = sysParam.Value;
                    CacheManager.Cache["APBSSetACNo"] = data;
                }
                return data;
            }
            set
            {
                CacheManager.Cache["APBSSetACNo"] = value;
            }
        }

        public string APBS_GL_CD
        {
            get
            {
                var data = CacheManager.Cache["APBSSetGlCd"] as string;
                if (null == data)
                {
                    SysParameter sysParam = new SysParameterService().GetByField("APBSSetGlCd");
                    data = sysParam.Value;
                    CacheManager.Cache["APBSSetGlCd"] = data;
                }
                return data;
            }
            set
            {
                CacheManager.Cache["APBSSetGlCd"] = value;
            }
        }

        public string INWARD_FILE_PATH
        {           
            get
            {
                object data = CacheManager.Cache["Inward_File_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data =  mandateService.GetEhConfigByField("Inward_File_Path");
                    CacheManager.Cache["Inward_File_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["Inward_File_Path"] = value;
            }
        }
        public string INWARD_IMAGE_PATH
        {
            get
            {
                object data = CacheManager.Cache["Inward_Image_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data = mandateService.GetEhConfigByField("Inward_Image_Path");
                    CacheManager.Cache["Inward_Image_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["Inward_Image_Path"] = value;
            }
        }
        public string INWARD_RESPONSE_PATH
        {
            get
            {
                object data = CacheManager.Cache["Inward_Response_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data = mandateService.GetEhConfigByField("Inward_Response_Path");
                    CacheManager.Cache["Inward_Response_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["Inward_Image_Path"] = value;
            }
        }
        public string INWARD_PROCESSED_PATH
        {
            get
            {
                object data = CacheManager.Cache["Inward_Processed_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data = mandateService.GetEhConfigByField("Inward_Processed_Path");
                    CacheManager.Cache["Inward_Processed_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["Inward_Processed_Path"] = value;
            }
        }
        public string MANDATE_SCAN_PATH
        {
            get
            {
                object data = CacheManager.Cache["Mandate_Scan_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data = mandateService.GetEhConfigByField("Mandate_Scan_Path");
                    CacheManager.Cache["Mandate_Scan_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["Mandate_Scan_Path"] = value;
            }
        }

        public string INWARD_OUTPUT_PATH
        {
            get
            {
                object data = CacheManager.Cache["Inward_Output_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data = mandateService.GetEhConfigByField("Inward_Output_Path");
                    CacheManager.Cache["Inward_Output_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["Inward_Output_Path"] = value;
            }
        }

        public string ACKNOWLEDGE_PATH
        {
            get
            {
                object data = CacheManager.Cache["Acknowledge_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data = mandateService.GetEhConfigByField("Acknowledge_Path");
                    CacheManager.Cache["Acknowledge_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["Acknowledge_Path"] = value;
            }
        }

        public string H2H_SHARE_PATH
        {
            get
            {
                object data = CacheManager.Cache["H2H_Share_Path"];
                if (null == data)
                {
                    MandateService mandateService = new MandateService();
                    data = mandateService.GetEhConfigByField("H2H_Share_Path");
                    CacheManager.Cache["H2H_Share_Path"] = data;
                }
                return Convert.ToString(data);
            }
            set
            {
                CacheManager.Cache["H2H_Share_Path"] = value;
            }
        }
        public void LoadParameters()
        {
            MandateService mandateService = new MandateService();

            INWARD_FILE_PATH =  mandateService.GetEhConfigByField("Inward_File_Path");
            INWARD_IMAGE_PATH = mandateService.GetEhConfigByField("Inward_Image_Path");
            INWARD_RESPONSE_PATH = mandateService.GetEhConfigByField("Inward_Response_Path");
            INWARD_PROCESSED_PATH = mandateService.GetEhConfigByField("Inward_Processed_Path");
            MANDATE_SCAN_PATH = mandateService.GetEhConfigByField("Mandate_Scan_Path");
            INWARD_OUTPUT_PATH = mandateService.GetEhConfigByField("Inward_Output_Path");
            ACKNOWLEDGE_PATH = mandateService.GetEhConfigByField("Acknowledge_Path");
            H2H_SHARE_PATH = mandateService.GetEhConfigByField("H2H_Share_Path");
        }
    }
}