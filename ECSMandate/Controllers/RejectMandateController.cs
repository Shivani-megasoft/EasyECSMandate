using EasyBank.BusinessLayer.Accounts.Objects;
using EasyBank.BusinessLayer.Accounts.Services;
using EasyBank.BusinessLayer.Transactions.Services;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSMandate.Models;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using EasyBank.UI.Controllers.Transactions;
using EasyBank.BusinessLayer.Transactions.Objects;
using EasyBank.BusinessLayer.HOAdmin.Contracts;
using EasyBank.BusinessLayer.Transactions.Services;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EasyBank.BusinessLayer.Transactions.Contracts;
using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.Infrastructure.SystemParameters;
using EasyBank.BusinessLayer.Accounts.Contracts;
using EasyBank.Framework.EasyPrinting;

namespace ECSMandate.Controllers
{
    public class RejectMandateController : AppController
    {
        MandateService mandateService = new MandateService();
        // GET: VerifyMandate
        public ActionResult Index()
        {
            Response response = new Response();
            //SetViewBag();
            return View();
        }
      

        public ActionResult GetRejectMandate(string mandatetyp, string MandateStatus, string FrmDate, string ToDate)
        {
            var ecsReturnList = new MandateService().GetAllECSByStatusandDT("IW", "Rejected", Convert.ToDateTime(FrmDate), Convert.ToDateTime(ToDate));
       
            foreach (var ecs in ecsReturnList)
            {
                if (ecs.IsAccept == false && ecs.Reason == "")
                {
                    ecs.MandateStatus = "Pending";
                }
                else if (ecs.IsAccept == false && ecs.Reason != "")
                {
                    ecs.MandateStatus = "Rejected";
                }
            }


            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_ECSRejectList", ecsReturnList, null) }, JsonRequestBehavior.AllowGet);
        }





        public ActionResult UpdateMandate(string selectedRecordId)
        {
            Response response = new Response();

            if (string.IsNullOrWhiteSpace(selectedRecordId))
            {
                response.Status = ServiceStatus.Failure;
                response.Message = "No record selected.";
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            string CloseTimeStr = ConfigurationManager.AppSettings["NPCI_Close_Time"];
            TimeSpan npciCloseTime;
            if (!TimeSpan.TryParse(CloseTimeStr, out npciCloseTime))
            {
                response.Status = ServiceStatus.Failure;
                response.Message = "Invalid time format in web config.";
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            var mandate = mandateService.GetById(Convert.ToInt32(selectedRecordId));
            if (mandate == null)
            {
                response.Status = ServiceStatus.Failure;
                response.Message = "Mandate not found.";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
           
                //bool isAccp = false;
            bool isAccp = mandate.RtnRsn == "ac01";
            mandate.TrnInd = isAccp ? "A" : "R";
            mandate.MandateStatus = isAccp ? "Auth" : "Return";
            mandate.Operation = "Update";
            mandate.ReqDt = DateTime.Now;
            mandate.AuthUser = StartUp.LOGIN_ID;
            mandate.AuthDt = StartUp.DAY_OP_DT;
            mandate.DtStamp = DateTime.Now;
            mandate.ExecDt = DateTime.Now;

            // Process mandate acceptance and generate response file
            Account account = null;
                string brnCd = "", glcd = "", acno = "";
                if (mandate.AcNo.Length == 15)
                {
                    brnCd = mandate.AcNo.Substring(0, 4);
                    glcd = mandate.AcNo.Substring(4, 4);
                    acno = mandate.AcNo.Substring(8, 7);
                }
                account = AccountService.GetByNo(brnCd + glcd + acno);

                //if (isAccp && account == null)
                //{
                //    response.Status = ServiceStatus.ValidationError;
                //    response.Message = "Account not found";
                //    return Json(response, JsonRequestBehavior.AllowGet);
                //}

                new MandateCommonService().GenerateMandateXML(mandate, isAccp);
                mandate.DtStamp = DateTime.Now;
                mandate.AuthDt = DateTime.Now;
                mandate.ExecDt = DateTime.Now;
                mandate.Status = null;
                mandate.Reason = null;


                mandateService.Update(mandate);

                if (isAccp && account != null)
                {
                    if (account.Customer.Address.Any(a => !string.IsNullOrEmpty(a.Mobile)))
                    {
                        string mobileNo = account.Customer.Address.FirstOrDefault(a => !string.IsNullOrEmpty(a.Mobile)).Mobile;
                        if (!string.IsNullOrWhiteSpace(mobileNo))
                        {
                            try
                            {
                                SMS sms = new SMS
                                {
                                    AcId = account.Id,
                                    MobNo = mobileNo,
                                    Amt = mandate.Amt,
                                    Name = account.Customer.Name,
                                    AcNo = mandate.AcNo,
                                    reason = ""
                                };
                                new SMSService().SendSms(sms, "ECSAcptIW");
                            }
                            catch (Exception ex)
                            {
                                // Handle exception
                            }
                        }
                    }
                }
            
            response.Message = "Mandate Regenerate Successfully";
            response.Status = ServiceStatus.Success;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

    }




}