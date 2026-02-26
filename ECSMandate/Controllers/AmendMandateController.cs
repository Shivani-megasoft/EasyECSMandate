using EasyBank.Infrastructure.SystemParameters;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.BussinessLayer.ECSService.Object;
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
        MandateAuditLogService mandateAuditLogService = new MandateAuditLogService();
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
                    // response.Item = mandate;
                    response.Item = new
                    {
                        Mandate = mandate,
                        UMRDt = mandate.UMRDt == null ? string.Empty : Convert.ToDateTime(mandate.UMRDt).ToString("dd/MM/yyyy"),
                        UtilityCd = mandate.UtilityCd,
                        UtilityName = mandate.UtilityName,
                        AcTyp = mandate.AcTyp,
                        Freq = mandate.Freq,
                        DebitTyp = mandate.DebitTyp,
                        RefNo1 = mandate.RefNo1,
                        RefNo2 = mandate.RefNo2,
                        EMail = mandate.EMail,
                        RsnList = rsnListNew
                    };

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
            ECSHistory ecsHistory = new ECSHistory();

            if (AmendReason == "")
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select reason!";
                return Json(response);
            }
            //var ecsHistory = mandateCommonService.
            var mandate = mandateService.GetByUmrn(ecsMandate.UMRNo);
            ecsHistory = CommonService.Cast<ECSHistory>(mandate);

            ecsHistory.UMRDt = ecsMandate.UMRDt;
            ecsHistory.SpnsrBnkCd = ecsMandate.SpnsrBnkCd;
            ecsHistory.UtilityCd = ecsMandate.UtilityCd;
            ecsHistory.UtilityName = ecsMandate.UtilityName;
            ecsHistory.AcTyp = ecsMandate.AcTyp;
            ecsHistory.AcNo = ecsMandate.AcNo;
            ecsHistory.BankName = ecsMandate.BankName;
            ecsHistory.BnkCd = ecsMandate.BnkCd;
            ecsHistory.Amt = ecsMandate.Amt;
            ecsHistory.Freq = ecsMandate.Freq;
            ecsHistory.RefNo1 = ecsMandate.RefNo1;
            ecsHistory.RefNo2 = ecsMandate.RefNo2;
            ecsHistory.PhoneNo = ecsMandate.PhoneNo;
            ecsHistory.EMail = ecsMandate.EMail;
            ecsHistory.FrmPrd = ecsMandate.FrmPrd;
            ecsHistory.ToPrd = ecsMandate.ToPrd;

            ecsHistory.TrnInd = "S";
            ecsHistory.RtnRsn = AmendReason;
            ecsHistory.Clerk = StartUp.LOGIN_ID;
            ecsHistory.DtStamp = DateTime.Now;
            ecsHistory.MandateStatus = ECS.AMEND;
            mandateService.UpdateEcsHistory(null, ecsHistory);

            response.Message = "Mandate Amend Successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }
        public ActionResult GetMandateById(int Id)
        {
            Response response = new Response();

            var mandateHistory = new MandateService().GetMandateHistoryById(Id);

            if (mandateHistory == null)
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Mandate not found";
                return Json(response);

            }
            if (mandateHistory.Clerk == startUp.LOGIN_ID)
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Same user cannot authenticate mandate";
                return Json(response);
            }
            response.Item = new
            {
                Mandate = mandateHistory,
                UMRDt = mandateHistory.UMRDt == null ? string.Empty : Convert.ToDateTime(mandateHistory.UMRDt).ToString("dd/MM/yyyy"),
                UtilityCd = mandateHistory.UtilityCd,
                UtilityName = mandateHistory.UtilityName,
                AcTyp = mandateHistory.AcTyp,
                Freq = mandateHistory.Freq,
                DebitTyp = mandateHistory.DebitTyp,
                RefNo1 = mandateHistory.RefNo1,
                RefNo2 = mandateHistory.RefNo2,
                EMail = mandateHistory.EMail,
                RtnRsn = mandateHistory.RtnRsn,
            };
            response.Status = ServiceStatus.Success;
            return Json(response);
        }


        public ActionResult AuthenticateAmend(int Id)
        {
            Response response = new Response();
            ECSAuditLog auditLog = new ECSAuditLog();
            IStartUp startUp = StartupManager.StartUp;
            ECS ecs = new ECS();
            ECSHistory eCSHistory = new ECSHistory();

            var existmandate = mandateService.GetById(Id);

            var jsonResult = Json(response, JsonRequestBehavior.AllowGet);

            eCSHistory = mandateService.GetMandateHistoryById(Id);

            auditLog.DATA = mandateAuditLogService.BuildData(existmandate, eCSHistory);
            auditLog.DATE_ = DateTime.Now;
            auditLog.KEY = existmandate.Id;
            auditLog.BRNCD = startUp.BRN_CD;
            auditLog.UMRNO = existmandate.UMRNo;
            auditLog.AUTHUSER = startUp.LOGIN_ID;
            auditLog.ENTITY = "ebECSMandate";
            ecs = CommonService.Cast<ECS>(eCSHistory);
            ecs.Id = existmandate.Id;
            ecs.IsNew = false;
            ecs.DtStamp = DateTime.Now;
            ecs.TrnInd = "A";
            ecs.AuthUser = StartUp.LOGIN_ID;
            ecs.CredtrAgentName = startUp.BNK_NAME;
            ecs.Status = ECS.AMEND;
            eCSHistory.TrnInd = "A";
            eCSHistory.Status = ECS.AMEND;

            try
            {
                var doc = mandateCommonService.MandateAmendXML(ecs);// AmendReason);
                mandateCommonService.CreateOutputZip(ecs, doc, ECS.AMEND);
                mandateService.Update(ecs, eCSHistory, auditLog);
                response.Status = ServiceStatus.Success;
                response.Message = "Amend mandate authenticate successfully";
           }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Status = ServiceStatus.Failure;
                return jsonResult;
            }

            jsonResult.MaxJsonLength = Int32.MaxValue;
            return jsonResult;
        }


        private void SetViewBag()
        {
            var typList = MandateCommonService.GetAllAcTyp();
            var freqList = MandateCommonService.GetAllFreq();
            var drTypList = MandateCommonService.GetAllDrTyp();
            var rsnList = mandateService.GetAllReason(ECS.AMEND, "All");
            var mandateList = mandateService.GetECSHistoryByTrnInd("S", "OW").Where(a => a.MandateStatus == ECS.AMEND).ToList();
            //var mandateList = mandateService.GetUnAuthMandate("OW").Where(m => m.MandateStatus == ECS.AMEND).ToList();
            ViewBag.MandateList = mandateList;
            ViewBag.ReasonList = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd });

            ViewBag.AcTypList = typList;
            ViewBag.FrqList = freqList;
            ViewBag.DrTypList = drTypList;
        }
    }
}