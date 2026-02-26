using EasyBank.BusinessLayer.Base.Services;
using EasyBank.Infrastructure.SystemParameters;
using EasyBank.UI.Controllers;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class ACHInputFileController : AppController
    {
        public IEnumerable<ECS> ECSList { get; private set; }

        // GET: ACHInputFile
        public ActionResult Index()
        {
            ViewBag.ForDate = DateTime.Now.Date.ToShortDateString();
            ViewBag.ToDate = DateTime.Now.Date.ToShortDateString();
            ViewBag.SetDate = DateTime.Now.Date.AddDays(1).ToShortDateString();
            return View();
        }

        public ActionResult GenerateACHFile(string FromDate, string ToDate, string SetDate)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(FromDate))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select from date";
                return Json(response);
            }
            if (string.IsNullOrEmpty(ToDate))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select to date";
                return Json(response);
            }
            if (string.IsNullOrEmpty(SetDate))
            {
                response.Message = "Please select settlement date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response);
            }

            MandateCommonService mandateCommonService = new MandateCommonService();
            DateTime frmDt = Convert.ToDateTime(FromDate);
            DateTime toDt = Convert.ToDateTime(ToDate);
            DateTime setDt = Convert.ToDateTime(SetDate);

            if (frmDt > toDt)
            {
                response.Message = "From Date should be less than To Date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response);
            }
            if (setDt < frmDt)
            {
                response.Message = "Settlement Date should be greater than from Date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response);
            }

            var ecsList = new MandateService().GetAllECSByFrmDtAndToDt(frmDt.Day, toDt.Day, frmDt, toDt);
           

            if (ecsList.Count == 0)
            {
                response.Message = "No Records Found...!";
                response.Status = ServiceStatus.ValidationError;
                return Json(response);
            }
            else
            {
                var retStr = mandateCommonService.GenerateACHFile(ecsList, setDt);

                response.Status = ServiceStatus.Success;

                if (retStr == "")
                    response.Message = "ACH File generated successfully";
                else
                    response.Message = retStr;
                return Json(response);
            }
        }
        // GET: FileSummary
        public ActionResult ViewACHFile(string FromDate, string ToDate, string SetDate)
        {
            MandateCommonService mandateCommonService = new MandateCommonService();
            List<MandateInstruction> mandateInstructionList = new List<MandateInstruction>();


            Response response = new Response();
           
            if (string.IsNullOrEmpty(FromDate))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select from date";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(ToDate))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select to date";
                return Json(response, JsonRequestBehavior.AllowGet);
            } 
            if (string.IsNullOrEmpty(SetDate))
            {
                response.Message = "Please select settlement date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            DateTime frmDt = Convert.ToDateTime(FromDate);
            DateTime toDt = Convert.ToDateTime(ToDate);
            DateTime setDt = Convert.ToDateTime(SetDate);

            if (frmDt > toDt)
            {
                response.Message = "From Date should be less than To Date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            if (setDt < frmDt)
            {
                response.Message = "Settlement Date should be greater than from Date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            // var ecsList = new MandateService().GetAllUnExecutedECS(Convert.ToDateTime(SetDate));
            try
            {
                mandateInstructionList = new MandateService().GetAllECSByFrmDtAndToDt(frmDt.Day, toDt.Day, frmDt, toDt);
            }
            catch(Exception ex)
            {

            }
         

            if (mandateInstructionList.Count ==0 || mandateInstructionList ==  null)
            {
                response.Message = "No records for posting.!";
                response.Status = ServiceStatus.Failure;
                //return Json(response);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            var todayDay = DateTime.Now;

            List<ECS> ecsList = new List<ECS>();
            foreach (MandateInstruction Instr in mandateInstructionList)
            {
                ECS ecs = new ECS();

                ecs = new MandateService().GetById(Instr.MNDTID);
                Instr.REFNO = ("REF" + todayDay.Day.ToString().PadLeft(2, '0') + todayDay.Month.ToString().PadLeft(2, '0') + todayDay.Year.ToString().PadLeft(2, '0') + todayDay.Hour.ToString().PadLeft(2, '0') + todayDay.Minute.ToString().PadLeft(2, '0') + todayDay.Second.ToString().PadLeft(2, '0')).PadRight(30, ' ');
                ecs.RefNo2 = Instr.REFNO;
                ecsList.Add(ecs);
            }
            //response.Status = ServiceStatus.Success;
            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_ACHInputList", ecsList, null) }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult ViewGenACHFile(string FromDate, string ToDate, string SetDate)
        {
            MandateCommonService mandateCommonService = new MandateCommonService();
            List<MandateInstruction> mandateInstructionList = new List<MandateInstruction>();


            Response response = new Response();

            if (string.IsNullOrEmpty(FromDate))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select from date";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(ToDate))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select to date";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(SetDate))
            {
                response.Message = "Please select settlement date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            DateTime frmDt = Convert.ToDateTime(FromDate);
            DateTime toDt = Convert.ToDateTime(ToDate);
            DateTime setDt = Convert.ToDateTime(SetDate);

            if (frmDt > toDt)
            {
                response.Message = "From Date should be less than To Date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            if (setDt < frmDt)
            {
                response.Message = "Settlement Date should be greater than from Date";
                response.Status = ServiceStatus.ValidationError;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            // var ecsList = new MandateService().GetAllUnExecutedECS(Convert.ToDateTime(SetDate));
            try
            {
                mandateInstructionList = new MandateService().GetAllECSByINPFILENAME(frmDt.Day, toDt.Day, frmDt, toDt);
            }
            catch (Exception ex)
            {

            }


            if (mandateInstructionList.Count == 0 || mandateInstructionList == null)
            {
                response.Message = "No records for posting.!";
                response.Status = ServiceStatus.Failure;
                //return Json(response);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            var todayDay = DateTime.Now;

            List<ECS> ecsList = new List<ECS>();
            foreach (MandateInstruction Instr in mandateInstructionList)
            {
                ECS ecs = new ECS();

                ecs = new MandateService().GetById(Instr.MNDTID);
                Instr.REFNO = ("REF" + todayDay.Day.ToString().PadLeft(2, '0') + todayDay.Month.ToString().PadLeft(2, '0') + todayDay.Year.ToString().PadLeft(2, '0') + todayDay.Hour.ToString().PadLeft(2, '0') + todayDay.Minute.ToString().PadLeft(2, '0') + todayDay.Second.ToString().PadLeft(2, '0')).PadRight(30, ' ');
                ecs.RefNo2 = Instr.REFNO;
                ecsList.Add(ecs);
            }
            //response.Status = ServiceStatus.Success;
            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_ACHInputList", ecsList, null) }, JsonRequestBehavior.AllowGet);
        }


    }
}