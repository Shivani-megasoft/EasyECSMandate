using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.Service;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class FileSummaryController : AppController
    {
        // GET: FileSummary
        MandateService mandateService = new MandateService();
        public ActionResult Index()
        {
            ViewBag.StatusList = MandateCommonService.GetMandateStatus();
            ViewBag.FrmDate = DateTime.Now.Date.ToShortDateString();
            ViewBag.ToDate = DateTime.Now.Date.ToShortDateString();


            //var branchList = new Dictionary<string, string>();
            //string BrnCd;
            //BrnCd = StartUp.BRN_CD;
            //StartUp.BRN_NAME = StartUp.BRN_LIST.Where(a => a.BrnCd == StartUp.BRN_CD).FirstOrDefault().Name;
            //branchList.Add("", "---Select---");

            //foreach (var branch in StartUp.BRN_LIST)
            //{
            //    branchList.Add(branch.BrnCd, branch.BrnCdName);
            //}

            //ViewBag.BranchList = branchList.Select(b => new
            //{
            //    Name = b.Value,
            //    Id = b.Key
            //});
            return View();

        }

        public ActionResult GetECSByStatus(string status, string FrmDate, string ToDate)
        {
            var ecsReturnList = new MandateService().GetAllECSByStatus(status, Convert.ToDateTime(FrmDate), Convert.ToDateTime(ToDate));
            List<ECS> ecsList = new List<ECS>();
            
           // var manHistoryList = mandateService.GetAllReqMandte(ECS.ACCEPT, Convert.ToDateTime( FrmDate), Convert.ToDateTime(ToDate));
            foreach (ECS ecs in ecsReturnList)
            {                
                //var mandateHistory = manHistoryList.Where(a => a.Id == ecs.Id).FirstOrDefault();
                //if(mandateHistory != null)
                {
                    string ackMndt;
                    string rsncd = "";
                    
                    //if (ecs.MandateTyp == ECS.CREATE)
                    //    ackMndt = ECS.CREATE_ACK;
                    //else
                    //    ackMndt = ECS.ACCEPT_ACK;

                    //var ack = mandateService.GetAckMandte(ecs.Id, ackMndt, mandateHistory.MandateFile);
                    //if (ack != null)
                   // {
                        //ecs.Status = ecs.IsAccept == true ? "Accepted" : "Decline";
                        //ecs.Reason = ack.Reason;
                        var ReturnReason = new MandateService().GetReasonByReasonCode(ecs.RtnRsn);
                        foreach( var rtn in ReturnReason)
                        {
                            var Reason = rtn.Reason;
                            ecs.RtnRsn = ecs.RtnRsn + " " + Reason;
                        }                       
                    //}
                }
                 ecsList.Add(ecs);
            }
            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_ECSStatList", ecsList, null) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateExcel(string MandateStatus, string FrmDate, string ToDate, string Branch)
        {
            int recordIndex = 1;
            int srno = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Excel = new ExcelPackage();

            var worksheet = Excel.Workbook.Worksheets.Add(" ECS_DETAILS_REPORT_" + Convert.ToDateTime(FrmDate).ToShortDateString() + "_TO_" + Convert.ToDateTime(ToDate).ToShortDateString());
            worksheet.DefaultRowHeight = 20;
            worksheet.DefaultColWidth = 20;
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 1].Value = StartUp.BNK_NAME + "  Branch : " + "";
            worksheet.Cells[recordIndex, 1, recordIndex, 11].Merge = true;
            worksheet.Row(recordIndex).Style.Font.Bold = true;

            recordIndex++;
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Row(recordIndex).Style.Font.Bold = true;
            worksheet.Cells[recordIndex, 1].Value = "ECS_DETAILS_REPORT";
            worksheet.Cells[recordIndex, 1, recordIndex, 11].Merge = true;
            worksheet.Row(recordIndex).Style.Font.Bold = true;

            recordIndex++;
            worksheet.Row(recordIndex).Style.Font.Bold = true;
            worksheet.Cells[recordIndex, 1].Value = "TYPE";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[recordIndex, 1, recordIndex, 1].Merge = true;

            worksheet.Cells[recordIndex, 2].Value = "ACCOUNT NO";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 2, recordIndex, 2].Merge = true;

            worksheet.Cells[recordIndex, 3].Value = "NPCI FILE";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[recordIndex, 3, recordIndex, 3].Merge = true;

            worksheet.Cells[recordIndex, 4].Value = "MANDATE_FILE";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[recordIndex, 4, recordIndex, 4].Merge = true;

            worksheet.Cells[recordIndex, 5].Value = "UMRN NO";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[recordIndex, 5, recordIndex, 5].Merge = true;

            worksheet.Cells[recordIndex, 6].Value = "AMOUNT";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[recordIndex, 6, recordIndex, 6].Merge = true;

            worksheet.Cells[recordIndex, 7].Value = "UMRN DATE";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[recordIndex, 7, recordIndex, 7].Merge = true;

            worksheet.Cells[recordIndex, 8].Value = "STATUS";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 8, recordIndex, 8].Merge = true;

            worksheet.Cells[recordIndex, 9].Value = "AUTH BY";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 9, recordIndex, 9].Merge = true;

            worksheet.Cells[recordIndex, 10].Value = "DATE";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 10, recordIndex, 10].Merge = true;

            worksheet.Cells[recordIndex, 11].Value = "Debitor Name";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 11, recordIndex, 11].Merge = true;

            worksheet.Cells[recordIndex, 12].Value = "Return Reason";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 12, recordIndex, 12].Merge = true;

            var ecsList = new MandateService().GetAllECSByStatus("ALL", Convert.ToDateTime(FrmDate), Convert.ToDateTime(ToDate));
            string rsncd = "";

            foreach (ECS ecs in ecsList)
            {
                var ReturnReason = new MandateService().GetReasonByReasonCode(ecs.RtnRsn);
              
                    srno++; recordIndex++;
                    if (ecs.MandateTyp == "IW")
                        worksheet.Cells[recordIndex, 1].Value = "INWARD";
                    else
                        worksheet.Cells[recordIndex, 1].Value = "OUTWARD";
                    worksheet.Cells[recordIndex, 2].Value = ecs.AcNo;
                    worksheet.Cells[recordIndex, 3].Value = ecs.NPCIFile;
                    worksheet.Cells[recordIndex, 4].Value = ecs.MandateFile;
                    worksheet.Cells[recordIndex, 5].Value = ecs.UMRNo;
                    worksheet.Cells[recordIndex, 6].Value = ecs.Amt;
                    worksheet.Cells[recordIndex, 7].Value = Convert.ToDateTime(ecs.UMRDt).ToShortDateString();
                    worksheet.Cells[recordIndex, 8].Value = ecs.Status;
                    worksheet.Cells[recordIndex, 9].Value = ecs.AuthUser;
                    worksheet.Cells[recordIndex, 10].Value = ecs.DtStamp.ToShortDateString();
                    worksheet.Cells[recordIndex, 11].Value = ecs.DebtrName;

                foreach (var rtn in ReturnReason)
                {
                    var Reason = rtn.Reason;
                    ecs.RtnRsn = ecs.RtnRsn + " " + Reason;
                    worksheet.Cells[recordIndex, 12].Value = ecs.RtnRsn;
                }
            }        

            string excelFileName = "ECS_DETAILS_FROM_" + Convert.ToDateTime(FrmDate).ToShortDateString() + "_TO_" + Convert.ToDateTime(ToDate).ToShortDateString() + ".xlsx";
           // string[] result = null;
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