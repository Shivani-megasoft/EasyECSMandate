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

namespace ECSMandate.Controllers
{
    public class MandateAcknowledgeController : AppController
    {
        MandateService mandateService = new MandateService();
       
        public ActionResult Index()
        {
            ViewBag.FrmDate = DateTime.Now.Date.ToShortDateString();
            ViewBag.ToDate = DateTime.Now.Date.AddDays(1).ToShortDateString();           

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


            var StatList = new Dictionary<string, string>();
            StatList.Add("All", "All");
            StatList.Add("Accepted", "Accepted");
            StatList.Add("Rejected", "Rejected");
            StatList.Add("Pending", "Pending");
            StatList.Add("Cancel", "Cancel");
            var StatusList = StatList.Select(b => new
            {
                Name = b.Value,
                Id = b.Key
            });
            ViewBag.StatusList = StatusList;
            return View();
        }
        public ActionResult GetECSAckByDate(string Type, string FrmDate, string ToDate, string status, string Branch)
        {
            Response response = new Response();
            List<MandateAckModel> mndtAckList = new List<MandateAckModel>();
            List<MandateAckModel> mndtAckList2 = new List<MandateAckModel>();

            DateTime frmDate = Convert.ToDateTime(FrmDate);
            DateTime toDate = Convert.ToDateTime(ToDate);

            //if (Type == "Outward")
            //    Type = ECS.CREATE;
            //else if (Type == "Inward")
            //    Type = ECS.ACCEPT;


            if (Type == "Outward")
                Type = "OW";
            else if (Type == "Inward")
                Type = "IW";
            else
                Type = "All";

            var manadteList = mandateService.GetAllReqMandtebystatus(Type, frmDate, toDate, status);
            
            if (Type == "OW" && manadteList.Count != 0)
                {
                string brncd = manadteList[0].CrAcNo.Substring(0, 4);
                manadteList =  manadteList.Where(a => a.CrAcNo.Substring(0, 4) == Branch).ToList();
            }

            if (Type == "OW" && Branch == "All")
              {
                manadteList = mandateService.GetAllReqMandtebystatus(Type, frmDate, toDate, status);
            }

            mndtAckList = BindAckModel(manadteList);

            foreach (var d in mndtAckList)
            {
                if (d.UMRDt >= frmDate && d.UMRDt <= toDate)
                {
                    mndtAckList2.Add(d);
                }
            }
            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_ECSAckList", mndtAckList2, null) }, JsonRequestBehavior.AllowGet);
        }
        private List<MandateAckModel> BindAckModel(List<ECS> ecsmandateList)

        {
            List<MandateAckModel> mndtAckList = new List<MandateAckModel>();

            foreach (var ecsMandate in ecsmandateList)
            {
                    MandateAckModel mndtAckModel = new MandateAckModel();

                    if (ecsMandate.MandateTyp == "OW")
                        mndtAckModel.Type = "Outward";
                    else
                        mndtAckModel.Type = "Inward";

                    var mandate = mandateService.GetById(ecsMandate.Id);
                    if (mandate == null) continue;

                    mndtAckModel.UMRNo = mandate.UMRNo;
                    mndtAckModel.UMRDt = mandate.UMRDt;
                    mndtAckModel.FileName = mandate.MandateFile;
                    mndtAckModel.CrBrnCd = mandate.CrBrnCd;
                    mndtAckModel.CrGlCd = mandate.CrGlCd;
                    mndtAckModel.CrAcNo = mandate.CrAcNo;
                    mndtAckModel.Mobile = mandate.Mobile;
                    mndtAckModel.CreditorsName = mandate.CrdtrName;
                    mndtAckModel.DebtrName = mandate.DebtrName;
                    mndtAckModel.Amt = Convert.ToDecimal(mandate.Amt).ToString();
                    mndtAckModel.Status = mandate.Status;

                //string ackMndt;
                //if (ecsMandate.MandateTyp == ECS.CREATE)
                //    ackMndt = ECS.CREATE_ACK;
                //else
                //    ackMndt = ECS.ACCEPT_ACK;

                var ack = mandateService.GetAckMandte(mandate.Id, ecsMandate.MandateTyp, mandate.MandateFile);
                    if (ack != null)
                    {
                        mndtAckModel.Reason = ack.Reason;
                        if (string.IsNullOrWhiteSpace(mndtAckModel.Reason))
                            mndtAckModel.IsAccept = mandate.IsAccept == true ? "Accepted" : "Pending";
                        else
                            mndtAckModel.IsAccept = mandate.IsAccept == true ? "Accepted" : "Rejected";
                    }
                    mndtAckList.Add(mndtAckModel);                
            }
            return mndtAckList;
        }

