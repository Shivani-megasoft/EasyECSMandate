using EASYECS.BussinessLayer.ECSService.Service;
using ECSMandate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EASYECS.BussinessLayer.ECSService.Object;
using ServiceObjects.Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using EasyBank.BusinessLayer.Transactions.Objects;
using EasyBank.Framework.EasyLogging;
using System.Diagnostics;
using EasyBank.Framework.EasyExceptions;

namespace ECSMandate.Controllers
{
    public class MandateResponseController : AppController
    {
        MandateService mandateService = new MandateService();

        public ActionResult Index()
        {
            ViewBag.FrmDate = DateTime.Now.Date.ToShortDateString();
            ViewBag.ToDate = DateTime.Now.Date.AddDays(1).ToShortDateString();



            var branchList = new Dictionary<string, string>();
            string BrnCd;
            BrnCd = StartUp.BRN_CD;
            StartUp.BRN_NAME = StartUp.BRN_LIST.Where(a => a.BrnCd == StartUp.BRN_CD).FirstOrDefault().Name;
            branchList.Add("All", "All");

            foreach (var branch in StartUp.BRN_LIST)
            {
                branchList.Add(branch.BrnCd, branch.BrnCdName);
            }

            ViewBag.BranchList = branchList.Select(b => new
            {
                Name = b.Value,
                Id = b.Key
            });

            var typeList = new Dictionary<string, string>();
            typeList.Add("All", "All");
            typeList.Add("Outward", "Outward");
            typeList.Add("Inward", "Inward");
            var TypeList = typeList.Select(b => new
            {
                Name = b.Value,
                Id = b.Key
            });
            ViewBag.TypeList = TypeList;
            return View();

        }



        public ActionResult GetMandateResponse(string Type, string FrmDate, string ToDate, string Branch)
        {
            Response response = new Response();
            List<MandateResModel> mndtResList = new List<MandateResModel>();

            try
            {
                DateTime frmDate = Convert.ToDateTime(FrmDate);
                DateTime toDate = Convert.ToDateTime(ToDate);

                var EBTCreditList = mandateService.GetAllMandteResponse(frmDate, toDate);

                if (Branch == "All")
                {
                    mndtResList = BindResModel(EBTCreditList);
                }
                else
                {
                    // Filter records based on the selected branch
                    if (EBTCreditList.Any())
                    {
                        EBTCreditList = EBTCreditList.Where(a => a.BenfBankAcNo.StartsWith(Branch)).ToList();
                        mndtResList = BindResModel(EBTCreditList);
                    }
                }

                return Json(new
                {
                    partialView = MvcHelpers.RenderPartialView(this, "_ResponseList", mndtResList, null)
                }, JsonRequestBehavior.AllowGet);
            }
            

            catch (System.Exception ex)
            {
                ViewBag.Error = "An error occurred while processing the request.";
                response.Status = ServiceStatus.Failure;
                EasyLogger.Log(ex.Message, "General", 1, TraceEventType.Error);
                //return View(ViewBag.Error);

                return View("~/Views/Shared/Error.cshtml");
            }

        }




        private List<MandateResModel> BindResModel(List<EBTCredit> EBTCreditList)

        {
            MandateService mandateService = new MandateService();
            var status = "";
            List<MandateResModel> mndtResList = new List<MandateResModel>();

            foreach (var ebtcredit in EBTCreditList)
            {
                ECS ecsmandate = new ECS();
                MandateResModel mndtResModel = new MandateResModel();
                if (ebtcredit == null) continue;
                ecsmandate = mandateService.GetByUmrn(ebtcredit.UMRN);
                mndtResModel.CBSDate = ebtcredit.ProcessDt;
                mndtResModel.UMRNNo = ebtcredit.UMRN;
                mndtResModel.DebtrName = ecsmandate.DebtrName;
                mndtResModel.BenAcNo = ebtcredit.BenfBankAcNo;
                mndtResModel.BenAcName = ebtcredit.BenfName;
                mndtResModel.InputFile = ebtcredit.InputFileNo;
                mndtResModel.InputDate = ebtcredit.RecdDt;
                mndtResModel.Amt = ebtcredit.Amt;
                mndtResModel.ResCd = ebtcredit.Rcode;
                switch (ebtcredit.Rcode.ToUpper())
                {
                    case "00":
                        status = "SUCCESS";
                        break;
                    case "01":
                        status = "ACC CLOSED OR TRANSFERRED";
                        break;
                    case "04":
                        status = "BALANCE INSUFFICIENT";
                        break;
                    case "05":
                        status = "NOT ARRANGED FOR";
                        break;
                    case "06":
                        status = "PAYMENT STOPPED BY DRAWER";
                        break;
                    case "07":
                        status = "PAYMENT STOPPED UNDER COURT ORDERACC UNDER LITIGATION";
                        break;
                    case "11":
                        status = "INVALID IFSCMICR CODE";
                        break;
                    case "12":
                        status = "MISMATCH IN MANDATE FREQUENCY";
                        break;
                    case "13":
                        status = "DUPLICATE TRN TRN ALREADY DEBITED EITHER UNDER ACH OR NACH DEBIT ECS";
                        break;
                    case "51":
                        status = "KYC DOCUMENTS PENDING";
                        break;
                    case "52":
                        status = "DOCUMENTS PENDING FOR ACC HOLDER TURNING MAJOR";
                        break;
                    case "53":
                        status = "AC INACTIVE NO TRNS FOR LAST 3 MONTHS";
                        break;
                    case "54":
                        status = "DORMANT AC NO TRNS FOR LAST 6 MONTHS";
                        break;
                    case "55":
                        status = "AC IN ZERO BALANCENO TRNS HAVE HAPPENED FIRST TRN IN CASH OR SELF CHEQUE";
                        break;
                    case "56":
                        status = "SIMPLE ACC FIRST TRN TO BE FROM BASE BRANCH";
                        break;
                    case "57":
                        status = "AMOUNT EXCEEDS LIMIT SET ON ACC BY BANK FOR DEBIT PER TRN";
                        break;
                    case "58":
                        status = "ACC REACHED MAXIMUM DEBI LIMIT SET ON ACC BY BANK";
                        break;
                    case "59":
                        status = "NETWORK FAILURE CBS";
                        break;
                    case "60":
                        status = "ACC HOLDER EXPIRED";
                        break;
                    case "61":
                        status = "MANDATE CANCELLED";
                        break;
                    case "68":
                        status = "AC BLOCKED OR FROZEN";
                        break;
                    case "69":
                        status = "CUSTOMER INSOLVENT INSANE";
                        break;
                    case "70":
                        status = "CUSTOMER TO REFER TO THE BRANCH";
                        break;
                    default:
                        status = "";
                        break;


                }

                mndtResModel.Status = status;
                mndtResList.Add(mndtResModel);
            }
            return mndtResList;
        }

