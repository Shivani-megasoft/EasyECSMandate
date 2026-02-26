using EasyBank.BusinessLayer.Accounts.Objects;
using EasyBank.BusinessLayer.Accounts.Services;
using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EasyBank.BusinessLayer.Transactions.Contracts;
using EasyBank.BusinessLayer.Transactions.Objects;
using EasyBank.BusinessLayer.Transactions.Services;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSMandate.Models;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class ACHFilePostingController : AppController
    {
        // GET: ACHInputFile

        IDailyTransactionService dailyTransactionService = new DailyTransactionService();
        MandateService mandateService = new MandateService();
        TrnIdGenerator trnIdGenerator = new TrnIdGenerator();
        ITrnIdGeneratorService trnIdGeneratorService = new TrnIdGeneratorService();

        List<DailyTransaction> dailyTransactionList = new List<DailyTransaction>();
        List<MainBalance> mainBalanceList = new List<MainBalance>();
        DailyTransaction dailyTransaction = new DailyTransaction();
        MainBalance mainBalance = new MainBalance();
        public ActionResult Index()
        {
            DataTable Files = mandateService.GetACHFileNames(StartUp.DAY_OP_DT);
            var FileNames = new Dictionary<string, string>();
            foreach (DataRow dr in Files.Rows)
            {
                var file = dr["InputFileNo"].ToString();
                FileNames.Add(file, file);

            }
            ViewBag.FileNames = new SelectList(FileNames, "Key", "Value");
            ViewBag.ForDate = DateTime.Now.Date.AddDays(1).ToShortDateString();

            var statusList = new Dictionary<string, string>();

            statusList.Add("A", "Authenticated");
            statusList.Add("S", "UnAuthenticated");
            statusList.Add("R", "Return");
            statusList.Add("ALL", "ALL");
            ViewBag.FilterStatus = new SelectList(statusList, "Key", "Value");
            return View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            var statusList = new Dictionary<string, string>();

            statusList.Add("A", "Authenticated");
            statusList.Add("S", "UnAuthenticated");
            statusList.Add("R", "Return");
            statusList.Add("ALL", "ALL");
            ViewBag.FilterStatus = new SelectList(statusList, "Key", "Value");

            DataTable Files = mandateService.GetACHFileNames(StartUp.DAY_OP_DT);
            var FileNames = new Dictionary<string, string>();
            foreach (DataRow dr in Files.Rows)
            {
                var files = dr["InputFileNo"].ToString();
                FileNames.Add(files, files);
            }
            ViewBag.FileNames = new SelectList(FileNames, "Key", "Value");
            List<ACHFileModel> achFileList = new List<ACHFileModel>();
            List<EBTCredit> achCreditList = new List<EBTCredit>();
            EBTCredit achCredit = new EBTCredit();

            string InputFileNo = file.FileName.Split('-')[5];
            string settleMentDate = string.Empty;
            DataTable Filess = mandateService.GetACHByFileName(InputFileNo);
            if (file == null)
            {
                TempData["ErrorMsg"] = "Please select file!.";
            }
            else if (Filess.Rows.Count > 0)
            {
                TempData["ErrorMsg"] = "File already imported. Please select correct file!.";
                TempData["ShowAuthButton"] = true;
                return View("Index", achCreditList);
            }
            else
            {
                string line = "";
                string achDbCr = TransactionType.CREDIT;
                using (StreamReader sr = new StreamReader(file.InputStream))
                {
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (line.Length < 177) continue;
                        else if (Convert.ToInt16(line.Substring(0, 2)) == 56)
                        {
                            achDbCr = TransactionType.CREDIT;
                            settleMentDate = file.FileName.Split('-')[4];
                        }
                        else if (Convert.ToInt16(line.Substring(0, 2)) == 44 || Convert.ToInt16(line.Substring(0, 2)) == 67)
                        {
                            ACHFileModel achFile = new ACHFileModel();
                            achFile.SettlementDate = settleMentDate;
                            achFile.TrnCode = line.Substring(0, 2).Trim();
                            achFile.DestAcType = line.Substring(11, 2).Trim();
                            achFile.LedFolio = line.Substring(14, 3).Trim();
                            achFile.BenficiaryAcName = line.Substring(31, 40).Trim();
                            achFile.UserName = line.Substring(107, 20).Trim();
                            achFile.Amt = Convert.ToDecimal(line.Substring(120, 11).Trim());
                            achFile.ItemSeqNo = line.Substring(133, 10).Trim();
                            achFile.StatusFlag = line.Substring(153, 1).Trim();
                            achFile.ResponseCd = line.Substring(154, 2).Trim();
                            achFile.DestBnkCd = line.Substring(156, 11).Trim();
                            achFile.BenficiaryBnkAcNo = line.Substring(167, 35).Trim();
                            achFile.SponsorBnkCd = line.Substring(202, 11).Trim();
                            achFile.UserNumber = line.Substring(213, 18).Trim();
                            achFile.TranRefNo = line.Substring(231, 30).Trim();
                            achFile.ProductType = line.Substring(261, 3).Trim();
                            achFile.BenficiaryAadhaar = line.Substring(264, 15).Trim();
                            achFile.UMRNNo = line.Substring(279, 20).Trim();

                            ECS ecsMandate = mandateService.GetByUmrn(achFile.UMRNNo);
                            if (ecsMandate != null)
                            {
                                achFile.Account = AccountService.GetByNo(ecsMandate.CrAcNo);
                            }
                            if (achFile.Account != null && Convert.ToInt32(achFile.StatusFlag) == 0)
                                achFileList.Add(achFile);

                            achCredit = new EBTCredit();
                            if (achFile.Account != null)
                            {
                                achCredit.BrnCd = achFile.Account.BaseBrn;
                                achCredit.AcNo = achFile.Account.Id.ToString();
                                achCredit.Rcode = achFile.ResponseCd;
                                if (achCredit.Rcode == "00")
                                {
                                    achCredit.Stat = TransactionType.SCROLL;
                                    achCredit.TrnInd = TransactionType.SCROLL;
                                }
                                else
                                {
                                    achCredit.TrnInd = TransactionType.SCROLL;
                                    achCredit.Stat = TransactionType.RETURN;
                                }
                                achCredit.LoanAcNo =  achFile.Account.AcNo;
                              // achCredit.LoanAcNo = achFile.Account.BaseBrn + achFile.Account.GLCd + achFile.Account.AcNo;
                                achCredit.LoanAcName = achFile.Account.Name;
                            }
                            else
                            {
                                achCredit.Rcode = achFile.ResponseCd;
                                achCredit.Stat = TransactionType.RETURN;
                                achCredit.TrnInd = TransactionType.SCROLL;
                                achCredit.Stat = TransactionType.RETURN;
                                achCredit.Rcode = achFile.ResponseCd;
                            }
                            achCredit.ProcessDt = StartUp.DAY_OP_DT;
                            achCredit.RecdDt = DateTime.ParseExact(settleMentDate, "ddMMyyyy", CultureInfo.InvariantCulture);
                            //achCredit.TrnInd = TransactionType.SCROLL;
                            achCredit.TrnCode = achFile.TrnCode;
                            achCredit.DestBankId = achFile.DestBnkCd;
                            achCredit.DestAccType = achFile.DestAcType;
                            achCredit.LFNo = achFile.LedFolio;
                            achCredit.AadhaarNo = achFile.BenficiaryAadhaar;
                            achCredit.BenfName = achFile.BenficiaryAcName;
                            achCredit.SponBankId = achFile.SponsorBnkCd;
                            achCredit.UserNo = achFile.UserNumber;
                            achCredit.UserName = achFile.UserName;
                            achCredit.TranRefNo = achFile.TranRefNo;
                            achCredit.Amt = achFile.Amt;
                            achCredit.ItemSeqNo = achFile.ItemSeqNo;
                            achCredit.Checksum = "";//achFile.BenficiaryAcName; need to add from the file to ach view model
                            achCredit.BenfBankAcNo = achFile.BenficiaryBnkAcNo;
                            achCredit.ProductType = achFile.ProductType;
                            achCredit.UMRN = achFile.UMRNNo;
                            achCredit.Clerk = StartUp.LOGIN_ID;
                            achCredit.ClerkTime = string.Empty;
                            achCredit.Acnt = string.Empty;
                            achCredit.AcntTime = string.Empty;
                            achCredit.Operation = "Insert";
                            achCredit.Deleted = "N";
                            achCredit.DelBy = string.Empty;
                            achCredit.DelTime = string.Empty;
                            achCredit.InputFileNo = InputFileNo;
                            achCredit.TrnType = "RES";
                            achCredit.DbCr = TransactionType.CREDIT;
                            achCreditList.Add(achCredit);
                        }
                    }
                }
            }

            mandateService.BulkUpdate(null, null, achCreditList);
            TempData["ErrorMsg"] = "File Imported Successfully!. Total Records Count : " + achCreditList.Count;
            if (achFileList.Count != 0)
            {
                TempData["ShowAuthButton"] = true;
            }
            ViewBag.ForDate = DateTime.Now.Date.AddDays(1).ToShortDateString();
            return View("Index", achCreditList);
        }

        [HttpPost]
        public ActionResult Post(List<EBTCredit> Model)
        {
            ViewBag.ForDate = DateTime.Now.Date.AddDays(1).ToShortDateString();
            var statusList = new Dictionary<string, string>();
            statusList.Add("A", "Authenticated");
            statusList.Add("S", "UnAuthenticated");
            statusList.Add("R", "Return");
            statusList.Add("ALL", "ALL");
            ViewBag.FilterStatus = new SelectList(statusList, "Key", "Value");

            decimal Amt1 = 0;
            Response response = new Response();
            response.Status = ServiceStatus.Success;
            Account account = null;
            List<EBTCredit> achCreditList = new List<EBTCredit>();
            string setNo = string.Empty;
            if (Model == null)
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "No records for posting!.";
            }
            else
            {
                foreach (EBTCredit ach in Model)
                {
                    account = null;
                    var achCr = mandateService.GetByItemSeqNo(ach.RecdDt, ach.ItemSeqNo);
                    if (achCr == null) continue;
                    else if (achCr.TrnInd != TransactionType.AUTHENTICATE && ach.Stat == TransactionType.RETURN)
                    {
                        achCr.TrnInd = TransactionType.AUTHENTICATE;
                        achCr.Acnt = StartUp.LOGIN_ID;
                        achCr.AcntTime = DateTime.Now.ToShortTimeString();
                        achCreditList.Add(achCr);
                        continue;
                    }
                    else if (achCr.TrnInd == TransactionType.AUTHENTICATE || Convert.ToInt32(achCr.AcNo) == 0 || ach.Stat == TransactionType.RETURN) continue;
                    account = AccountService.GetById(Convert.ToInt32(achCr.AcNo));
                    if (account == null) continue;

                    if (achCr.DbCr == TransactionType.CREDIT)
                    {
                        trnIdGenerator = trnIdGeneratorService.GetTrnNo(1, 0, 0);
                        dailyTransaction = new DailyTransaction();
                        dailyTransaction.TrnBrn = StartUp.HEAD_OFFICE_BRN_CD;
                        dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                        dailyTransaction.TrnNo = trnIdGenerator.TrnNo.ToString().PadLeft(StartUp.TRN_LEN, '0');
                        dailyTransaction.SetNo = null;
                        dailyTransaction.BookCd = TransactionType.TRANSFER;
                        dailyTransaction.DbCr = TransactionType.CREDIT;
                        dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                        dailyTransaction.AcId = account.Id;
                        dailyTransaction.Amt = achCr.Amt;
                        dailyTransaction.EntryTyp = "N";
                        dailyTransaction.Narr = "By " + achCr.BenfBankAcNo + "/UMR:" + achCr.UMRN + "/SeqNo:" + achCr.ItemSeqNo;
                        dailyTransaction.TrnType = "ACHRES";
                        dailyTransaction.CompGen = "N";
                        dailyTransaction.Clerk = StartUp.LOGIN_ID;
                        dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                        dailyTransaction.Operation = "Insert";
                        dailyTransaction.CustBrn = account.BaseBrn;
                        dailyTransaction.STAN = achCr.ItemSeqNo;
                        dailyTransactionList.Add(dailyTransaction);

                        mainBalance = mainBalanceList.Where(mb => mb.AcNo == account.Id.ToString()).FirstOrDefault();
                        if (mainBalance == null)
                            mainBalance = account.MainBalance;

                        mainBalance.ClrBal += achCr.Amt;

                        //Amt1 += achCr.Amt;
                        //mainBalance.ClrBal = Amt1;
                        mainBalance.AuthUser = StartUp.LOGIN_ID;
                        mainBalanceList.Add(mainBalance);

                        achCr.TrnInd = TransactionType.AUTHENTICATE;
                        achCr.Stat = TransactionType.AUTHENTICATE;
                        achCr.TrnNo = dailyTransaction.TrnNo;
                        achCr.Acnt = StartUp.LOGIN_ID;
                        achCr.AcntTime = DateTime.Now.ToShortTimeString();
                        achCreditList.Add(achCr);
                    }
                }
            }
            Response res = new Response();
            res.Message = "No Records Found!.";
            if (dailyTransactionList.Count != 0)
            {
                List<MainBalance> newMainBalList = new List<MainBalance>();
                foreach (MainBalance mainBal in mainBalanceList)
                {
                    var dupliMainBal = newMainBalList.Where(m => m.Id == mainBal.Id).FirstOrDefault();

                    if (dupliMainBal == null)
                        newMainBalList.Add(mainBal);
                }

                trnIdGenerator = trnIdGeneratorService.GetTrnNo(0, 1, 0);
                //var trnNo = trnIdGenerator.TrnNo - dailyTransactionList.Count + 1;
                foreach (DailyTransaction dlyTrn in dailyTransactionList)
                {
                    //dlyTrn.TrnNo = trnNo++.ToString().PadLeft( StartUp.TRN_LEN, '0');
                    dlyTrn.SetNo = "IB" + trnIdGenerator.SetNo.ToString().PadLeft(StartUp.TRN_LEN, '0');
                    setNo = dlyTrn.SetNo;
                }
                mandateService.BulkUpdate(dailyTransactionList, newMainBalList, achCreditList);
                response.Status = ServiceStatus.Success;
                response.Message = "Posted Successfully!. Please note setno : " + dailyTransactionList[0].SetNo;
            }
            res = UpdateControllAcc(setNo, StartUp.DAY_OP_DT.ToString(), Model[0].InputFileNo + " File Date :" + Model[0].RecdDt.ToString("dd/MM/yyyy"));
            if (res.Status != ServiceStatus.Success)
            {
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFileNames(string date)
        {
            Response response = new Response();
            DateTime fileDate = Convert.ToDateTime(date);
            DataTable Files = mandateService.GetACHFileNames(fileDate);
            var FileNames = new Dictionary<string, string>();
            foreach (DataRow dr in Files.Rows)
            {
                var file = dr["InputFileNo"].ToString();
                FileNames.Add(file, file);
            }
            response.Item = new SelectList(FileNames, "key", "value");
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Show(string SettleDt, string FileName, string FilterStatus)
        {
            var statusList = new Dictionary<string, string>();

            statusList.Add("A", "Authenticated");
            statusList.Add("S", "UnAuthenticated");
            statusList.Add("R", "Return");
            statusList.Add("ALL", "ALL");
            ViewBag.FilterStatus = new SelectList(statusList, "Key", "Value");

            string loanAccNo = string.Empty;
            string senderName = string.Empty;
            DataTable Files = mandateService.GetACHFileNames(Convert.ToDateTime(SettleDt));
            var FileNames = new Dictionary<string, string>();
            FileNames.Add("", "---Select---");

            foreach (DataRow dr in Files.Rows)
            {
                var files = dr["InputFileNo"].ToString();
                FileNames.Add(files, files);
            }
            ViewBag.FileNames = new SelectList(FileNames, "key", "value");

            if (string.IsNullOrWhiteSpace(FilterStatus))
                FilterStatus = "ALL";

            List<EBTCredit> achCreditList = new List<EBTCredit>();
            List<EBTCredit> achCreditList1 = new List<EBTCredit>();
            List<EBTCredit> achCreditList67 = new List<EBTCredit>();

            if (string.IsNullOrWhiteSpace(SettleDt))
            {
                TempData["ErrorMsg"] = "Please select date!.";
                return View("Index", achCreditList1);
            }
            if (FilterStatus == "S")
            {
                achCreditList = mandateService.RESERVEACHCREDIT("44", Convert.ToDateTime(SettleDt), FileName, StartUp.LOGIN_ID.ToString().ToUpper(), 100);
                achCreditList67 = mandateService.RESERVEACHCREDIT("67", Convert.ToDateTime(SettleDt), FileName, StartUp.LOGIN_ID.ToString().ToUpper(), 100);
                if (achCreditList67.Count() > 0)
                    achCreditList.AddRange(achCreditList67);
            }
            else
                achCreditList = mandateService.GetAllACHByByStatus("44", FileName, FilterStatus, Convert.ToDateTime(SettleDt));

            if (achCreditList.Count == 0)
            {
                if (FilterStatus == "S")
                    achCreditList = mandateService.RESERVEACHCREDIT("67", Convert.ToDateTime(SettleDt), FileName, StartUp.LOGIN_ID.ToString().ToUpper(), 100);
                else
                    achCreditList = mandateService.GetAllACHByByStatus("67", FileName, FilterStatus, Convert.ToDateTime(SettleDt));
            }

            Account acc = null;
            foreach (EBTCredit ach in achCreditList)
            {
                if (Convert.ToInt32 (ach.AcNo) != 0)
                {
                    acc = AccountService.GetById(Convert.ToInt32(ach.AcNo));
                    ach.LoanAcNo = acc.BaseBrn + acc.GLCd + acc.AcNo;
                    ach.LoanAcName = acc.Name;
                }
                else
                {
                    ach.LoanAcNo = "";
                    ach.LoanAcName = "";
                }
                achCreditList1.Add(ach);
            }

            TempData["ErrorMsg"] = "Total Records Count : " + achCreditList1.Count;
            if (achCreditList.Count != 0)
            {
                TempData["ShowAuthButton"] = true;
            }
            return View("Index", achCreditList1);
        }

        private Response UpdateControllAcc(string setNo, string asonDate, string narration)
        {
            Response response = new Response();
            response.Status = ServiceStatus.Failure;
            //if (string.IsNullOrWhiteSpace(setNo))
            //    return response;

            List<Branch> brlist = StartUp.BRN_LIST;
            List<Branch> brlist1 = new List<Branch>();
            var hoCrAccount = new Account();
            Account ibAccount = new Account();
            List<string> brcdlist = new List<string>();

            mainBalanceList = new List<MainBalance>();
            List<DailyTransaction> dailyTransactionListHO = new List<DailyTransaction>();
            DateTime forDt = Convert.ToDateTime(asonDate);

            var achSetAccount = AccountService.GetByNo(StartUp.HEAD_OFFICE_BRN_CD + ConfigStartUp.APBS_GL_CD + ConfigStartUp.APBS_AC_NO);
            if (achSetAccount == null)
            {
                response.Message = "Account not found" + StartUp.HEAD_OFFICE_BRN_CD + ConfigStartUp.APBS_GL_CD + ConfigStartUp.APBS_AC_NO;
                response.Status = ServiceStatus.ValidationError;
                return response;
            }

            decimal totalamtHo = 0; decimal returnTotalamtHo = 0;
            var achAllCreditList = mandateService.GetAllACHCreditByType("RES", Convert.ToDateTime(asonDate)).FindAll(a => a.Stat == "A" || a.Stat == "R");
            totalamtHo = achAllCreditList.Sum(A => A.Amt);
            var returnCrList = achAllCreditList.Where(a => a.Stat == TransactionType.RETURN).ToList();
            returnTotalamtHo = returnCrList.Sum(A => A.Amt);
            achAllCreditList = mandateService.GetAllACHCreditByType("RES", Convert.ToDateTime(asonDate)).FindAll(a => a.Stat == "A");
            brcdlist = achAllCreditList.Select(A => A.BrnCd).Distinct().ToList();

            foreach (string br in brcdlist)
            {
                if (string.IsNullOrEmpty(br))
                    continue;
                Branch b = new Branch();
                b = BranchService.GetAll().FindAll(a => a.BrnCd == br).FirstOrDefault();
                brlist1.Add(b);
            }
            string setNoHo = string.Empty;

            foreach (Branch b in brlist1)
            {
                List<EBTCredit> allCreditedList = new List<EBTCredit>();
                decimal totalamt = 0;

                allCreditedList = achAllCreditList.FindAll(a => a.TrnType == "RES" && a.BrnCd == b.BrnCd).FindAll(a => a.TrnInd == "A" || a.TrnInd == "R");
                if (allCreditedList.Count == 0)
                    continue;
                else
                {
                    totalamt = allCreditedList.Sum(a => a.Amt);
                }
                var topEbtCredit = allCreditedList.FirstOrDefault();
                string setno = setNo;


                //setno = trnIdGenerator.SetNo.ToString().PadLeft(StartUp.TRN_LEN, '0');


                if (b.BrnCd == StartUp.HEAD_OFFICE_BRN_CD)
                {


                }
                else
                {
                    // Debit Branch adjuctment A/c for Transaction
                    #region Branch IB Entry
                    ibAccount = AccountService.GetByNo(b.BrnCd + b.InterBrnAdjGLCd + b.InterBrnAdjAcNo);
                    if (ibAccount == null)
                    {
                        response.Message = "Account not found" + b.BrnCd + b.InterBrnAdjGLCd + b.InterBrnAdjAcNo;
                        response.Status = ServiceStatus.ValidationError;
                        return response;
                    }
                    List<DailyTransaction> ibAccountTrans = new List<DailyTransaction>();
                    ibAccountTrans = dailyTransactionService.GetAllAccTrnByDt(ibAccount.Id, forDt);
                    ibAccountTrans = ibAccountTrans.FindAll(a => a.TrnType == "ACHRES-3");


                    dailyTransaction = new DailyTransaction();
                    dailyTransaction = null;
                    dailyTransaction = ibAccountTrans.FirstOrDefault();
                    dailyTransaction = ibAccountTrans.FirstOrDefault();
                    if (dailyTransaction == null)
                    {
                        dailyTransaction = new DailyTransaction();
                        dailyTransaction.TrnBrn = StartUp.HEAD_OFFICE_BRN_CD;
                        dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                        //dailyTransaction.TrnNo = StringUtils.PadLeft((trnIdGenerator.TrnNo).ToString(), StartUp.TRN_LEN, '0');
                        dailyTransaction.SetNo = setno;
                        dailyTransaction.BookCd = TransactionType.TRANSFER;
                        dailyTransaction.DbCr = TransactionType.DEBIT;
                        dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                        dailyTransaction.AcId = ibAccount.Id;
                        dailyTransaction.Amt = totalamt;
                        dailyTransaction.EntryTyp = "N";
                        dailyTransaction.Narr = "To ACH-RES InputFile" + narration + "/" + b.BrnCd;
                        dailyTransaction.TrnType = "ACHRES";
                        dailyTransaction.CompGen = "Y";
                        dailyTransaction.Clerk = StartUp.LOGIN_ID;
                        dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                        //dailyTransaction.Acnt = StartUp.LOGIN_ID;
                        //dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
                        dailyTransaction.Operation = "Insert";
                        dailyTransaction.STAN = b.BrnCd + "ACHRES-3";
                        dailyTransaction.CustBrn = ibAccount.BaseBrn;
                        dailyTransactionListHO.Add(dailyTransaction);

                        mainBalance = mainBalanceList.Where(mb => mb.AcNo == ibAccount.Id.ToString()).FirstOrDefault();
                        if (mainBalance == null) mainBalance = ibAccount.MainBalance;
                        mainBalance.ClrBal -= totalamt;
                        mainBalance.AuthUser = StartUp.LOGIN_ID;
                        mainBalanceList.Add(mainBalance);
                    }
                    else
                    {
                        dailyTransaction.Amt = totalamt;
                        dailyTransactionListHO.Add(dailyTransaction);
                        mainBalance = mainBalanceList.Where(mb => mb.AcNo == ibAccount.Id.ToString()).FirstOrDefault();
                        if (mainBalance == null) mainBalance = ibAccount.MainBalance;
                        mainBalance.ClrBal -= totalamt;
                        mainBalance.AuthUser = StartUp.LOGIN_ID;
                        mainBalanceList.Add(mainBalance);
                    }
                    #endregion

                    // Credit branch Adjust A/c for Transaction
                    #region HO Branch Adjustment
                    hoCrAccount = AccountService.GetByNo(StartUp.HEAD_OFFICE_BRN_CD + StartUp.HEAD_GLCd + b.BrnCd.ToString().PadLeft(StartUp.AC_LEN, '0'));

                    if (hoCrAccount == null)
                    {
                        response.Message = "Account not found" + StartUp.HEAD_OFFICE_BRN_CD + StartUp.HEAD_GLCd + b.BrnCd.ToString().PadLeft(StartUp.AC_LEN, '0');
                        response.Status = ServiceStatus.ValidationError;
                        return response;
                    }

                    List<DailyTransaction> hoCrAccountTrans = new List<DailyTransaction>();
                    hoCrAccountTrans = dailyTransactionService.GetAllAccTrnByDt(hoCrAccount.Id, forDt);
                    hoCrAccountTrans = hoCrAccountTrans.FindAll(a => a.TrnType == "ACHRES-2");

                    dailyTransaction = new DailyTransaction();
                    dailyTransaction = null;
                    dailyTransaction = hoCrAccountTrans.FirstOrDefault();
                    if (dailyTransaction == null)
                    {
                        dailyTransaction = new DailyTransaction();
                        dailyTransaction.TrnBrn = StartUp.HEAD_OFFICE_BRN_CD;
                        dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                        //dailyTransaction.TrnNo = StringUtils.PadLeft((trnIdGenerator.TrnNo - 1).ToString(), StartUp.TRN_LEN, '0');
                        dailyTransaction.SetNo = setno;
                        dailyTransaction.BookCd = TransactionType.TRANSFER;
                        dailyTransaction.DbCr = TransactionType.CREDIT;
                        dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                        dailyTransaction.AcId = hoCrAccount.Id;
                        dailyTransaction.Amt = totalamt;
                        dailyTransaction.Clerk = StartUp.LOGIN_ID;
                        dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                        dailyTransaction.EntryTyp = "N";
                        dailyTransaction.Narr = "By " + b.BrnCd + "/ " + narration;
                        dailyTransaction.TrnType = "ACHRES";
                        dailyTransaction.CompGen = "Y";
                        //dailyTransaction.Acnt = StartUp.LOGIN_ID;
                        //dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
                        dailyTransaction.Operation = "Insert";
                        dailyTransaction.STAN = b.BrnCd + "ACHRES-2";
                        dailyTransaction.CustBrn = hoCrAccount.BaseBrn;
                        dailyTransactionListHO.Add(dailyTransaction);

                        mainBalance = mainBalanceList.Where(mb => mb.AcNo == hoCrAccount.Id.ToString()).FirstOrDefault();
                        if (mainBalance == null) mainBalance = hoCrAccount.MainBalance;
                        mainBalance.ClrBal += totalamt;
                        mainBalance.AuthUser = StartUp.LOGIN_ID;
                        mainBalanceList.Add(mainBalance);
                    }
                    else
                    {
                        dailyTransaction.Amt = totalamt;
                        dailyTransactionListHO.Add(dailyTransaction);
                        mainBalance = mainBalanceList.Where(mb => mb.AcNo == hoCrAccount.Id.ToString()).FirstOrDefault();
                        if (mainBalance == null) mainBalance = hoCrAccount.MainBalance;
                        mainBalance.ClrBal += totalamt;
                        mainBalance.AuthUser = StartUp.LOGIN_ID;
                        mainBalanceList.Add(mainBalance);
                    }
                    #endregion
                }
                //totalamtHo += totalamt;
                    if (string.IsNullOrWhiteSpace(setNoHo))
                    setNoHo = setno;
            }

            // Debit Head office set A/c for Transaction
            #region Credit To Settlement Account
            achSetAccount = AccountService.GetByNo(StartUp.HEAD_OFFICE_BRN_CD + ConfigStartUp.APBS_GL_CD + ConfigStartUp.APBS_AC_NO);
            if (achSetAccount == null)
            {
                response.Message = "Account not found" + StartUp.HEAD_OFFICE_BRN_CD + ConfigStartUp.APBS_GL_CD + ConfigStartUp.APBS_AC_NO;
                response.Status = ServiceStatus.ValidationError;
                return response;
            }
            List<DailyTransaction> HoTransaction = new List<DailyTransaction>();
            HoTransaction = dailyTransactionService.GetAllAccTrnByDt(achSetAccount.Id, forDt);
            HoTransaction = HoTransaction.FindAll(a => a.TrnType == "ACHRES-1");

            if (string.IsNullOrWhiteSpace(setNoHo))
            {
                trnIdGenerator = trnIdGeneratorService.GetTrnNo(0, 1, 0);
                setNoHo = trnIdGenerator.SetNo.ToString().PadLeft(7, '0');
            }
            dailyTransaction = new DailyTransaction();
            dailyTransaction = null;
            dailyTransaction = HoTransaction.FirstOrDefault();
            if (totalamtHo > 0)
            {
                if (dailyTransaction == null)
                {
                    dailyTransaction = new DailyTransaction();
                    dailyTransaction.TrnBrn = StartUp.HEAD_OFFICE_BRN_CD;
                    dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                    //dailyTransaction.TrnNo = StringUtils.PadLeft((trnIdGenerator.TrnNo - 2).ToString(), StartUp.TRN_LEN, '0');
                    dailyTransaction.SetNo = setNoHo;
                    dailyTransaction.BookCd = TransactionType.TRANSFER;
                    dailyTransaction.DbCr = TransactionType.DEBIT;
                    dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                    dailyTransaction.AcId = achSetAccount.Id;
                    dailyTransaction.Amt = totalamtHo;
                    dailyTransaction.EntryTyp = "N";
                    dailyTransaction.Narr = "To ACH-RES Input File " + narration + " Total Records : " + achAllCreditList.Count() ;
                    dailyTransaction.TrnType = "ACHRES";
                    dailyTransaction.CompGen = "Y";
                    dailyTransaction.Clerk = StartUp.LOGIN_ID;
                    dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                    dailyTransaction.Acnt = StartUp.LOGIN_ID;
                    dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
                    dailyTransaction.Operation = "Insert";
                    dailyTransaction.STAN = StartUp.HEAD_OFFICE_BRN_CD + "ACHRES-1";
                    dailyTransaction.CustBrn = achSetAccount.BaseBrn;
                    dailyTransactionListHO.Add(dailyTransaction);

                    mainBalance = mainBalanceList.Where(mb => mb.AcNo == achSetAccount.Id.ToString()).FirstOrDefault();
                    if (mainBalance == null) mainBalance = achSetAccount.MainBalance;
                    mainBalance.ClrBal -= totalamtHo;
                    mainBalance.AuthUser = StartUp.LOGIN_ID;
                    mainBalanceList.Add(mainBalance);
                }
                else
                {
                    dailyTransaction.Amt = totalamtHo;

                    dailyTransactionListHO.Add(dailyTransaction);

                    mainBalance = mainBalanceList.Where(mb => mb.AcNo == achSetAccount.Id.ToString()).FirstOrDefault();
                    if (mainBalance == null) mainBalance = achSetAccount.MainBalance;
                    mainBalance.ClrBal -= totalamtHo;
                    mainBalance.AuthUser = StartUp.LOGIN_ID;
                    mainBalanceList.Add(mainBalance);
                }
            }

            HoTransaction = new List<DailyTransaction>();
            dailyTransaction = null;
            HoTransaction = dailyTransactionService.GetAllAccTrnByDt(achSetAccount.Id, forDt);
            HoTransaction = HoTransaction.FindAll(a => a.TrnType == "ACHRET");
            dailyTransaction = HoTransaction.FirstOrDefault();

            if (returnTotalamtHo > 0)
            {
                if (dailyTransaction == null)
                {
                    dailyTransaction = new DailyTransaction();
                    dailyTransaction.TrnBrn = StartUp.HEAD_OFFICE_BRN_CD;
                    dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                    //dailyTransaction.TrnNo = StringUtils.PadLeft((trnIdGenerator.TrnNo - 2).ToString(), StartUp.TRN_LEN, '0');
                    dailyTransaction.SetNo = setNoHo;
                    dailyTransaction.BookCd = TransactionType.TRANSFER;
                    dailyTransaction.DbCr = TransactionType.CREDIT;
                    dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                    dailyTransaction.AcId = achSetAccount.Id;
                    dailyTransaction.Amt = returnTotalamtHo;
                    dailyTransaction.EntryTyp = "N";
                    dailyTransaction.Narr = "To ACH-RES Input File " + narration + " Return Cnt :" + returnCrList.Count();
                    dailyTransaction.TrnType = "ACHRES";
                    dailyTransaction.CompGen = "Y";
                    dailyTransaction.Clerk = StartUp.LOGIN_ID;
                    dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                    dailyTransaction.Acnt = StartUp.LOGIN_ID;
                    dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
                    dailyTransaction.Operation = "Insert";
                    dailyTransaction.STAN = StartUp.HEAD_OFFICE_BRN_CD + "ACHRES-1";
                    dailyTransaction.CustBrn = achSetAccount.BaseBrn;
                    dailyTransactionListHO.Add(dailyTransaction);

                    mainBalance = mainBalanceList.Where(mb => mb.AcNo == achSetAccount.Id.ToString()).FirstOrDefault();
                    if (mainBalance == null) mainBalance = achSetAccount.MainBalance;
                    mainBalance.ClrBal += returnTotalamtHo;
                    mainBalance.AuthUser = StartUp.LOGIN_ID;
                    mainBalanceList.Add(mainBalance);
                }
                else
                {
                    dailyTransaction.Amt = totalamtHo;

                    dailyTransactionListHO.Add(dailyTransaction);

                    mainBalance = mainBalanceList.Where(mb => mb.AcNo == achSetAccount.Id.ToString()).FirstOrDefault();
                    if (mainBalance == null) mainBalance = achSetAccount.MainBalance;
                    mainBalance.ClrBal += returnTotalamtHo;
                    mainBalance.AuthUser = StartUp.LOGIN_ID;
                    mainBalanceList.Add(mainBalance);

                }
            }
            if (dailyTransactionListHO.Count != 0)
            {
                trnIdGenerator = new TrnIdGenerator();
                trnIdGenerator = trnIdGeneratorService.GetTrnNo(dailyTransactionListHO.Count, 0, 0);
                var trnNo = trnIdGenerator.TrnNo - dailyTransactionListHO.Count + 1;
                foreach (DailyTransaction dlyTrn in dailyTransactionListHO)
                {
                    dlyTrn.TrnNo = trnNo++.ToString().PadLeft(StartUp.TRN_LEN, '0');
                    dlyTrn.SetNo = setNoHo;
                }

                List<MainBalance> newMainBalListHo = new List<MainBalance>();
                foreach (MainBalance mainBal in mainBalanceList)
                {
                    var dupliMainBal = newMainBalListHo.Where(m => m.Id == mainBal.Id).FirstOrDefault();

                    if (dupliMainBal == null)
                        newMainBalListHo.Add(mainBal);
                }
                //END

                if (dailyTransactionListHO.Count != 0)
                {
                    try
                    {
                        dailyTransactionService.Update(dailyTransactionListHO, newMainBalListHo);
                        response.Status = ServiceStatus.Success;
                    }
                    catch (Exception ex)
                    {
                        response.Message = ex.Message;
                        response.Status = ServiceStatus.Failure;
                    }
                }
            }
            #endregion

            return response;
        }
    }
}