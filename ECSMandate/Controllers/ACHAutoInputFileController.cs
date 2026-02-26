using EasyBank.BusinessLayer.Base.Services;
using EasyBank.BusinessLayer.HOAdmin.Contracts;
using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EasyBank.Framework.EasySecurity;
using EasyBank.Infrastructure.SystemParameters;
using EasyBank.UI.Controllers;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace ECSMandate.Controllers
{
    public class ACHAutoInputFileController : Controller
    {
        
        ISysDayControlService sysDayControlService = new SysDayControlService();

        [HttpGet]
        [Route("ACHAutoInputFile/GenerateACHFile")]
        public ActionResult GenerateACHFile()
        {
            EasyBankPrincipal principal = new EasyBankPrincipal(new EasyBankIdentity("Admin"));//LoginId
            System.Threading.Thread.CurrentPrincipal = principal;
            MandateCommonService mandateCommonService = new MandateCommonService();
            MandateService mandateService = new MandateService();
            Response response = new Response();
            SysDayControl sysDayControl = null;
            DateTime FromDate = DateTime.Now;

            sysDayControl = sysDayControlService.GetMax();
            
            if (sysDayControl != null)
            {
                StartupManager.StartUp.DAY_OP_DT = sysDayControl.DayOpDt;
                 FromDate = sysDayControl.DayOpDt;
            }
 
            DateTime frmDt = Convert.ToDateTime(FromDate);
             DateTime setDt = Convert.ToDateTime(FromDate).AddDays(1);
             
            var ecsList = mandateService.GetAllECSByFrmDtAndToDt(frmDt.Day, frmDt.Day, frmDt, frmDt);

            if (ecsList.Count == 0)
            {
                response.Message = "No Records Found...!";
                response.Status = ServiceStatus.ValidationError;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var retStr = mandateCommonService.GenerateACHFile(ecsList, setDt);

                response.Status = ServiceStatus.Success;

                if (retStr == "")
                    response.Message = "ACH File generated successfully";
                else
                    response.Message = retStr;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
