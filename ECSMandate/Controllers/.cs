using EasyBank.Infrastructure.SystemParameters;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class AmendMandateController : AppController
    {
        MandateService mandateService = new MandateService();
        MandateCommonService mandateCommonService = new MandateCommonService();
        IStartUp startUp = StartupManager.StartUp;
        // GET: AmendMandate
        public ActionResult Index()
        {
            SetViewBag();
            return View();
        }

        public ActionResult GetMandateByUMRN(string UMRN)
        {
            Response response = new Response();
            var mandate = mandateService.GetByUmrn(UMRN);
            SetViewBag();
            bool stat = false;
            if (mandate != null)
            {
                if (mandate.MandateTyp == "OW" && mandate.MandateStatus != "Amend")
                {
                    var rsnList = mandateService.GetAllReason("Amend", mandate.Variant);
                    var rsnListNew = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd });
                    response.Item = mandate;
                    response.Status = ServiceStatus.Success;
                    return Json(response);
                }
                if (mandate.MandateStatus == "Amend")
                {
                    response.Message = "Mandate Already Amend";
                    response.Status = ServiceStatus.Failure;
                    return Json(response);
                }
            }

            response.Message = "Mandate Not Found";
            response.Status = ServiceStatus.Failure;
            return Json(response);
        }

        public ActionResult UpdateMandate(ECS ecsMandate, string AmendReason)
        {
            Response response = new Response();
            if (AmendReason == "")
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select reason!";
                return Json(response);
            }

            var mandate = mandateService.GetByUmrn(ecsMandate.UMRNo);
            //mandate.IsUnitCancel = true;
            mandate.MandateStatus = ECS.AMEND;
            mandate.UMRDt = ecsMandate.UMRDt;
            mandate.SpnsrBnkCd = ecsMandate.SpnsrBnkCd;
            mandate.UtilityCd = ecsMandate.UtilityCd;
            mandate.UtilityName = ecsMandate.UtilityName;
            mandate.AcTyp = ecsMandate.AcTyp;
            mandate.AcNo = ecsMandate.AcNo;
            mandate.BankName = ecsMandate.BankName;
            mandate.BnkCd = ecsMandate.BnkCd;
            mandate.Amt = ecsMandate.Amt;
            mandate.Freq = ecsMandate.Freq;
            mandate.RefNo1 = ecsMandate.RefNo1;
            mandate.RefNo2 = ecsMandate.RefNo2;
            mandate.PhoneNo = ecsMandate.PhoneNo;
            mandate.EMail = ecsMandate.EMail;
            mandate.FrmPrd = ecsMandate.FrmPrd;
            mandate.ToPrd = ecsMandate.ToPrd;
            mandate.TrnInd = "S";
            mandate.MandateStatus = ECS.AMEND;
            mandate.Clerk = startUp.LOGIN_ID;
            mandate.RtnRsn = AmendReason;

            mandateService.Update(mandate);

            response.Message = "Mandate Cancel Successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }
        public ActionResult GetMandateById(int Id)
        {
            Response response = new Response();

            var mandate = new MandateService().GetById(Id);

            if (mandate == null)
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Mandate not found";
                return Json(response);

            }
            if (mandate.Clerk == startUp.LOGIN_ID)
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Same user cannot authenticate mandate";
                return Json(response);
            }
            response.Item = mandate;
            response.Status = ServiceStatus.Success;
            return Json(response);
        }

        public ActionResult AuthenticateAmend(int Id)
        {
            Response response = new Response();
            IStartUp startUp = StartupManager.StartUp;

            var mandate = mandateService.GetById(Id);

            mandate.TrnInd = "A";
            mandate.AuthUser = StartUp.LOGIN_ID;

            mandate.CredtrAgentName = startUp.BNK_NAME;

            var doc = mandateCommonService.MandateAmendXML(mandate);// AmendReason);

            mandateCommonService.CreateOutputZip(mandate, doc, ECS.AMEND);
            mandateService.Update(mandate);
            response.Status = ServiceStatus.Success;
            response.Message = "Amend mandate authenticate successfully";
            return Json(response);
        }

        private void SetViewBag()
        {
            var typList = MandateCommonService.GetAllAcTyp();

            var freqList = MandateCommonService.GetAllFreq();
            var drTypList = MandateCommonService.GetAllDrTyp();

            var rsnList = mandateService.GetAllReason(ECS.AMEND, "ALL");
            var mandateList = mandateService.GetUnAuthMandate("OW").Where(m => m.MandateStatus == ECS.AMEND).ToList();
            ViewBag.MandateList = mandateList;
            ViewBag.ReasonList = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd });

            ViewBag.AcTypList = typList;
            ViewBag.FrqList = freqList;
            ViewBag.DrTypList = drTypList;
        }
    }
}