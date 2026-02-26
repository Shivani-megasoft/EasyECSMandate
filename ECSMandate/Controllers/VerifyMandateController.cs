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
    public class VerifyMandateController : AppController
    {
        MandateService mandateService = new MandateService();
        // GET: VerifyMandate
        public ActionResult Index()
        {
            Response response = new Response();
            SetViewBag();
            return View();
        }
        public ActionResult LoadMandateImage()
        {
            Response response = new Response();
            List<ImageModel> imgModelList = new List<ImageModel>();
            string frntImgSrc, detailFrntImgSrc;
            string cbsAcNo = "";
            ImageModel imgModel;
            MandateAccountModel manAccModel = new MandateAccountModel();
            string brnCd, glcd, acno;
            brnCd = glcd = acno = "";
            var ecsMandate = mandateService.AuthReserveECSMandate("1", StartUp.LOGIN_ID);

            if (ecsMandate != null)
            {
                ecsMandate.RefNo1 = ecsMandate.RefNo1;
                manAccModel.AccNo = ecsMandate.AcNo;
                if (ecsMandate.AcNo.Length == 15)
                {
                    brnCd = ecsMandate.AcNo.Substring(0, 4);
                    glcd = ecsMandate.AcNo.Substring(4, 4);
                    acno = ecsMandate.AcNo.Substring(8, 7);
                }

                decimal acBal = 0;
                string acName, fName;
                acName = fName = string.Empty;

                Account account = null;

                try
                {
                    account = AccountService.GetByNo(ecsMandate.AcNo);
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    response.Status = ServiceStatus.ValidationError;
                }
                string acStatus = "";

                if (account != null)
                {
                    cbsAcNo = account.AcNo;
                    acBal = 40;
                    acName = account.Name;
                    fName = account.Customer.FHName;
                    manAccModel.CrdtrName = ecsMandate.UtilityName;
                    manAccModel.AcName = account.Name;
                    manAccModel.FName = account.Customer.FHName;
                    manAccModel.BrnCd = account.BaseBrn;
                    manAccModel.GLCd = account.GLCd;
                    manAccModel.AcNo = account.AcNo;

                    //cbsAcNo = account.BaseBrn + account.GLCd + account.AcNo;
                    manAccModel.ClrBal = account.MainBalance.ClrBal;
                    manAccModel.Status = account.AcStatus.Name;
                    manAccModel.Category = account.Customer.AcCat;

                    KYCStatus kycStatus = account.Customer.KYCStatus;
                    if (kycStatus != null)
                        manAccModel.KYCStatus = kycStatus.Status;
                    else
                        manAccModel.KYCStatus = "";
                    if (account.Customer.PanNo != null && account.Customer.PanNo.Length == 10)
                    {
                        manAccModel.PANNo = string.Concat("".PadLeft(6, '*'), account.Customer.PanNo.Substring(account.Customer.PanNo.Length - 4));
                    }
                    if (account.Customer.AadhaarNo != null && account.Customer.AadhaarNo.Length == 12)
                    {
                        //manAccModel.AadhaarNo = string.Concat("".PadLeft(8, '*'), account.Customer.AdharCardNo.Substring(account.Customer.AdharCardNo.Length - 4));
                        manAccModel.AadhaarNo = account.Customer.AadhaarNo;
                    }

                    //UtilityExtensions.CreateLog(account.AcStatus.Name);
                    acStatus = account.AcStatus.Name;
                }

                var imagePath = ConfigStartUp.INWARD_IMAGE_PATH;

                //UtilityExtensions.CreateLog(imagePath);
                if (ecsMandate.FrntImg == null && ecsMandate.DetailFrntImg == null)
                {
                    response.Message = "Image Not Found";
                    response.Status = ServiceStatus.ValidationError;
                    return Json(response);
                }

                frntImgSrc = imagePath + @"\" + ecsMandate.FrntImg;
                detailFrntImgSrc = imagePath + @"\" + ecsMandate.DetailFrntImg;
                //UtilityExtensions.CreateLog(ecsMandate.DetailFrntImg);
                Image bitMapImg = null;
                string base64detailImgSrc = string.Empty;
                string name = string.Empty;
                string base64frntImgSrc = string.Empty;

                if (ecsMandate.DetailFrntImg != string.Empty)
                {
                    imgModel = new ImageModel();
                    imgModel.ImgName = ecsMandate.DetailFrntImg;
                    imgModel.ImgSrc = imagePath;
                    imgModel.ImageTyp = "DetailFrnt";

                    imgModelList.Add(imgModel);
                }

                bitMapImg = null;
                if (ecsMandate.FrntImg != string.Empty)
                {
                    bitMapImg = UtilityExtensions.ConvertTiffToJPG(frntImgSrc, ref name);
                    ImageConverter converter = new ImageConverter();
                    base64frntImgSrc = "data:image/jpg;base64," + Convert.ToBase64String((byte[])converter.ConvertTo(bitMapImg, typeof(byte[])));//ecsMandate.FrntImg,

                    imgModel = new ImageModel();
                    imgModel.ImgName = ecsMandate.FrntImg;
                    imgModel.ImgSrc = base64frntImgSrc;
                    imgModel.ImageTyp = "Front";
                    imgModelList.Add(imgModel);
                }

                //Get Signature of accountr
                //Fetch Signature for account
                string signatureFilePath = ConfigurationManager.AppSettings.Get("SIGNATURE_FILE_PATH") + @"\" + brnCd;
                string signFileName = ""; string signFilePath = "";
                UtilityExtensions.CreateLog(signatureFilePath);
                UtilityExtensions.IPersonateSignUser();

                if (account == null)
                {
                    response.Message = "Account not found.";
                    //response.Status = ServiceStatus.ValidationError;
                }
                else if (account != null && account.AcStatus.Name != "Operative")
                {
                    response.Message = "A/c is " + account.AcStatus.Name + ", no Authority to view the Signature !";
                    //response.Status = ServiceStatus.ValidationError;
                    //return Json(response);
                }
                else if (System.IO.Directory.Exists(signatureFilePath))
                {
                    string signatureFileName = brnCd + glcd + acno + "*.jpg";

                    var signFiles = Directory.GetFiles(signatureFilePath, signatureFileName);

                    signFileName = signFilePath = string.Empty;
                    if (signFiles.Length > 0)
                    {
                        var sign = signFiles[0];
                        signFileName = sign.Substring(sign.LastIndexOf(@"\") + 1);
                        signFilePath = sign.Substring(0, sign.LastIndexOf(signFileName) - 1);
                    }
                }
                var rsnList = mandateService.GetAllReason("Accept", ecsMandate.Variant);
                //var oplist = OpInstructionService.GetAll();
                var OpinstName = "";
                if (account != null)
                {
                    OpinstName = account.OpInstruction.Name;
                }
                //END
                response.Item = new
                {
                    //FrntImgSrc = base64frntImgSrc,
                    //DetailFrntImgSrc = ecsMandate.DetailFrntImg,
                    //ImgPath = imagePath,
                    ImageList = imgModelList,
                    Mandate = ecsMandate,
                    AcModel = manAccModel,
                    CustSign = signFileName,
                    SignImgSrc = signFilePath,
                    AcNo = cbsAcNo,
                    Name = acName,
                    FName = fName,
                    ClrBal = acBal,
                    OpinstName = OpinstName,
                    KycStatus = manAccModel.KYCStatus,
                    RsnList = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd }),
                    IsValidAcc = (account == null) ? false : true,
                    //AcStatus=
                };

                if (ecsMandate.FrntImg != string.Empty)
                {
                    UtilityExtensions.IPersonateuser();
                    if (bitMapImg != null)
                        bitMapImg.Dispose();
                    try
                    {
                        System.IO.File.Delete(name);
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                //UtilityExtensions.CreateLog("3.mandate not found");
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Next Mandate Not Found";
            }

            return Json(response);
        }
        public ActionResult VerifiedMandate(string Id)
        {
            Response response = new Response();
            var mandate = mandateService.GetById(Convert.ToInt32(Id));
            mandate.TrnInd = "A";
            mandate.AuthUser = StartUp.LOGIN_ID;
            mandate.Operation = "Update";
            mandate.MandateStatus = "Auth";

            mandateService.Update(mandate);

            response.Message = "Mandate Updated Successfully";
            response.Status = ServiceStatus.Success;

            return Json(response);
        }

        public ActionResult ReturnMandate(string Id)
        {
            Response response = new Response();

            var mandate = mandateService.GetById(Convert.ToInt32(Id));

            mandate.TrnInd = "R";
            mandate.Operation = "Update";
            mandate.MandateStatus = "Return";
            mandateService.Update(mandate);

            response.Message = "Mandate Updated Successfully";
            response.Status = ServiceStatus.Success;

            return Json(response);
        }
        public ActionResult ReleaseMandate()
        {
            Response response = new Response();

            var ecsList = mandateService.GetMandateByStatus("H", StartUp.LOGIN_ID);
            foreach (ECS ecs in ecsList)
            {
                ecs.RtnRsn = "";
                ecs.TrnInd = "Q";
                ecs.MandateStatus = "Uploaded";
                //ecs.ReserveBy = "";
                mandateService.Update(ecs);
            }

            response.Message = "Mandate Released Successfully";
            response.Status = ServiceStatus.Success;

            return Json(response);
        }
        public ActionResult HoldMandate(string Id)
        {
            Response response = new Response();

            var mandate = mandateService.GetById(Convert.ToInt32(Id));

            mandate.TrnInd = "H";
            mandate.Operation = "Update";
            mandate.MandateStatus = "Hold";
            mandate.AuthUser = StartUp.LOGIN_ID;

            mandateService.Update(mandate);

            response.Message = "Mandate Hold Successfully";
            response.Status = ServiceStatus.Success;

            return Json(response);
        }

        public ActionResult UpdateMandate(string Id, string Operation, string RtnRsn)
        {
            Response response = new Response();
            string CloseTimeStr = ConfigurationManager.AppSettings["NPCI_Close_Time"];
            TimeSpan npciCloseTime;
            if (TimeSpan.TryParse(CloseTimeStr, out npciCloseTime))
            {
                TimeSpan currentTime = DateTime.Now.TimeOfDay;
                int comparisonResult = TimeSpan.Compare(currentTime, npciCloseTime);
                if (comparisonResult > 0)
                {
                    response.Status = ServiceStatus.Failure;
                    response.Message = "You can not verify, time is over for today.!";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                response.Status = ServiceStatus.Failure;
                response.Message = "Invalid time format in web config.";
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            var mandate = mandateService.GetById(Convert.ToInt32(Id));
            bool isAccp = false;
            if (Operation == "Verify")
            {
                if (!string.IsNullOrWhiteSpace(RtnRsn) && RtnRsn.StartsWith("M0"))
                {
                    response.Status = ServiceStatus.Failure;
                    response.Message = "Please do not select return reason!.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                mandate.TrnInd = "A";
                mandate.MandateStatus = "Auth";
                mandate.RtnRsn = "ac01";
                isAccp = true;
            }
            else if (Operation == "Return")
            {
                if (!RtnRsn.StartsWith("M0"))
                {
                    response.Status = ServiceStatus.Failure;
                    response.Message = "Please select return reason!.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                mandate.RtnRsn = RtnRsn;
                mandate.TrnInd = "R";
                mandate.MandateStatus = "Return";
                isAccp = false;
            }
            else
            {
                mandate.TrnInd = "H";
                mandate.MandateStatus = "Hold";
                mandateService.Update(mandate);
                response.Message = "Mandate Hold Successfully";
                response.Status = ServiceStatus.Success;
            }
            mandate.Operation = "Update";
            mandate.ReqDt = DateTime.Now;
            mandate.AuthUser = StartUp.LOGIN_ID;
            mandate.AuthDt = StartUp.DAY_OP_DT;// DateTime.Now;

            //Create Mandate acceptance response file
            Account account = null;
            string brnCd = ""; string glcd = ""; string acno = "";
            if (Operation != "Hold")
            {
                if (mandate.AcNo.Length == 15)
                {
                    brnCd = mandate.AcNo.Substring(0, 4);
                    glcd = mandate.AcNo.Substring(4, 4);
                    acno = mandate.AcNo.Substring(8, 7);
                }
                account = AccountService.GetByNo(brnCd + glcd + acno);

                string applyChrg = mandateService.GetEhConfigByField("ECSCharges");
                if (isAccp == true)
                {
                    if (account == null)
                    {
                        response.Status = ServiceStatus.ValidationError;
                        response.Message = "account not found";
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    if (account.Customer.CustTyp.ToLower() != "preferred" && applyChrg.ToLower() == "true")
                    {

                        var chargesResponse = ApplyCharges("ECSIW", mandate.Amt, account, 0, mandate.MndtReqId, 1, "Mandate verification charges", mandate.UMRNo);

                        if (chargesResponse.Item)
                        {
                            response.Message = chargesResponse.Message;
                        }
                    }
                }
                else if (isAccp == false)
                {
                    if (account != null)
                    {
                        var rsnList = mandateService.GetAllReason("Accept", "All");
                        MandateReturnReason reason = rsnList.Where(a => a.RsnCd == RtnRsn).FirstOrDefault();

                        if (account.Customer.CustTyp.ToLower() != "preferred" && applyChrg.ToLower() == "true" && reason.ChgApl)
                        {
                            var chargesResponse = ApplyCharges("ECSIR", mandate.Amt, account, 0, mandate.MndtReqId, 1, "Mandate return charges", mandate.UMRNo);

                            if (chargesResponse.Item)
                            {
                                response.Message = chargesResponse.Message;
                            }
                        }
                    }
                }
                new MandateCommonService().GenerateMandateXML(mandate, isAccp);
                mandate.DtStamp = DateTime.Now;
                mandate.AuthDt = DateTime.Now;
                mandate.ExecDt = DateTime.Now;

                if (response.Message == null)
                {
                    response.Message = "Mandate Updated Successfully";
                    response.Status = ServiceStatus.Success;
                }
                mandateService.Update(mandate);
                SMSService sMSService = new SMSService();
                if (Operation == "Verify")
                {
                    if (account != null)
                    {
                        if (account.Customer.Address.Count > 0)
                        {
                            string mobileNo = account.Customer.Address.Find(a => !string.IsNullOrEmpty(a.Mobile)) == null ? string.Empty :
                                account.Customer.Address.Find(a => !string.IsNullOrEmpty(a.Mobile)).Mobile;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(mobileNo))
                                {
                                    SMS sms = new SMS();
                                    sms.AcId = account.Id;
                                    sms.MobNo = mobileNo;
                                    sms.Amt = mandate.Amt;
                                    sms.Name = account.Customer.Name;
                                    sms.AcNo = mandate.AcNo;
                                    sms.reason = "";
                                    sMSService.SendSms(sms, "ECSAcptIW");
                                }
                            }
                            catch (Exception ex)
                            {

                            }


                        }
                    }
                }

                //if (Operation == "Return")
                //{
                //    try
                //    {
                //        var Reason = "";
                //        List<MandateReturnReason> mandateReturnReasonList = new List<MandateReturnReason>();
                //        var ecsmandate = mandateService.GetByUmrn(mandate.UMRNo);
                //        mandateReturnReasonList = mandateService.GetReasonByReasonCode(RtnRsn);
                //        foreach (var rtn in mandateReturnReasonList)
                //        {
                //            Reason = rtn.Reason;
                //        }

                //        if (!string.IsNullOrWhiteSpace(ecsmandate.Mobile))
                //        {
                //            SMS sms = new SMS();
                //            sms.AcId = 0;
                //            sms.MobNo = ecsmandate.Mobile;
                //            sms.Amt = mandate.Amt;
                //            sms.Name = mandate.CrdtrName;
                //            sms.AcNo = mandate.AcNo;
                //            sms.reason = Reason;
                //            sMSService.SendSms(sms, "ECSRejIW");
                //        }
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //}
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public void CreateAck(ECS mandate, bool isAccept)
        {
            //Create XML file
            string xmlFileName;
            if (mandate.DetailFrntImg != string.Empty)
            {
                xmlFileName = mandate.DetailFrntImg.Substring(0, mandate.DetailFrntImg.IndexOf('_')); //need to change
            }
            else
            {
                xmlFileName = mandate.FrntImg.Substring(0, mandate.FrntImg.IndexOf('_'));
            }
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode productsNode = doc.CreateElement("MndtAccptncRpt");
            //doc.AppendChild(productsNode);

            XmlNode grpNode = doc.CreateElement("GrpHdr");
            //productsNode.AppendChild(productNode);

            XmlNode msgNode = doc.CreateElement("MsgId");//
            msgNode.InnerText = mandate.MsgId;

            grpNode.AppendChild(msgNode);

            XmlNode creNode = doc.CreateElement("CreDtTm");
            creNode.InnerText = DateTime.Now.ToString("s");
            grpNode.AppendChild(creNode);

            if (!string.IsNullOrEmpty(mandate.UserNACHId))
            {
                XmlNode initNode = doc.CreateElement("InitgPty");
                XmlNode initId = doc.CreateElement("Id");
                XmlNode initOrg = doc.CreateElement("OrgId");
                XmlNode initOthr = doc.CreateElement("Othr");
                XmlNode othrId = doc.CreateElement("Id");
                othrId.InnerText = mandate.UserNACHId;

                initOthr.AppendChild(othrId);
                initOrg.AppendChild(initOthr);
                initId.AppendChild(initOrg);
                initNode.AppendChild(initId);
                grpNode.AppendChild(initNode);
            }

            //Instructing Agent
            XmlNode instNode = doc.CreateElement("InstgAgt");
            XmlNode finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysNode = doc.CreateElement("ClrSysMmbId");
            XmlNode memIdNode = doc.CreateElement("MmbId");
            memIdNode.InnerText = mandate.BnkCd;

            clrSysNode.AppendChild(memIdNode);
            XmlNode name = doc.CreateElement("Nm");
            name.InnerText = mandate.BankName;

            finInstNode.AppendChild(clrSysNode);
            finInstNode.AppendChild(name);
            instNode.AppendChild(finInstNode);
            grpNode.AppendChild(instNode);

            //Instructed Agent
            XmlNode instdAgtNode = doc.CreateElement("InstdAgt");
            finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysMmbNode = doc.CreateElement("ClrSysMmbId");
            XmlNode mmbIdNode = doc.CreateElement("MmbId");
            mmbIdNode.InnerText = mandate.SpnsrBnkCd;

            clrSysMmbNode.AppendChild(mmbIdNode);
            XmlNode spnrName = doc.CreateElement("Nm");
            spnrName.InnerText = mandate.SpnsrBankName;

            finInstNode.AppendChild(clrSysMmbNode);
            finInstNode.AppendChild(spnrName);

            instdAgtNode.AppendChild(finInstNode);
            grpNode.AppendChild(instdAgtNode);
            productsNode.AppendChild(grpNode);
            //END GrpHdr tag

            //Underlying Amendment Details
            XmlNode underAccp = doc.CreateElement("UndrlygAccptncDtls");
            XmlNode msgInfo = doc.CreateElement("OrgnlMsgInf");
            XmlNode msgId = doc.CreateElement("MsgId");
            msgId.InnerText = mandate.MndtReqId;
            msgInfo.AppendChild(msgId);

            XmlNode msgNmId = doc.CreateElement("MsgNmId");
            msgNmId.InnerText = ""; //Dont know...need to discuss
            msgInfo.AppendChild(msgNmId);

            XmlNode creDt = doc.CreateElement("CreDtTm");
            creDt.InnerText = DateTime.Now.ToString("s");
            msgInfo.AppendChild(creDt);
            underAccp.AppendChild(msgInfo);

            //Acceptance Result
            XmlNode accResult = doc.CreateElement("AccptncRslt");
            XmlNode accpt = doc.CreateElement("Accptd");
            accpt.InnerText = isAccept.ToString(); //need to chk
            accResult.AppendChild(accpt);

            XmlNode rejRsn = doc.CreateElement("RjctRsn");
            XmlNode code = doc.CreateElement("Prtry");
            code.InnerText = "RejAccCode";//rejection/Acceptance code
            rejRsn.AppendChild(code);
            accResult.AppendChild(rejRsn);
            underAccp.AppendChild(accResult);

            XmlNode orgnMndt = doc.CreateElement("OrgnlMndt");
            XmlNode orgnMndtId = doc.CreateElement("OrgnlMndtId");
            orgnMndtId.InnerText = mandate.UMRNo;
            orgnMndt.AppendChild(orgnMndtId);
            underAccp.AppendChild(orgnMndt);
            productsNode.AppendChild(underAccp);

            doc.AppendChild(productsNode);

            string tempFilePath = Server.MapPath("~/img/ScanMandate");
            Directory.CreateDirectory(tempFilePath + @"\" + xmlFileName);

            //doc.Save(tempFilePath + @"\" + xmlFileName + @"\" + xmlFileName + "-INP.xml");

            using (TextWriter sw = new StreamWriter(tempFilePath + @"\" + xmlFileName + @"\" + xmlFileName + "-INP.xml"))//, false, Encoding.UTF8)) //Set encoding
            {
                doc.Save(sw);
            }
        }

        private void SetViewBag()
        {
            var rsnList = mandateService.GetAllReason("Accept", "All");
            ViewBag.ReasonList = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd });
        }

        private Response<bool> ApplyCharges(string chgId, decimal trnAmt, Account account, decimal amt, string refNo, int instance, string Narration, string UMRN)
        {
            Charges charges = null;
            MainBalance balance = null;
            var response = new Response<bool>();
            Response resp = new Response();

            TrnIdGenerator trnIdGenerator = null;
            DailyTransaction dailyTransaction = new DailyTransaction();
            List<DailyTransaction> dailyTransactionList = new List<DailyTransaction>();
            List<MainBalance> mainBalanceList = new List<MainBalance>();
            IChargesService chargesService = new ChargesService();
            IChargesSlabService chargesSlabService = new ChargesSlabService();
            IServiceTaxService serviceTaxService = new ServiceTaxService();
            ITrnIdGeneratorService trnIdGeneratorService = new TrnIdGeneratorService();
            List<Charges> chargesList = chargesService.GetAllChargesByChgId(chgId);
            bool isChargesApplied = false;
            response.Item = isChargesApplied;
            string setNo = string.Empty;

            foreach (Charges chg in chargesList)
            {
                if (chg.ApplGLCd.Contains(account.GLCd))
                {
                    charges = chg;
                    break;
                }
            }

            if (charges != null && charges.ApplGLCd.Contains(account.GLCd))
            {
                var accountList = new List<Account>();
                accountList.Add(account);
                List<MinimumBalance> refMiniBalList = new List<MinimumBalance>(); //pritam change
                List<GSTTransaction> gstTranList = new List<GSTTransaction>();
                List<PendingCharges> pendingChargesList = new List<PendingCharges>();
                List<AccountConfig> accConfigList = new List<AccountConfig>();

                //If not applicable to staff then do not deduct the charges.
                if (account.Customer.AcCat.ToLower().Contains("staff") && !charges.IsApplStaff)
                {
                    response.Item = isChargesApplied;
                    return response;
                }
                decimal chgs = 0;
                if (charges.ChgTyp == "SlabWise" && amt > 0)
                {
                    ChargesSlab chargesSlab = chargesSlabService.GetByAmt(charges.ChgId, amt);
                    if (chargesSlab != null)
                    {
                        if (charges.ChgId == ChargeId.STOP_PAYMENT)
                        {
                            chgs = amt * chargesSlab.MinChrg;
                            if (chgs > chargesSlab.MaxChrg)
                                chgs = chargesSlab.MaxChrg;
                        }
                        else if (chargesSlab.PFC == "F")
                            chgs = chargesSlab.MinChrg;
                        else
                        {
                            chgs = amt * chargesSlab.Percent / 100;
                            if (chgs > chargesSlab.MaxChrg) chgs = chargesSlab.MaxChrg;
                        }
                    }
                }
                else if (charges.ChgTyp == "Percent" && amt > 0)
                {
                    chgs = (amt * charges.ChgPer / 36500) * instance;
                }

                else if (charges.ChgTyp == "Both" && amt > 0)
                    chgs = (charges.ChgVal + (amt * charges.ChgPer / 36500)) * instance;
                else
                {
                    chgs = charges.ChgVal * instance;
                }
                chgs = Math.Round(chgs, MidpointRounding.AwayFromZero);



                if (chgs == 0)
                {
                    response.Status = ServiceStatus.ValidationError;
                    //return false;
                    return response;
                }

                var res = CalculateAndPostServiceTax(StartUp.BRN_CD, chgId, chgs, charges.Name, accountList, null, ref dailyTransactionList, ref mainBalanceList, ref refMiniBalList, null, null, false, ref gstTranList, ref pendingChargesList, ref accConfigList, refNo, Narration, UMRN);
                if (res.Status == ServiceStatus.ValidationError)
                {
                    return response;
                }

                //if Inter Branch handle it
                if (StartUp.BRN_CD != account.BaseBrn && dailyTransactionList.Count > 0)
                {
                    Response interBranchResponse = null;
                    //Akash changes
                    dailyTransaction = dailyTransactionList.Where(a => a.AcId == account.Id).FirstOrDefault();
                    dailyTransaction.TrnFlag = (StartUp.BRN_CD != account.BaseBrn) ? "O" : "R";
                    dailyTransaction.CustBrn = account.BaseBrn;
                    if (dailyTransaction.DbCr == TransactionType.DEBIT)
                    {
                        interBranchResponse = AuthenticateInterBranchTxTransactions(account.BaseBrn, StartUp.BRN_CD, dailyTransaction.Amt, ref dailyTransactionList, ref mainBalanceList, setNo, account);
                        if (interBranchResponse.Status == ServiceStatus.ValidationError)
                        {
                            response.Message = interBranchResponse.Message;
                            response.Status = interBranchResponse.Status;
                            return response;
                        }
                        setNo = interBranchResponse.Item.ToString();
                    }
                }
                if (dailyTransactionList.Count > 0)
                {
                    trnIdGenerator = trnIdGeneratorService.GetTrnNo(dailyTransactionList.Count, 1, 0);
                    Int64 trnNo = trnIdGenerator.TrnNo - dailyTransactionList.Count + 1;
                    //string setNo = StringUtils.PadLeft(trnIdGenerator.SetNo.ToString(), StartUp.TRN_LEN, '0');
                    if (string.IsNullOrWhiteSpace(setNo))
                    {
                        setNo = trnIdGenerator.SetNo.ToString().PadLeft(StartUp.TRN_LEN, '0');
                    }
                    foreach (DailyTransaction dlyTrn in dailyTransactionList)
                    {
                        dlyTrn.TrnNo = trnNo++.ToString().PadLeft(StartUp.TRN_LEN, '0');
                        dlyTrn.SetNo = setNo;
                    }

                    new DailyTransactionService().Update(dailyTransactionList, mainBalanceList);

                    //Oracle
                    List<DailyTransaction> postedDailyTranList = new DailyTransactionService().GetAllBySetNo(StartUp.BRN_CD, setNo, TransactionType.TRANSFER, Convert.ToDateTime(StartUp.DAY_OP_DT));

                    try
                    {
                        foreach (GSTTransaction gstTran in gstTranList)
                        {
                            gstTran.TrnId = postedDailyTranList.Where(d => d.AcId == Convert.ToInt32(gstTran.AcNo) && d.DbCr == TransactionType.DEBIT && d.Narr.StartsWith("To Chg")).FirstOrDefault().Id;
                        }
                        new GSTTransactionService().BulkInsert(gstTranList); //gstTran changes

                        foreach (PendingCharges penChg in pendingChargesList)
                        {
                            penChg.TrnId = postedDailyTranList.Where(d => d.AcId == Convert.ToInt32(penChg.AcNo) && d.DbCr == TransactionType.DEBIT && d.Narr.StartsWith("To Chg")).FirstOrDefault().Id;
                        }
                        new PendingChargesService().BulkUpdate(pendingChargesList);
                    }
                    catch
                    {

                    }
                    response.Message = charges.Name + " posted successfully ! \nPlease note the Set No : " + setNo;
                    isChargesApplied = true;
                }
            }
            response.Item = isChargesApplied;
            response.Status = ServiceStatus.Success;
            return response;
        }

        //Jan19:04 Add new parameter Narration for Inward Return reason
        private Response CalculateAndPostServiceTax(string brnCd, string chgId, decimal chgAmt, string chgName, List<Account> accountList, List<MinimumBalance> minimumBalList, ref List<DailyTransaction> dailyTransactionList, ref List<MainBalance> mainBalanceList, ref List<MinimumBalance> retMinimumBalList
            , Account crAccount, Account servTaxAccount, bool serTaxTAppl, ref List<GSTTransaction> gstTranList, ref List<PendingCharges> pendingChargesList, ref List<AccountConfig> accConfigList, string chqNo, string Narration, string UMRN)
        {
            Response response = new Response();
            decimal orgChgs = 0, orgServTax = 0, chgs = 0, servTax = 0, totChrgs = 0, totServTax = 0, totCess = 0, totCessAmt = 0;
            List<ServiceTaxCess> serviceTaxCessList = new List<ServiceTaxCess>();
            ServiceTax serviceTax = null;
            IGLMasterService glMasterService = new GLMasterService();
            IAccountService accountService = new AccountService();
            IChargesService chargesService = new ChargesService();
            IServiceTaxService serviceTaxService = new ServiceTaxService();
            IPendingChargesService pendingChargesService = new PendingChargesService();
            ITrnIdGeneratorService trnIdGeneratorService = new TrnIdGeneratorService();
            IDailyTransactionService dailyTransactionService = new DailyTransactionService();
            IAccountConfigService accountConfigService = new AccountConfigService();
            IAcStatusService acStatusService = new AcStatusService();
            var dailyTransaction = new DailyTransaction();
            IChargesSlabService chargesSlabService = new ChargesSlabService();
            var chgMainBalanceList = new List<MainBalance>();
            var chgDailyTransactionList = new List<DailyTransaction>();
            GSTTransaction gstTransaction;
            List<GSTTransaction> gstTransactionList = new List<GSTTransaction>();
            AccountConfig accountConfig = new AccountConfig();
            // List<PendingCharges> pendingChargesList = new List<PendingCharges>();
            decimal outChgAmt = 0;
            decimal cgst = 0, sgst = 0, igst = 0, ugst = 0;

            bool pending;
            int instance;

            List<Charges> chargesList = null; Charges charges = null;
            if (chgAmt == 0)
            {
                chargesList = chargesService.GetAllChargesByChgId(chgId);
                if (chargesList.Count == 0)  //(charges == null)            
                {
                    response.Message = "Service Charges not defined !";
                    response.Status = ServiceStatus.ValidationError;
                    return response;
                }
            }

            var acStatus = acStatusService.GetByName("Closed");
            if (accountList.Count == 0)
                return response;

            //for comment

            foreach (Account account in accountList)
            {
                decimal clrBal = 0;
                //CR1821:0 Balance account will skip
                if (account.MainBalance.ClrBal == 0) continue;

                charges = null;
                //Pritam for minimumbalance
                if (minimumBalList != null && minimumBalList.Count != 0)
                {
                    instance = 0;
                    var acMinBalanceList = minimumBalList.Where(m => m.AcId == account.Id);
                    foreach (MinimumBalance minBal in acMinBalanceList)
                    {
                        var existMinBalance = retMinimumBalList.Where(m => m.Id == minBal.Id).FirstOrDefault();

                        if (existMinBalance == null)
                        {
                            existMinBalance = minBal;
                        }
                        instance++;
                        existMinBalance.PostDt = StartUp.DAY_OP_DT;
                        existMinBalance.AuthUser = StartUp.LOGIN_ID;
                        retMinimumBalList.Add(existMinBalance);
                    }

                    if (instance == 0)
                        continue;
                }
                //End
                if (chgAmt == 0)
                {
                    foreach (Charges chg in chargesList)
                    {
                        if (chg.ApplGLCd.Contains(account.GLCd))
                        {
                            charges = chg;
                            break;
                        }

                    }
                    if (charges == null)
                    {
                        //For SMS charges posting
                        if (accConfigList.Count > 0)
                        {
                            var accConf = accConfigList.Where(a => a.AcId == account.Id).FirstOrDefault();
                            if (accConf != null) accConfigList.Remove(accConf);
                        }
                        charges = chargesList.FirstOrDefault();
                        //END
                        continue;
                    }

                    var glMaster = GLMasterService.GetByGLCode(account.GLCd);
                    if (glMaster == null)
                    {
                        response.Message = "GL Code not found !";
                        response.Status = ServiceStatus.ValidationError;
                        return response;
                    }

                    decimal amt = 0;
                    amt = charges.ChgVal;

                    if (charges != null && charges.ApplGLCd.Contains(account.GLCd))
                    {
                        if ((!charges.IsApplStaff && account.Customer.AcCat.ToLower().Contains("staff")) || account.Customer.CustTyp != "Normal" ||
                       account.AcStatId == acStatus.Id || account.AcClDt != null || (account.MainBalance.ClrBal < 0 &&
                       glMaster.GLGrp != GLGroup.CURRENT_DEPOSIT && glMaster.GLGrp != GLGroup.CASH_CREDIT && glMaster.GLGrp != GLGroup.OVERDRAFT && glMaster.GLGroup.DBTyp != GLGroup.TERM_LOAN) || account.MainBalance.ClrBal == 0)
                        {
                            //For SMS charges posting
                            if (accConfigList.Count > 0)
                            {
                                var accConf = accConfigList.Where(a => a.AcId == account.Id).FirstOrDefault();
                                if (accConf != null) accConfigList.Remove(accConf);
                            }
                            //END
                            continue;
                        }
                        if (charges.ChgTyp == "SlabWise" && amt > 0)
                        {
                            ChargesSlab chargesSlab = chargesSlabService.GetByAmt(charges.ChgId, amt);
                            if (chargesSlab != null)
                            {
                                if (chargesSlab.PFC == "F")
                                    chgs = chargesSlab.MinChrg;
                                else
                                {
                                    chgs = amt * chargesSlab.Percent / 100;
                                    if (chgs > chargesSlab.MaxChrg) chgs = chargesSlab.MaxChrg;
                                }
                            }
                        }
                        else if (charges.ChgTyp == "Percent" && amt > 0)
                            chgs = (amt * charges.ChgPer / 36500);
                        else if (charges.ChgTyp == "Both" && amt > 0)
                            chgs = (charges.ChgVal + (amt * charges.ChgPer / 36500));
                        else
                        {
                            chgs = charges.ChgVal;
                        }
                    }

                    orgChgs = charges.ChgVal;
                }
                else
                {
                    orgChgs = chgAmt;
                    if (!string.IsNullOrEmpty(chgId))
                        charges = chargesService.GetByChgId(chgId);
                }
                if (charges != null && charges.STAppl)
                {
                    CalculateServiceTax(orgChgs, out orgServTax, out serviceTaxCessList);
                }
                else if (charges == null && serTaxTAppl)
                {
                    CalculateServiceTax(orgChgs, out orgServTax, out serviceTaxCessList);
                }
                chgs = Math.Round(orgChgs, 2); //pritam ..round
                servTax = Math.Round(orgServTax, 2); //pritam ..round
                totCess = serviceTaxCessList.Sum(a => a.Amount);
                pending = false;


                //if (account.MainBalance.ClrBal >= 0 && account.MainBalance.ClrBal < (chgs + servTax + totCess) && account.GLMaster.GLGroup.DBTyp != GLGroup.TERM_LOAN)
                //{
                var mainbalance = chgMainBalanceList.Where(a => a.AcNo == account.Id.ToString()).FirstOrDefault();
                if (mainbalance == null)
                {
                    mainbalance = account.MainBalance;
                    chgMainBalanceList.Add(mainbalance);
                }
                if (mainbalance.ClrBal >= 0 && mainbalance.ClrBal < (chgs + servTax + totCess))// && account.GLMaster.GLGroup.DBTyp != GLGroup.TERM_LOAN)
                {
                    pending = true;
                    decimal roundDiff = 0;
                    decimal diffAmt = 0;
                    clrBal = mainbalance.ClrBal;
                    // Re-calculate charges
                    //Clrbal=10 chgs=100
                    chgs = Math.Round(clrBal * chgs / (chgs + totCess), 2);//pritam ..round
                    totCess = Math.Round(clrBal - chgs, 2);//pritam ...round
                                                           //servTax = Math.Round(account.MainBalance.ClrBal - (chgs + totCess), 2);
                    if (charges != null && charges.STAppl)
                    {
                        CalculateServiceTax(chgs, out orgServTax, out serviceTaxCessList);
                        if (totCess != serviceTaxCessList.Sum(a => a.Amount))
                        {
                            roundDiff = Math.Round(totCess / serviceTaxCessList.Count, 2);
                            diffAmt = totCess - serviceTaxCessList.Sum(a => a.Amount);
                            diffAmt = Math.Abs(diffAmt);
                        }
                    }
                    if (roundDiff != 0)
                    {
                        chgs += diffAmt;
                        totCess -= diffAmt;
                    }
                }
                if (string.IsNullOrEmpty(chgName))
                    chgName = charges == null ? chgName : charges.Name;
                // Debit Charges & Service Tax to Party A/c
                string narr;

                if (mainbalance.ClrBal < (chgs + servTax + totCess))
                {
                    dailyTransaction.Amt = mainbalance.ClrBal;
                }

                if (chgs + totCess > 0)
                {
                    dailyTransaction = new DailyTransaction();
                    dailyTransaction.TrnBrn = brnCd;
                    dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                    dailyTransaction.TrnType = charges.ChgId;
                    dailyTransaction.AcId = account.Id;
                    dailyTransaction.DbCr = TransactionType.DEBIT;
                    dailyTransaction.Amt = chgs + totCess;
                    dailyTransaction.EntryTyp = "N";
                    dailyTransaction.CustBrn = account.BaseBrn;
                    //Jan19:04
                    narr = "To " + chgName /*+ " " + chqNo*/;
                    if (Narration != string.Empty)
                        narr = "To " + chgName +/* " " + chqNo +*/ "(" + Narration + ")";
                    dailyTransaction.Narr = narr;//"To " + chgName + " " + chqNo+Narration;
                    dailyTransaction.BookCd = TransactionType.TRANSFER;
                    dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                    dailyTransaction.CompGen = "Y";
                    dailyTransaction.Clerk = StartUp.LOGIN_ID;
                    dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                    dailyTransaction.Operation = "Insert";
                    dailyTransaction.AcNo = account.AcNo;
                    dailyTransaction.TrnType = "ECSIW";
                    chgDailyTransactionList.Add(dailyTransaction);

                    // Update Party Balance

                    //if (mainbalance )

                    mainbalance.ClrBal -= dailyTransaction.Amt;


                    mainbalance.AuthUser = StartUp.LOGIN_ID;
                    //chgMainBalanceList.Add(mainbalance);

                    //[CR355]: Deepak Changes merged (4.4.4i)- Changes done for Pending charges

                    //Dec20: for pending charges
                    //var pendingChargesList = pendingChargesService.GetAllByAcNo(brnCd, account.GLCd, account.AcNo, frmDt, toDt);
                    //END

                    if (pending)
                    {
                        var pendingCharges = new PendingCharges();
                        pendingCharges.TrnId = dailyTransaction.Id;
                        pendingCharges.AcNo = account.Id.ToString();
                        pendingCharges.TrnDt = StartUp.DAY_OP_DT;
                        pendingCharges.Stat = TransactionType.PENDING;
                        pendingCharges.TrnAmt = orgChgs + orgServTax + serviceTaxCessList.Sum(a => a.Amount);
                        pendingCharges.RecdAmt = chgs + servTax + totCess;
                        pendingCharges.PendAmt = pendingCharges.TrnAmt - pendingCharges.RecdAmt;
                        pendingCharges.Narr = chgName;
                        pendingCharges.Deleted = "N";
                        pendingCharges.ChgTyp = charges.ChgId;
                        //oracle change
                        pendingChargesList.Add(pendingCharges);


                    }

                    //New GSTTransaction changes

                    if (chgs > 0 && charges.STAppl)
                    {
                        decimal diff = 0;
                        CalculateServiceTax(chgs, out outChgAmt, out serviceTaxCessList);
                        if (totCess != serviceTaxCessList.Sum(a => a.Amount))
                            diff = Math.Round(totCess / serviceTaxCessList.Count, 2); //pritam change round
                        foreach (var cess in serviceTaxCessList)
                        {
                            if (cess.Amount != 0)
                            {
                                var cessAccount = AccountService.GetByNo(brnCd + cess.GLCd + cess.AcNo);
                                if (null == cessAccount)
                                {
                                    response.Message = "Unable to Post the Charges." + cess.Name + " Control account (" + cess.GLCd + "/" + cess.AcNo + ") is not found.";
                                    response.Status = ServiceStatus.ValidationError;
                                    return response;
                                }
                                if (diff != 0)
                                {
                                    cess.Amount = diff;
                                }
                                if (cess.Amount == 0)
                                    continue;

                                if (cess.Name.StartsWith("S"))
                                    sgst = cess.Amount;
                                else if (cess.Name.StartsWith("C"))
                                    cgst = cess.Amount;
                                else if (cess.Name.StartsWith("I"))
                                    igst = cess.Amount;
                                else
                                    ugst = cess.Amount;
                                //pritam
                                //totGSTChr += cess.Amount;

                                //New GSTTransaction

                                //gstTransactionService.Insert(gstTransaction);
                            }
                        }
                        gstTransaction = new GSTTransaction();
                        gstTransaction.Id = 0;
                        gstTransaction.TrnDt = dailyTransaction.TrnDt;
                        gstTransaction.TrnId = dailyTransaction.Id;
                        gstTransaction.AcNo = account.Id.ToString();
                        gstTransaction.ChrgTyp = chgName;
                        gstTransaction.BookCd = dailyTransaction.BookCd;
                        gstTransaction.TrnAmt = dailyTransaction.Amt;
                        gstTransaction.SGST = sgst;
                        gstTransaction.CGST = cgst;
                        gstTransaction.IGST = igst;
                        gstTransaction.UGST = ugst;
                        gstTransaction.DtStamp = DateTime.Now;
                        gstTransaction.Operation = "Insert";
                        gstTransaction.Deleted = "N";
                        gstTransaction.AuthUser = StartUp.LOGIN_ID;
                        gstTransactionList.Add(gstTransaction);

                    }
                    //END
                }
                //pritam change Round
                totChrgs += Math.Round(chgs, 2);
                totServTax += Math.Round(servTax, 2);
                totCessAmt += Math.Round(totCess, 2);
            }

            //pritam 0.01 diff credit first gst 

            //narrtion for single account
            string newNarrtion = string.Empty;
            if (accountList.Count == 1)
            {
                var acc = accountList.FirstOrDefault();
                newNarrtion = "(" /*+ acc.BaseBrn + "/" + acc.GLCd + "/"*/ + acc.AcNo + ") UMRNo: " + UMRN;
            }
            //END
            decimal totGSTChr = 0;


            if (totChrgs > 0)
            {
                decimal diff = 0;

                CalculateServiceTax(totChrgs, out outChgAmt, out serviceTaxCessList);
                if (totCessAmt != serviceTaxCessList.Sum(a => a.Amount))
                    diff = Math.Round(totCessAmt / serviceTaxCessList.Count, 2); //pritam change round

                foreach (var cess in serviceTaxCessList)
                {
                    if (cess.Amount != 0)
                    {
                        var cessAccount = AccountService.GetByNo(brnCd + cess.GLCd + cess.AcNo);
                        if (null == cessAccount)
                        {
                            response.Message = "Unable to Post the Charges." + cess.Name + " Control account (" + cess.GLCd + "/" + cess.AcNo + ") is not found.";
                            response.Status = ServiceStatus.ValidationError;
                            return response;
                        }
                        if (diff != 0)
                        {
                            cess.Amount = diff;
                        }
                        if (cess.Amount == 0)
                            continue;

                        dailyTransaction = new DailyTransaction();
                        dailyTransaction.TrnBrn = brnCd;
                        dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                        dailyTransaction.TrnType = charges.ChgId;
                        dailyTransaction.AcId = cessAccount.Id;
                        dailyTransaction.DbCr = TransactionType.CREDIT;
                        dailyTransaction.Amt = cess.Amount;
                        dailyTransaction.EntryTyp = "N";
                        dailyTransaction.CustBrn = cessAccount.BaseBrn;
                        dailyTransaction.Narr = "By " + cess.Name + " (" + chgName + ") " + newNarrtion;
                        dailyTransaction.BookCd = TransactionType.TRANSFER;
                        dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                        dailyTransaction.CompGen = "Y";
                        dailyTransaction.Clerk = StartUp.LOGIN_ID;
                        dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                        dailyTransaction.Operation = "Insert";
                        dailyTransaction.AcNo = cessAccount.AcNo;
                        // dailyTransaction.TrnType = "ECSIW";
                        chgDailyTransactionList.Add(dailyTransaction);

                        // Update Service Tax Control A/c Balance

                        var mainBalance = chgMainBalanceList.Where(a => a.AcNo == cessAccount.Id.ToString()).FirstOrDefault();
                        if (mainBalance == null)
                        {
                            mainBalance = cessAccount.MainBalance;
                            chgMainBalanceList.Add(mainBalance);
                        }
                        mainBalance.ClrBal += cess.Amount;
                        mainBalance.AuthUser = StartUp.LOGIN_ID;
                        //chgMainBalanceList.Add(cessAccount.MainBalance);

                        //pritam
                        totGSTChr += cess.Amount;

                        //if (mainBalance.ClrBal < totChrgs)
                        //{
                        //    totChrgs = mainBalance.ClrBal;
                        //}
                    }
                }

                decimal totDebitAmt = totChrgs + totCessAmt;
                decimal newCharge = Math.Round(totDebitAmt - totGSTChr, 2);
                //END

                // Credit Charges Control A/c
                Account tmpAccount = null;
                if (charges != null)
                    tmpAccount = AccountService.GetByNo(brnCd + charges.ChgGLCd + charges.ChgAcNo);
                else if (crAccount != null)
                {
                    tmpAccount = crAccount;
                    if (null == tmpAccount)
                    {
                        response.Message = "Unable to Post the Charges. Charges Control account not found.";
                        response.Status = ServiceStatus.ValidationError;
                        return response;
                    }
                }
                if (null == tmpAccount)
                {
                    response.Message = "Unable to Post the Charges. Charges Control account not found.";
                    response.Status = ServiceStatus.ValidationError;
                    return response;
                }
                if (totChrgs != 0)
                {
                    dailyTransaction = new DailyTransaction();
                    dailyTransaction.TrnBrn = brnCd;
                    //dailyTransaction.TrnDt = DateTime.Now.Date;
                    dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
                    dailyTransaction.TrnType = charges.ChgId;
                    dailyTransaction.AcId = tmpAccount.Id;
                    dailyTransaction.DbCr = TransactionType.CREDIT;
                    dailyTransaction.Amt = newCharge;//totChrgs;  pritam change for 0.01 diff
                    dailyTransaction.EntryTyp = "N";
                    dailyTransaction.CustBrn = tmpAccount.BaseBrn;
                    dailyTransaction.Narr = "By " + chgName + newNarrtion;
                    dailyTransaction.BookCd = TransactionType.TRANSFER;
                    dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
                    dailyTransaction.CompGen = "Y";
                    dailyTransaction.Clerk = StartUp.LOGIN_ID;
                    dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
                    dailyTransaction.Operation = "Insert";
                    dailyTransaction.AcNo = tmpAccount.AcNo;
                    chgDailyTransactionList.Add(dailyTransaction);

                    // Update Charges Control A/c Balance

                    var mainBalance = chgMainBalanceList.Where(a => a.AcNo == tmpAccount.Id.ToString()).FirstOrDefault();
                    if (mainBalance == null)
                    {
                        mainBalance = tmpAccount.MainBalance;
                        chgMainBalanceList.Add(mainBalance);
                    }
                    mainBalance.ClrBal += newCharge;
                    mainBalance.AuthUser = StartUp.LOGIN_ID;
                    //chgMainBalanceList.Add(tmpAccount.MainBalance);
                }
            }
            if (chgDailyTransactionList.Count > 0 || chgMainBalanceList.Count > 0)
            {
                foreach (var daily in chgDailyTransactionList)
                    dailyTransactionList.Add(daily);
                foreach (var main in chgMainBalanceList)
                    mainBalanceList.Add(main);
                foreach (var gstTran in gstTransactionList)
                    gstTranList.Add(gstTran);
            }

            return response;

        }
        private void CalculateServiceTax(decimal orgChgs, out decimal orgServTax, out List<ServiceTaxCess> serviceTaxCesList)
        {
            IServiceTaxService serviceTaxService = new ServiceTaxService();
            serviceTaxCesList = new List<ServiceTaxCess>();
            var serviceTax = serviceTaxService.GetByEffectDt(StartUp.DAY_OP_DT);
            orgServTax = orgChgs * (serviceTax.ServTax + (serviceTax.ServTax * serviceTax.EduCess / 100)) / 100;
            var cessList = serviceTaxService.GetAllCess("", serviceTax.Id.ToString(), StartUp.DAY_OP_DT);
            if (cessList.Count() > 0)
            {
                foreach (var cess in cessList)
                {
                    cess.Amount = orgChgs * cess.Percent / 100;
                    if (serviceTax.RndOff)
                        cess.Amount = Math.Round(cess.Amount, MidpointRounding.AwayFromZero);
                    else
                        cess.Amount = Math.Round(cess.Amount, 2);

                    serviceTaxCesList.Add(cess);
                }
            }
            orgServTax = 0;
        }


        private Response AuthenticateInterBranchTxTransactions(string creditBrnCd, string debitBrnCd, decimal amount, ref List<DailyTransaction> dailyTransactionList, ref List<MainBalance> mainBalanceList, string setNo, Account account)
        {
            Response response = new Response();
            IBranchService branchService = new BranchService();
            ITrnIdGeneratorService trnIdGeneratorService = new TrnIdGeneratorService();
            //IDailyTransactionService dailyTransactionService = new DailyTransactionService();

            bool isHeadOffice = false;
            string creditBrnAdjGLCd, creditBrnAdjAcNo, debitBrnAdjGLCd, debitBrnAdjAcNo = string.Empty;
            string trnNo = string.Empty;
            //var dailyTransactionList = new List<DailyTransaction>();
            //var mainBalanceList = new List<MainBalance>();

            //Get InterBranch Adjustment Accounts
            var creditBranch = branchService.GetByCode(creditBrnCd);
            var debitBranch = branchService.GetByCode(debitBrnCd);

            creditBrnAdjGLCd = creditBranch.InterBrnAdjGLCd == null ? string.Empty : creditBranch.InterBrnAdjGLCd;
            creditBrnAdjAcNo = creditBranch.InterBrnAdjAcNo == null ? string.Empty : creditBranch.InterBrnAdjAcNo;
            debitBrnAdjGLCd = debitBranch.InterBrnAdjGLCd == null ? string.Empty : debitBranch.InterBrnAdjGLCd;
            debitBrnAdjAcNo = debitBranch.InterBrnAdjAcNo == null ? string.Empty : debitBranch.InterBrnAdjAcNo;

            //Inter branch Adjustment A/c of debit Branch
            Account interBranchAdjustmentAccount = null;
            //[CR398]: InterBranch Transaction with Headoffice as one of the transaction
            if (debitBrnCd == StartUp.HEAD_OFFICE_BRN_CD)
            {
                interBranchAdjustmentAccount = AccountService.GetByNo(debitBrnCd + StartUp.HEAD_GLCd + StringUtils.PadLeft(creditBrnCd, StartUp.AC_LEN - StartUp.BRN_LEN - StartUp.GL_LEN, '0'));
                //var accountno = (debitBrnCd + StartUp.HEAD_GLCd + StringUtils.PadLeft(creditBrnCd, StartUp.AC_LEN-StartUp.BRN_LEN-StartUp.GL_LEN, '0'));
                //interBranchAdjustmentAccount = AccountService.GetByNo(accountno);
                if (interBranchAdjustmentAccount == null)
                {
                    response.Message = "H/O A/c for branch " + creditBrnCd + " does not exist.";
                    response.Status = ServiceStatus.ValidationError;
                    return response;
                }
                isHeadOffice = true;
            }
            else
            {
                interBranchAdjustmentAccount = AccountService.GetByNo(debitBrnCd + debitBrnAdjGLCd + debitBrnAdjAcNo);
                if (interBranchAdjustmentAccount == null)
                {
                    response.Message = "Inter Branch Adjustment A/c for branch " + debitBrnCd + " does not exist.";
                    response.Status = ServiceStatus.ValidationError;
                    return response;
                }
            }

            if (string.IsNullOrEmpty(setNo))
            {
                TrnIdGenerator trnIdGenerator = trnIdGeneratorService.GetTrnNo(1, 1, 0);
                //setNo = StringUtils.PadLeft(trnIdGenerator.SetNo.ToString(), StartUp.TRN_LEN, '0');
                //setNo = "IB" + StartUp.BRN_CD + trnIdGenerator.SetNo;
                setNo = "IB" + StringUtils.PadLeft(trnIdGenerator.SetNo.ToString(), StartUp.TRN_LEN, '0');
                ////[CR373]: For inter branch transactions, ledger of the account shows incorrect sequence of the transactions resulting in wrong balance calculations.  
                //txtSetNo.Text += "IBX";
                trnNo = trnIdGeneratorService.GetTrnNo(interBranchAdjustmentAccount.BaseBrn, 1, 0, 0).TrnNo.ToString();
                trnNo = StringUtils.PadLeft(trnIdGenerator.TrnNo.ToString(), StartUp.TRN_LEN, '0');
            }
            else
            {
                //trnNo = trnIdGeneratorService.GetTrnNo(1, 0, 0).TrnNo.ToString();
                //[CR373]: For inter branch transactions, ledger of the account shows incorrect sequence of the transactions resulting in wrong balance calculations.  
                trnNo = trnIdGeneratorService.GetTrnNo(interBranchAdjustmentAccount.BaseBrn, 1, 0, 0).TrnNo.ToString();
                trnNo = StringUtils.PadLeft(trnNo, StartUp.TRN_LEN, '0');
            }

            DailyTransaction dailyTransaction = new DailyTransaction();
            dailyTransaction.TrnBrn = StartUp.BRN_CD;
            dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
            dailyTransaction.TrnNo = trnNo;
            dailyTransaction.SetNo = setNo;
            dailyTransaction.BookCd = TransactionType.TRANSFER;
            dailyTransaction.DbCr = TransactionType.DEBIT;
            dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
            dailyTransaction.AcId = interBranchAdjustmentAccount.Id;
            dailyTransaction.AcNo = interBranchAdjustmentAccount.AcNo;
            dailyTransaction.Amt = amount;
            dailyTransaction.EntryTyp = "N";
            dailyTransaction.TrnType = "ECSIW";
            //[CR361]: Correct the Inter Branch transaction Narration
            //dailyTransaction.Narr = "To-(Trf) " + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == creditBrnCd).Name + " Branch";
            // [CRA16] Branch code for inter branch Trnascrtion
            //dailyTransaction.Narr = "To-(Trf) " + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == creditBrnCd).Name + " for CB:" + debitBrnCd;
            //Inter Branch Tran Narration with customer Name
            if (StartUp.ALLOW_INTER_BRANCH_TRAN_NARRATION_WITH_CUST_NAME)
            {
                // InterBranch Narration is wrong
                //This throws error..
                //var crAccount = AccountService.GetById(dailyTransactionList.Where(d => d.DbCr == TransactionType.CREDIT).First().AcId);

                //dailyTransaction.Narr = "To TRF/" + brnName + " (" + crAccount.BrnCd + crAccount.GLCd + crAccount.AcNo + ")";                
                if (creditBrnCd == StartUp.BRN_CD)
                {
                    // dailyTransaction.Narr = "To TRF/" + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == creditBrnCd).Name;
                    dailyTransaction.Narr = "To TRF/" + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == creditBrnCd).Name + " (" /*+ account.BaseBrn + account.GLCd*/ + account.AcNo + ")";

                }
                else
                {
                    //dailyTransaction.Narr = "To TRF/" + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == debitBrnCd).Name;
                    dailyTransaction.Narr = "To TRF/" + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == debitBrnCd).Name + " (" /*+ account.BaseBrn + account.GLCd*/ + account.AcNo + ")";

                }
            }
            else
            {
                if (creditBrnCd == StartUp.BRN_CD)
                {
                    dailyTransaction.Narr = " IB (TRF) To- TB" + creditBrnCd + " for CB:" + debitBrnCd;
                }
                else
                {
                    dailyTransaction.Narr = " IB (TRF) To- TB" + debitBrnCd + " for CB:" + creditBrnCd;
                }
            }
            dailyTransaction.CompGen = "Y";
            dailyTransaction.Clerk = StartUp.LOGIN_ID;
            dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Acnt = StartUp.LOGIN_ID;
            dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Operation = "Insert";
            dailyTransaction.CustBrn = interBranchAdjustmentAccount.BaseBrn;
            dailyTransaction.TrnFlag = (StartUp.BRN_CD == debitBrnCd) ? "O" : "R";
            dailyTransactionList.Add(dailyTransaction);

            //oracle changes
            var existingMainBal = mainBalanceList.Where(a => a.AcNo == interBranchAdjustmentAccount.Id.ToString()).LastOrDefault();
            if (existingMainBal == null)
            {
                existingMainBal = interBranchAdjustmentAccount.MainBalance;
                mainBalanceList.Add(existingMainBal);
            }

            existingMainBal.ClrBal -= dailyTransaction.Amt;
            existingMainBal.AuthUser = StartUp.LOGIN_ID;
            //mainBalanceList.Add(interBranchAdjustmentAccount.MainBalance);

            //Inter branch Adjustment A/c of credit Branch
            //[CR398]: InterBranch Transaction with Headoffice as one of the transaction
            if (creditBrnCd == StartUp.HEAD_OFFICE_BRN_CD)
            {
                interBranchAdjustmentAccount = AccountService.GetByNo(creditBrnCd + StartUp.HEAD_GLCd + StringUtils.PadLeft(debitBrnCd, StartUp.AC_LEN, '0'));
                if (interBranchAdjustmentAccount == null)
                {
                    response.Message = "H/O A/c for branch " + debitBrnCd + " does not exist.";
                    response.Status = ServiceStatus.ValidationError;
                    return response;
                }
                isHeadOffice = true;
            }
            else
            {
                interBranchAdjustmentAccount = AccountService.GetByNo(creditBrnCd + creditBrnAdjGLCd + creditBrnAdjAcNo);
                if (interBranchAdjustmentAccount == null)
                {
                    response.Message = "Inter Branch Adjustment A/c for branch " + creditBrnCd + " does not exist.";
                    response.Status = ServiceStatus.ValidationError;
                    return response;
                }
            }

            //[CR373]: For inter branch transactions, ledger of the account shows incorrect sequence of the transactions resulting in wrong balance calculations.  
            //trnNo = trnIdGeneratorService.GetTrnNo(1, 0, 0).TrnNo.ToString();            
            trnNo = trnIdGeneratorService.GetTrnNo(interBranchAdjustmentAccount.BaseBrn, 1, 0, 0).TrnNo.ToString();
            trnNo = StringUtils.PadLeft(trnNo, StartUp.TRN_LEN, '0');

            dailyTransaction = new DailyTransaction();
            dailyTransaction.TrnBrn = StartUp.BRN_CD;
            dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
            dailyTransaction.TrnNo = trnNo;
            dailyTransaction.SetNo = setNo;
            dailyTransaction.BookCd = TransactionType.TRANSFER;
            dailyTransaction.DbCr = TransactionType.CREDIT;
            dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
            dailyTransaction.AcId = interBranchAdjustmentAccount.Id;
            dailyTransaction.Amt = amount;
            dailyTransaction.EntryTyp = "N";
            dailyTransaction.AcNo = interBranchAdjustmentAccount.AcNo;
            dailyTransaction.TrnType = "ECSIW";
            //dailyTransaction.Narr = "By " + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == debitBrnCd).Name + " Branch";
            // [CRA16] Branch code for inter branch Trnascrtion
            //dailyTransaction.Narr = "By-(Trf) " + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == debitBrnCd).Name + " From CB:" + creditBrnCd;
            //Inter Branch Tran Narration with customer Name
            if (StartUp.ALLOW_INTER_BRANCH_TRAN_NARRATION_WITH_CUST_NAME)
            {
                dailyTransaction.Narr = "By TRF/" + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == debitBrnCd).Name + " (" /*+ account.BaseBrn + account.GLCd*/ + account.AcNo + ")";
            }
            else
            {
                if (creditBrnCd == StartUp.BRN_CD)
                {
                    dailyTransaction.Narr = " IB (TRF) To- TB" + creditBrnCd + " for CB:" + debitBrnCd;
                }
                else
                {
                    dailyTransaction.Narr = " IB (TRF) To- TB" + debitBrnCd + " for CB:" + creditBrnCd;
                }
            }
            dailyTransaction.CompGen = "Y";
            dailyTransaction.Clerk = StartUp.LOGIN_ID;
            dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Acnt = StartUp.LOGIN_ID;
            dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Operation = "Insert";
            dailyTransaction.CustBrn = interBranchAdjustmentAccount.BaseBrn;
            dailyTransaction.TrnFlag = (StartUp.BRN_CD == creditBrnCd) ? "O" : "R";
            dailyTransactionList.Add(dailyTransaction);

            existingMainBal = mainBalanceList.Where(a => a.AcNo == interBranchAdjustmentAccount.Id.ToString()).LastOrDefault();
            if (existingMainBal == null)
            {
                existingMainBal = interBranchAdjustmentAccount.MainBalance;
                mainBalanceList.Add(existingMainBal);
            }

            existingMainBal.ClrBal += dailyTransaction.Amt;
            existingMainBal.AuthUser = StartUp.LOGIN_ID;
            //mainBalanceList.Add(interBranchAdjustmentAccount.MainBalance);

            //[CR219E]: Inter Branch- Add Contra Entry in HO Branch
            if (!isHeadOffice)
            {
                var interBranchResponse = InterBranchTransactions(debitBrnCd, creditBrnCd, amount, account, ref dailyTransactionList, ref mainBalanceList, setNo, TransactionType.TRANSFER);
                if (interBranchResponse.Status == ServiceStatus.ValidationError)
                {
                    return interBranchResponse;
                }
                setNo = interBranchResponse.Item.ToString();
            }
            if (!string.IsNullOrEmpty(setNo))
            {
                response.Item = setNo;
            }
            return response;
        }

        public Response InterBranchTransactions(string creditBrnCd, string debitBrnCd, decimal amount, Account customerAccount, ref List<DailyTransaction> dailyTransactionList, ref List<MainBalance> mainBalanceList, string setNo, string interBranchTransactionType)
        {
            Response response = new Response();
            IBranchService branchService = new BranchService();
            ITrnIdGeneratorService trnIdGeneratorService = new TrnIdGeneratorService();

            string trnNo = string.Empty;
            //Get HO Accounts having Branch code as A/c No
            string hoBranchCode = StartUp.HEAD_OFFICE_BRN_CD;

            if (string.IsNullOrEmpty(setNo))
            {
                TrnIdGenerator trnIdGenerator = trnIdGeneratorService.GetTrnNo(hoBranchCode, 1, 1, 0);
                setNo = StringUtils.PadLeft(trnIdGenerator.SetNo.ToString(), StartUp.TRN_LEN, '0');
                trnNo = StringUtils.PadLeft(trnIdGenerator.TrnNo.ToString(), StartUp.TRN_LEN, '0');
            }
            else
            {
                trnNo = trnIdGeneratorService.GetTrnNo(hoBranchCode, 1, 0, 0).TrnNo.ToString();
                trnNo = StringUtils.PadLeft(trnNo, StartUp.TRN_LEN, '0');
            }

            if (string.IsNullOrEmpty(hoBranchCode))
            {
                response.Message = "HO Branch Code is not set in System Parameter.";
                response.Status = ServiceStatus.ValidationError;
                return response;
            }
            string headGlCode = StartUp.HEAD_GLCd;
            if (string.IsNullOrEmpty(headGlCode))
            {
                response.Message = "HO GL Code is not set in System Parameter.";
                response.Status = ServiceStatus.ValidationError;
                return response;
            }

            var debitHOAccount = AccountService.GetByNo(hoBranchCode + headGlCode + StringUtils.PadLeft(debitBrnCd, StartUp.AC_LEN, '0'));

            //debitHOAccount         
            if (debitHOAccount == null)
            {
                response.Message = "HO A/c for branch " + debitBrnCd + " does not exist.";
                response.Status = ServiceStatus.ValidationError;
                return response;
            }
            //creditHOAccount
            var creditHOAccount = AccountService.GetByNo(hoBranchCode + headGlCode + StringUtils.PadLeft(creditBrnCd, StartUp.AC_LEN, '0'));
            if (creditHOAccount == null)
            {
                response.Message = "HO A/c for branch " + creditBrnCd + " does not exist.";
                response.Status = ServiceStatus.ValidationError;
                return response;
            }

            DailyTransaction dailyTransaction = new DailyTransaction();
            dailyTransaction.TrnBrn = StartUp.BRN_CD;
            dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
            dailyTransaction.TrnNo = trnNo;
            dailyTransaction.SetNo = setNo;
            dailyTransaction.BookCd = TransactionType.TRANSFER;
            dailyTransaction.DbCr = TransactionType.DEBIT;
            dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
            dailyTransaction.AcId = debitHOAccount.Id;
            dailyTransaction.Amt = amount;
            dailyTransaction.EntryTyp = "N";
            dailyTransaction.AcNo = debitHOAccount.AcNo;
            dailyTransaction.TrnType = "ECSIW";
            //dailyTransaction.Narr = "To " + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == creditBrnCd).Name + " Branch for A/c " + customerAccount.GLCd + "/" + customerAccount.AcNo;
            // [CRA16] Branch code for inter branch Trnascrtion
            //dailyTransaction.Narr = "To " + "CB:" + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == creditBrnCd).Name + " Branch for A/c " + customerAccount.GLCd + "/" + customerAccount.AcNo + "To TB:" + StartUp.BRN_CD;

            //Inter Branch Tran Narration with customer Name
            if (StartUp.ALLOW_INTER_BRANCH_TRAN_NARRATION_WITH_CUST_NAME)
            {
                dailyTransaction.Narr = "To " + interBranchTransactionType + "/" + creditHOAccount.Name + " (" + customerAccount.BaseBrn + customerAccount.GLCd + customerAccount.AcNo + ")";
            }
            else
            {
                if (interBranchTransactionType == TransactionType.CASH_RECEIPT)
                    dailyTransaction.Narr = "IB (CR-TRF) To  TB:" + debitBrnCd + " for  CB: " + creditBrnCd;
                else if (interBranchTransactionType == TransactionType.CASH_PAYMENT)
                    dailyTransaction.Narr = "IB (CP-TRF) To  TB:" + creditBrnCd + " for  CB: " + debitBrnCd;
                else if (interBranchTransactionType == TransactionType.TRANSFER)
                {
                    if (debitBrnCd == StartUp.BRN_CD)
                        dailyTransaction.Narr = "IB (TX-TRF) To  TB:" + debitBrnCd + " for  CB: " + creditBrnCd;
                    else
                        dailyTransaction.Narr = "IB (TX-TRF) To  TB:" + creditBrnCd + " for  CB: " + debitBrnCd;
                }
                else
                {
                    dailyTransaction.Narr = "IB (TRF) To  TB:" + debitBrnCd + " for  CB: " + creditBrnCd;
                }
            }
            dailyTransaction.CompGen = "Y";
            dailyTransaction.Clerk = StartUp.LOGIN_ID;
            dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Acnt = StartUp.LOGIN_ID;
            dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Operation = "Insert";
            dailyTransaction.CustBrn = customerAccount.BaseBrn;
            dailyTransaction.TrnFlag = (StartUp.BRN_CD == debitBrnCd) ? "O" : "R";
            dailyTransactionList.Add(dailyTransaction);

            //oracle changes
            var existingMainBal = mainBalanceList.Where(a => a.AcNo == debitHOAccount.Id.ToString()).LastOrDefault();
            if (existingMainBal == null)
            {
                existingMainBal = debitHOAccount.MainBalance;
                mainBalanceList.Add(existingMainBal);
            }

            existingMainBal.ClrBal -= dailyTransaction.Amt;
            existingMainBal.AuthUser = StartUp.LOGIN_ID;
            //mainBalanceList.Add(debitHOAccount.MainBalance);


            trnNo = trnIdGeneratorService.GetTrnNo(hoBranchCode, 1, 0, 0).TrnNo.ToString();
            trnNo = StringUtils.PadLeft(trnNo, StartUp.TRN_LEN, '0');

            dailyTransaction = new DailyTransaction();
            dailyTransaction.TrnBrn = StartUp.BRN_CD;
            dailyTransaction.TrnDt = StartUp.DAY_OP_DT;
            dailyTransaction.TrnNo = trnNo;
            dailyTransaction.SetNo = setNo;
            dailyTransaction.BookCd = TransactionType.TRANSFER;
            dailyTransaction.DbCr = TransactionType.CREDIT;
            dailyTransaction.TrnInd = TransactionType.AUTHENTICATE;
            dailyTransaction.AcId = creditHOAccount.Id;
            dailyTransaction.Amt = amount;
            dailyTransaction.EntryTyp = "N";
            dailyTransaction.AcNo = creditHOAccount.AcNo;
            dailyTransaction.TrnType = "ECSIW";
            //[CR361]: Correct the Inter Branch transaction Narration
            //dailyTransaction.Narr = "By " + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == customerAccount.BrnCd).Name + " Branch";
            // [CRA16] Branch code for inter branch Trnascrtion
            //dailyTransaction.Narr = "By CB:" + StartUp.BRN_LIST.SingleOrDefault(b => b.BrnCd == debitBrnCd).Name + " Branch for Account " + customerAccount.GLCd + "/" + customerAccount.AcNo;
            //Inter Branch Tran Narration with customer Name
            if (StartUp.ALLOW_INTER_BRANCH_TRAN_NARRATION_WITH_CUST_NAME)
            {
                dailyTransaction.Narr = "By " + interBranchTransactionType + "/" + debitHOAccount.Name + " (" + customerAccount.BaseBrn + customerAccount.GLCd + customerAccount.AcNo + ")";
            }
            else
            {
                if (interBranchTransactionType == TransactionType.CASH_RECEIPT)
                    dailyTransaction.Narr = "IB (CR-TRF) By  TB:" + debitBrnCd + " for  CB: " + creditBrnCd;
                else if (interBranchTransactionType == TransactionType.CASH_PAYMENT)
                    dailyTransaction.Narr = "IB (CP-TRF) By  TB:" + creditBrnCd + " for  CB: " + debitBrnCd;
                else if (interBranchTransactionType == TransactionType.TRANSFER)
                {
                    if (debitBrnCd == StartUp.BRN_CD)
                        dailyTransaction.Narr = "IB (TX-TRF) To  TB:" + debitBrnCd + " for  CB: " + creditBrnCd;
                    else
                        dailyTransaction.Narr = "IB (TX-TRF) To  TB:" + creditBrnCd + " for  CB: " + debitBrnCd;
                }
                else
                {
                    dailyTransaction.Narr = "IB (TRF) By  TB:" + debitBrnCd + " for  CB: " + creditBrnCd;
                }
            }
            dailyTransaction.CompGen = "Y";
            dailyTransaction.Clerk = StartUp.LOGIN_ID;
            dailyTransaction.ClerkTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Acnt = StartUp.LOGIN_ID;
            dailyTransaction.AcntTime = DateTime.Now.ToShortTimeString();
            dailyTransaction.Operation = "Insert";
            dailyTransaction.CustBrn = customerAccount.BaseBrn;
            dailyTransaction.TrnFlag = (StartUp.BRN_CD == creditBrnCd) ? "O" : "R";
            dailyTransactionList.Add(dailyTransaction);

            existingMainBal = mainBalanceList.Where(a => a.AcNo == creditHOAccount.Id.ToString()).LastOrDefault();
            if (existingMainBal == null)
            {
                existingMainBal = creditHOAccount.MainBalance;
                mainBalanceList.Add(existingMainBal);
            }
            existingMainBal.ClrBal += dailyTransaction.Amt;
            existingMainBal.AuthUser = StartUp.LOGIN_ID;
            //mainBalanceList.Add(creditHOAccount.MainBalance);

            response.Item = setNo;

            //return setNo;

            return response;
        }

    }



}