        [HttpPost]
        public ActionResult GetExcelByDate(string Type, string FrmDate, string ToDate, string Branch)
        {
            int recordIndex = 1; int srno = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Excel = new ExcelPackage();

            var worksheet = Excel.Workbook.Worksheets.Add(" ECS_Response_REPORT_" + Convert.ToDateTime(FrmDate).ToShortDateString() + "_TO_" + Convert.ToDateTime(ToDate).ToShortDateString());
            worksheet.DefaultRowHeight = 20;
            worksheet.DefaultColWidth = 25;
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 1].Value = StartUp.BNK_NAME + "  Branch : " + "";
            worksheet.Cells[recordIndex, 1, recordIndex, 11].Merge = true;
            worksheet.Row(recordIndex).Style.Font.Bold = true;

            //recordIndex++;
            //worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //worksheet.Row(recordIndex).Style.Font.Bold = true;
            //worksheet.Cells[recordIndex, 1].Value = "Reserve List";
            //worksheet.Cells[recordIndex, 1, recordIndex, 11].Merge = true;
            //worksheet.Row(recordIndex).Style.Font.Bold = true;

            recordIndex++;
            worksheet.Row(recordIndex).Style.Font.Bold = true;
            worksheet.Cells[recordIndex, 1].Value = "Sr.No.";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[recordIndex, 2].Value = "CBS Date";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 3].Value = "UMRN No.";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 4].Value = "Debitors Name";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


            worksheet.Cells[recordIndex, 5].Value = "Ben AcNo.";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            worksheet.Cells[recordIndex, 6].Value = "Ben Name";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 7].Value = "Input File";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            worksheet.Cells[recordIndex, 8].Value = "Input Date";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            worksheet.Cells[recordIndex, 9].Value = "Amount";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 10].Value = "RsnCd";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 11].Value = "Reason";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            Response response = new Response();
            List<MandateAckModel> mndtAckList = new List<MandateAckModel>();
            DateTime frmDate = Convert.ToDateTime(FrmDate);
            DateTime toDate = Convert.ToDateTime(ToDate);

           
            List<MandateResModel> mndtResList = new List<MandateResModel>();
            var EBTCreditList = mandateService.GetAllMandteResponse(frmDate, toDate);
            if (Branch == "All")
            {
                mndtResList = BindResModel(EBTCreditList);
            }
            else
            {
                // Filter records based on the selected branch
                if (EBTCreditList.Any())
                {
                    EBTCreditList = EBTCreditList.Where(a => a.BenfBankAcNo.StartsWith(Branch)).ToList();
                    mndtResList = BindResModel(EBTCreditList);
                }
            }

            foreach (var mndtRes in mndtResList)
            {

                srno++;
                recordIndex++;
                worksheet.Cells[recordIndex, 1].Value = srno;
                worksheet.Cells[recordIndex, 2].Value = Convert.ToDateTime(mndtRes.CBSDate).ToShortDateString();
                worksheet.Cells[recordIndex, 3].Value = mndtRes.UMRNNo;
                worksheet.Cells[recordIndex, 4].Value = mndtRes.DebtrName;
                worksheet.Cells[recordIndex, 5].Value = mndtRes.BenAcNo; ;
                worksheet.Cells[recordIndex, 6].Value = mndtRes.BenAcName;
                worksheet.Cells[recordIndex, 7].Value = mndtRes.InputFile;
                worksheet.Cells[recordIndex, 8].Value = Convert.ToDateTime(mndtRes.InputDate).ToShortDateString();
                worksheet.Cells[recordIndex, 9].Value = mndtRes.Amt;
                worksheet.Cells[recordIndex, 10].Value = mndtRes.ResCd;
                worksheet.Cells[recordIndex, 11].Value = mndtRes.Status;

            }
            string excelFileName = "ECS_Response_Report_" + Convert.ToDateTime(FrmDate).ToShortDateString() + "_TO_" + Convert.ToDateTime(ToDate).ToShortDateString() + ".xlsx";
            Excel.Workbook.Properties.Title = "Attempts";
            System.Web.HttpResponse Response = System.Web.HttpContext.Current.Response;
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", string.Format("attachment;  filename={0}", excelFileName));
            Response.BinaryWrite(Excel.GetAsByteArray());
            Response.Flush();
            Response.End();
            return View();
        }
    }
}