       [HttpPost] 
        public ActionResult GetExcelByDate(string Type, string FrmDate, string ToDate,string status, string Branch)
        {
            int recordIndex = 1; int srno = 0;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage Excel = new ExcelPackage();

            var worksheet = Excel.Workbook.Worksheets.Add(" ECS_Acknowledge_REPORT_" + Convert.ToDateTime(FrmDate).ToShortDateString() + "_TO_" + Convert.ToDateTime(ToDate).ToShortDateString());
            worksheet.DefaultRowHeight = 20;
            worksheet.DefaultColWidth = 25;
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[recordIndex, 1].Value = StartUp.BNK_NAME + "  Branch : " + "";
            worksheet.Cells[recordIndex, 1, recordIndex, 11].Merge = true;
            worksheet.Row(recordIndex).Style.Font.Bold = true;

            recordIndex++;
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Row(recordIndex).Style.Font.Bold = true;
            worksheet.Cells[recordIndex, 1].Value = "Reserve List";
            worksheet.Cells[recordIndex, 1, recordIndex, 11].Merge = true;
            worksheet.Row(recordIndex).Style.Font.Bold = true;

            recordIndex++;
            worksheet.Row(recordIndex).Style.Font.Bold = true;
            worksheet.Cells[recordIndex, 1].Value = "TYPE";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            worksheet.Cells[recordIndex, 2].Value = "UMR No.";  
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 3].Value = "UMR Date";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 4].Value = "File";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[recordIndex, 4, recordIndex, 5].Merge = true;

            worksheet.Cells[recordIndex, 6].Value = "Creditors Account No.";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            worksheet.Cells[recordIndex, 7].Value = "Creditors Name";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 8].Value = "Amount";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            worksheet.Cells[recordIndex, 9].Value = "Mobile";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            worksheet.Cells[recordIndex, 10].Value = "ACK Flag";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
           
            worksheet.Cells[recordIndex, 11].Value = "Debitors Name";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 12].Value = "Reason";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells[recordIndex, 13].Value = "Status";
            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            Response response = new Response();
            List<MandateAckModel> mndtAckList = new List<MandateAckModel>();
            DateTime frmDate = Convert.ToDateTime(FrmDate);
            DateTime toDate = Convert.ToDateTime(ToDate);

            //if (Type == "Outward")
            //    Type = ECS.CREATE;
            //else if (Type == "Inward")
            //    Type = ECS.ACCEPT;
            //else
            //    Type = "All";

            if (Type == "Outward")
                Type = "OW";
            else if (Type == "Inward")
                Type = "IW";
            else
                Type = "All";

            var manHistoryList = mandateService.GetAllReqMandtebystatus(Type, frmDate, toDate, status);


            //var manadteList = mandateService.GetAllReqMandtebystatus(Type, frmDate, toDate, status);
            //string brncd = manHistoryList[0].CrAcNo.Substring(0, 4);
            
            if (Type == "OW")
            {
                if (Branch != "All")
                {
                    manHistoryList = manHistoryList.Where(a => a.CrAcNo.Substring(0, 4) == Branch).ToList();
                }
            }


            mndtAckList = BindAckModel(manHistoryList);

            foreach (var mn in mndtAckList)
            {
                if (mn.UMRDt >= frmDate && mn.UMRDt <= toDate)
                {
                    srno++;
                    recordIndex++;
                    if (mn.Type == "Inward")
                        worksheet.Cells[recordIndex, 1].Value = "INWARD";
                    else
                        worksheet.Cells[recordIndex, 1].Value = "OUTWARD";

                    worksheet.Cells[recordIndex, 2].Value = mn.UMRNo;
                    worksheet.Cells[recordIndex, 3].Value = Convert.ToDateTime(mn.UMRDt).ToShortDateString();
                    worksheet.Cells[recordIndex, 4].Value = mn.FileName;
                    worksheet.Cells[recordIndex, 6].Value = (mn.CrBrnCd + mn.CrGlCd + mn.CrAcNo);
                    worksheet.Cells[recordIndex, 7].Value = mn.CreditorsName;
                    worksheet.Cells[recordIndex, 8].Value = mn.Amt;
                    worksheet.Cells[recordIndex, 9].Value = mn.Mobile;
                    worksheet.Cells[recordIndex, 10].Value = mn.IsAccept;
                    worksheet.Cells[recordIndex, 11].Value = mn.DebtrName;
                    worksheet.Cells[recordIndex, 12].Value = mn.Reason;
                    worksheet.Cells[recordIndex, 13].Value = mn.Status;
                }
            }
            string excelFileName = "ECS_Acknowledge_Report_" + Convert.ToDateTime(FrmDate).ToShortDateString() + "_TO_" + Convert.ToDateTime(ToDate).ToShortDateString() + ".xlsx";
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