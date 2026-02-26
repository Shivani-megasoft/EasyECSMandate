using EasyBank.BusinessLayer.Common.Services;
using EasyBank.Infrastructure.SystemParameters;
using EasyBank.UI.Controllers.Common.Extensions;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class CancelMandateController : AppController
    {
        MandateService mandateService = new MandateService();
        MandateCommonService mandateCommonService = new MandateCommonService();
        IStartUp startUp = StartupManager.StartUp;

        // GET: CancelMandate
        public ActionResult Index()
        {
            SetViewBag();
            //var unAuthECSList = new UnAuthEntityService().GetUnAuthEntityByEntity(StartUp.BRN_CD, new ECS().GetType().Name);
            //ViewBag.UnAuthECSList = unAuthECSList.Select(s => new { Id = s.Id, Entity = s.EntityDetails });
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
                if (mandate.MandateTyp == "OW" && mandate.MandateStatus != ECS.CANCEL)
                {
                    var rsnList = mandateService.GetAllReason(ECS.CANCEL, mandate.Variant);
                    var rsnListNew = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd });

                    response.Item = new
                    {
                        Mandate = mandate,
                        UMRDt = mandate.UMRDt == null ? string.Empty : mandate.UMRDt.ToShortDateString(),
                        RsnList = rsnListNew
                    };
                    response.Status = ServiceStatus.Success;
                    return Json(response);
                }
                if (mandate.MandateStatus == "Cancel")
                {
                    response.Message = "Mandate Already Cancel";
                    response.Status = ServiceStatus.ValidationError;
                    return Json(response);
                }
            }

            response.Message = "Mandate Not Found";
            response.Status = ServiceStatus.ValidationError;
            return Json(response);
        }

        public ActionResult UpdateMandate(string UMRN, string RtnRsn)
        {
            Response response = new Response();
            var mandate = mandateService.GetByUmrn(UMRN);

            if (RtnRsn == "")
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select reason to cancel";
                return Json(response);
            }

            mandate.MandateStatus = ECS.CANCEL;
            mandate.RtnRsn = RtnRsn;
            mandate.TrnInd = "S";
            mandateService.Update(mandate);
            response.Message = "Mandate Cancel request save successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }

        public ActionResult CancelMandate(string UMRN, string RtnRsn)
        {
            Response response = new Response();
            ECSHistory ecsHistory = new ECSHistory();
            var ecs = mandateService.GetByUmrn(UMRN);
            if (RtnRsn == "")
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select reason to cancel";
                return Json(response);
            }
            ecsHistory = CommonService.Cast<ECSHistory>(ecs);
            ecsHistory.TrnInd = "S";
            ecsHistory.RtnRsn = RtnRsn;
            ecsHistory.Clerk = StartUp.LOGIN_ID;
            ecsHistory.DtStamp = DateTime.Now;
            mandateService.Update(ecsHistory);
            response.Message = "Mandate Cancel request save successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }


        public ActionResult ShowAllUnAuth()
        {
            Response response = new Response();
            var unAuthATMList = new UnAuthEntityService().GetUnAuthEntityByEntity(StartUp.BRN_CD, new ECS().GetType().Name);

            response.Item = this.RenderPartialView("_UnAuthEntityList", unAuthATMList, "TransBackOffice");
            return Json(response);
        }
        public ActionResult AuthenticateCancel(int Id)
        {
            Response response = new Response();
            var mandate = mandateService.GetById(Id);
            var mandateHistory = mandateService.GetMandateHistoryById(Id);

            mandate.TrnInd = "A";
            mandate.AuthUser = StartUp.LOGIN_ID;
            mandate.RtnRsn = mandateHistory.RtnRsn;
            mandate.Status = ECS.CANCEL;
            mandateHistory.TrnInd = "A";
            mandateHistory.Status = ECS.CANCEL;
            var doc = mandateCommonService.MandateCancelXML(mandate);// AmendReason);
            mandateCommonService.CreateOutputZip(mandate, doc, ECS.CANCEL);
            mandateService.UpdateEcsHistory(mandate, mandateHistory);
            response.Status = ServiceStatus.Success;
            response.Message = "Cancel mandate authenticate successfully";
            return Json(response);
        }
        public ActionResult GetMandateById(int Id)
        {
            Response response = new Response();
            ECSHistory mandate = new ECSHistory();
            mandate = new MandateService().GetMandateHistoryById(Id);
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
        private void SetViewBag()
        {
            var typList = MandateCommonService.GetAllAcTyp();
            var freqList = MandateCommonService.GetAllFreq();
            var drTypList = MandateCommonService.GetAllDrTyp();
            var rsnList = mandateService.GetAllReason(ECS.CANCEL, "All");
            var mandateList = mandateService.GetECSHistoryByTrnInd("S", "OW");
            ViewBag.MandateList = mandateList;
            ViewBag.ReasonList = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd });
            ViewBag.AcTypList = typList;
            ViewBag.FrqList = freqList;
            ViewBag.DrTypList = drTypList;
        }
    }
}