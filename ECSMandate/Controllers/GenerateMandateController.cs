using EasyBank.BusinessLayer.Accounts.Objects;
using EasyBank.BusinessLayer.Accounts.Services;
using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EasyBank.Framework.EasyExtensions;
using EasyBank.Infrastructure.SystemParameters;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.BussinessLayer.ECSService.Object;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace ECSMandate.Controllers
{
    public class GenerateMandateController : AppController
    {
        MandateService mandateService = new MandateService();
        // GET: GenerateMandate
        public ActionResult Index()
        {
            List<MandateCategory> categoryList = new List<MandateCategory>();
            var typList = MandateCommonService.GetAllAcTyp();
            var freqList = MandateCommonService.GetAllFreq();
            var drTypList = MandateCommonService.GetAllDrTyp();
            var seqList = MandateCommonService.GetAllSquence();

            var category = mandateService.GetAllCategory().Where(a => a.Descrp.ToLower().Contains("installment pay")).FirstOrDefault();
            categoryList.Add(category);
            categoryList.AddRange(mandateService.GetAllCategory().Where(a => !a.Descrp.ToLower().Contains("installment pay")).ToList());
            ViewBag.UtilityCode = mandateService.GetEhConfigByField("Utility_Code");

            ViewBag.AcTypList = typList;
            ViewBag.FrqList = freqList;
            ViewBag.DrTypList = drTypList;
            ViewBag.CategoryList = categoryList;
            ViewBag.seqList = seqList;
            ViewBag.Brncd = StartUp.BRN_CD;

            string pathName = ConfigStartUp.MANDATE_SCAN_PATH + "\\" + StartUp.BRN_CD;
            string tempFilePath = Server.MapPath("~/img/ScanMandate") + "\\" + StartUp.BRN_CD;
            string[] tempFiles = null;
            string[] imagFiles = null;

            try
            {
                tempFiles = Directory.GetFiles(tempFilePath, "*.jpg");
                if (tempFiles.Count() == 0)
                    imagFiles = Directory.GetFiles(pathName, "*.jpg");
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new DirectoryNotFoundException(ex.Message + " path :" + pathName);
            }

            var mandateList = mandateService.GetUnAuthMandate("OW");

            ViewBag.MandateList = mandateList.Select(m => new { Id = m.Id, MandateFile = m.MandateFile });

            ViewBag.CurDate = StartUp.DAY_OP_DT;// DateTime.Now.Date;

            if (tempFiles != null && tempFiles.Count() > 0)
            {
                string greyImgPath = tempFiles.First();
                var gryImg = Path.GetFileName(greyImgPath);
                var fileName = gryImg.Substring(0, gryImg.IndexOf('.'));
                string bitonlImgPath = pathName + "\\" + fileName + "_Bitonal.tiff";
                //System.IO.File.Move(greyImgPath, tempFilePath + "\\" + gryImg);
                //System.IO.File.Move(bitonlImgPath, tempFilePath + "\\" + fileName + "_Bitonal.tiff");
                ViewBag.GreyImg = gryImg;
                ViewBag.BitonalImg = fileName + "_Bitonal.tiff";
                ViewBag.ImageName = fileName;
            }
            else if (imagFiles != null && imagFiles.Count() > 0)
            {
                string greyImgPath = imagFiles.First();
                var gryImg = Path.GetFileName(greyImgPath);
                var fileName = gryImg.Substring(0, gryImg.IndexOf('.'));
                string bitonlImgPath = pathName + "\\" + fileName + "_Bitonal.tiff";
                System.IO.File.Move(greyImgPath, tempFilePath + "\\" + gryImg);
                System.IO.File.Move(bitonlImgPath, tempFilePath + "\\" + fileName + "_Bitonal.tiff");
                ViewBag.GreyImg = gryImg;
                ViewBag.BitonalImg = fileName + "_Bitonal.tiff";
                ViewBag.ImageName = fileName;
            }
            return View();
        }

        public ActionResult ValidateAmt(decimal amount)
        {
            Response response = new Response();
            string numberInWord = string.Empty;
            if (amount != 0)
            {
                numberInWord = amount.NumberToWord();
            }

           
            response.Item = numberInWord;
            return Json(response);
        }

        public ActionResult Submit(ECS ecsMandate, string ImageName, string UMRDt, string DbTyp, string Id)
        {
            Response response = new Response();

            if (ImageName == "")
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Image not found";
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(ecsMandate.Category))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Image not found";
                return Json(response);
            }

            if (ecsMandate.UMRDt > StartUp.DAY_OP_DT)
            {
                //response.Status = ServiceStatus.ValidationError;
                //response.Message = "UMR date must be less than current date";
                //return Json(response);
            }
            MandateService mandateService = new MandateService();
            var dtArry = UMRDt.Split('/');


            if (ecsMandate.IsNew)
            {
                GlobalIdGenerator globalIdGenerator = ecsMandate.GlobalIdGenerator;
                var ecsId = Convert.ToInt32(globalIdGenerator.LastNo);
                ecsMandate.Id = ecsId;
            }
            else
            {
                ecsMandate.Id = Id.ToInt();
            }

            ecsMandate.UtilityCd = mandateService.GetEhConfigByField("Utility_Code");
            ecsMandate.UMRDt = Convert.ToDateTime(UMRDt);
            ecsMandate.DtStamp = StartUp.DAY_OP_DT;// DateTime.Now;
            ecsMandate.Clerk = StartUp.LOGIN_ID;
            ecsMandate.TrnInd = "S";
            ecsMandate.MandateTyp = "OW";
            ecsMandate.BnkCd = ecsMandate.BnkCd.ToUpper();
            ecsMandate.Deleted = "N";
            Account account = AccountService.GetByNo( ecsMandate.CrAcNo);
            ecsMandate.CrdtrName = account.Name;
            string newImageFile = MandateCommonService.CreateXMLName(ecsMandate.MandateFile, "XML", ECS.CREATE);//  "MMS-CREATE-" + ecsMandate.BnkCd+ "-" + StartUp.LOGIN_ID;
            ecsMandate.MandateFile = newImageFile;
            ecsMandate.DetailFrntImg = newImageFile + "_detailfront.jpg";
            ecsMandate.FrntImg = newImageFile + "_front.tiff";
            ecsMandate.ExecDt = DateTime.Now;
            ecsMandate.Variant = "Normal";

            // var SpnsrBnkCd1 = StartUp.BRN_LIST.Where(a => a.BrnCd == account.BaseBrn).FirstOrDefault().IFSCCode;
            List<Branch> branchlist = BranchService.GetAll();
            Branch branch = branchlist.Where(a => a.BrnCd == account.BaseBrn).FirstOrDefault();
            ecsMandate.SpnsrBnkCd = branch.IFSCCode;

            ecsMandate.DebtrName = ecsMandate.DebtrName.ToUpper();

            string tempFilePath = Server.MapPath("~/img/ScanMandate") + "\\" + StartUp.BRN_CD;
            string authFilePath = Server.MapPath("~/img/Authenticated") + "\\" + StartUp.BRN_CD;
            //if (!Directory.Exists(authFilePath))
            //{
            //    Directory.CreateDirectory(authFilePath);
            //}

            UtilityExtensions.IPersonateuser();

            ///Directory.CreateDirectory(authFilePath + @"\" + newImageFile);

            System.IO.File.Move(tempFilePath + "\\" + ImageName.Substring(0, ImageName.IndexOf('.')) + ".jpg", authFilePath + "\\" + newImageFile + "_detailfront.jpg");
            System.IO.File.Move(tempFilePath + "\\" + ImageName.Substring(0, ImageName.IndexOf('.')) + "_Bitonal.tiff", authFilePath + @"\" + newImageFile + "_front.tiff");

            System.IO.File.Delete(authFilePath + "\\" + ImageName);

            mandateService.Update(ecsMandate);
            response.Message = "Mandate Saved Successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }

        public ActionResult FetchNextMandate(string Id)
        {
            Response response = new Response();

            //For show scanned image
            string pathName = ConfigStartUp.MANDATE_SCAN_PATH + "\\" + StartUp.BRN_CD;
            string tempFilePath = Server.MapPath("~/img/ScanMandate") + "\\" + StartUp.BRN_CD;
            string[] files = Directory.GetFiles(pathName, "*.jpg");
            string gryImg, fileName, bitonalImg;

            gryImg = bitonalImg = fileName = string.Empty;

            if (System.IO.File.Exists(tempFilePath + @"/" + Id))
                System.IO.File.Delete(tempFilePath + @"/" + Id);

            string frntId = Id.Replace("detailfront.jpg", "front.tiff");
            if (System.IO.File.Exists(tempFilePath + @"/" + frntId))
                System.IO.File.Delete(tempFilePath + @"/" + frntId);

            string _bitonal = Id.Replace(".jpg", "_bitonal.tiff");
            if (System.IO.File.Exists(tempFilePath + @"/" + _bitonal))
                System.IO.File.Delete(tempFilePath + @"/" + _bitonal);

            //if (files.Count() > 0)
            //{
            //    string greyImgPath = files.First();
            //    gryImg = Path.GetFileName(greyImgPath);
            //    fileName = gryImg.Substring(0, gryImg.IndexOf('.'));
            //    string bitonlImgPath = pathName + "\\" + fileName + "_Bitonal.tiff";

            //    System.IO.File.Move(greyImgPath, tempFilePath + "\\" + gryImg);
            //    System.IO.File.Move(bitonlImgPath, tempFilePath + "\\" + fileName + "_Bitonal.tiff");

            //    gryImg = tempFilePath + "\\" + gryImg;
            //    bitonalImg = tempFilePath + "\\" + fileName + "_Bitonal.tiff";
            //    //ViewBag.ImageName = fileName;
            //    ViewBag.GreyImg = gryImg;
            //    ViewBag.BitonalImg = fileName + "_Bitonal.tiff";
            //    ViewBag.ImageName = fileName;
            //}
            response.Item = new { GreyImg = gryImg, BitonalImg = fileName + "_Bitonal.tiff", ImageName = fileName };

            return Json(response);
        }

        public ActionResult GetMandateById(int Id)
        {
            Response response = new Response();

            string amtInWord = string.Empty;

            var mandate = new MandateService().GetById(Id);
            var dirName = mandate.DetailFrntImg.Substring(0, mandate.DetailFrntImg.IndexOf('_'));

            string tempFilePath = Server.MapPath("~/img/Authenticated") + "\\" + StartUp.BRN_CD;
            if (!Directory.Exists(tempFilePath))
            {
                Directory.CreateDirectory(tempFilePath);
            }

           // var greyImg = @"\img\Authenticated" + "\\" + mandate.DetailFrntImg;
            var greyImg = @"\img\Authenticated" + "\\" + StartUp.BRN_CD + "\\" + mandate.DetailFrntImg;

            var creditAccount = AccountService.GetByNo(mandate.CrAcNo);

            amtInWord = mandate.Amt.NumberToWord();
            response.Item = new
            { Mandate = mandate, ImgSrc = greyImg, AmtInWord = amtInWord, Name = creditAccount.Name,
                DateOfMandate = mandate.DateOfMandate == null ? string.Empty : Convert.ToDateTime(mandate.DateOfMandate).ToString("dd/MM/yyyy"),
            };

            return Json(response);
        }

        public ActionResult Authenticate(int Id)
        {
            Response response = new Response();
            IStartUp startUp = StartupManager.StartUp;
            MandateCommonService mandateCommonService = new MandateCommonService();

            var mandate = new MandateService().GetById(Id);

            if (mandate.Clerk == StartUp.LOGIN_ID)
            {
                response.Message = "Same user cannot Authenticate mandate";
                response.Status = ServiceStatus.ValidationError;
                return Json(response);
            }

            mandate.TrnInd = "A";
            mandate.MandateStatus = "Uploaded";
            mandate.AuthUser = StartUp.LOGIN_ID;

            //Create XML file

            string xmlFileName;
            UtilityExtensions.IPersonateuser();

            xmlFileName = MandateCommonService.CreateXMLName(mandate.MandateFile, "XML", ECS.CREATE);
            string zipFileName = MandateCommonService.CreateXMLName(mandate.MandateFile, "ZIP", ECS.CREATE);

            mandate.MsgId = MandateCommonService.CreateMsgId();
            var creditAccount = AccountService.GetByNo(mandate.CrAcNo);

            mandate.CredtrAgentName = startUp.BNK_NAME;
            //mandate.CrdtrName = creditAccount.Customer.Name;

            var doc = mandateCommonService.CreateXML(mandate);

            string outputPath = ConfigStartUp.INWARD_OUTPUT_PATH;
            //outputPath = "D:\\Pritam\\test";

            string tempFilePath = Server.MapPath("~/img/Authenticated") + "\\" + StartUp.BRN_CD;

            if (!Directory.Exists(tempFilePath))
            {
                Directory.CreateDirectory(tempFilePath);
            }

            //doc.Save(tempFilePath + @"\" + xmlFileName + ".xml",);
            if (!Directory.Exists(tempFilePath + @"\" + xmlFileName))
            {
                Directory.CreateDirectory(tempFilePath + @"\" + xmlFileName);
            }
            using (TextWriter sw = new StreamWriter(tempFilePath + @"\" + xmlFileName + @"\" + xmlFileName + ".xml"))//,false, Encoding.UTF8)) //Set encoding
            {
                doc.Save(sw);
            }

            //using (ZipArchive zipArchive = ZipFile.Open(tempFilePath + @"\" + zipFileName + ".zip", ZipArchiveMode.Create))
            using (ZipArchive zipArchive = ZipFile.Open(outputPath + @"\" + zipFileName + ".zip", ZipArchiveMode.Create))
            {
                FileInfo fileToArchive = new FileInfo(tempFilePath + @"\" + xmlFileName + @"\" + xmlFileName + ".xml");
                zipArchive.CreateEntryFromFile(fileToArchive.FullName, fileToArchive.Name, CompressionLevel.Optimal);
                System.IO.File.Delete(tempFilePath + @"\" + xmlFileName + @"\" + xmlFileName + ".xml");

                FileInfo fileToArchive1 = new FileInfo(tempFilePath + @"\" + xmlFileName.Replace("-INP", "") + "_detailfront.jpg");
                zipArchive.CreateEntryFromFile(fileToArchive1.FullName, fileToArchive1.Name, CompressionLevel.Optimal);
                System.IO.File.Move(tempFilePath + @"\" + xmlFileName.Replace("-INP", "") + "_detailfront.jpg", tempFilePath + @"\" + xmlFileName + @"\" + xmlFileName.Replace("-INP", "") + "_detailfront.jpg");

                FileInfo fileToArchive2 = new FileInfo(tempFilePath + @"\" + xmlFileName.Replace("-INP", "") + "_front.tiff");
                zipArchive.CreateEntryFromFile(fileToArchive2.FullName, fileToArchive2.Name, CompressionLevel.Optimal);
                System.IO.File.Move(tempFilePath + @"\" + xmlFileName.Replace("-INP", "") + "_front.tiff", tempFilePath + @"\" + xmlFileName + @"\" + xmlFileName.Replace("-INP", "") + "_front.tiff");
            }
            //List<ACHInputFile> inphistory = new List<ACHInputFile>();

            //if (mandate.Freq == "MNTH")
            //{
            //    ACHInputFile inp = new ACHInputFile();
            //    inp.ExecDt = null;
            //    inp.MndtId = mandate.Id;
            //    inp.StartDt = Convert.ToDateTime(mandate.FrmPrd);
            //    inp.ExpDt = Convert.ToDateTime(mandate.ToPrd);
            //    inp.DueDt = Convert.ToDateTime(inp.StartDt);
            //    inp.Frq = mandate.Freq;
            //    inp.Stat = "U";
            //    inp.Remark = "";

            //    if (mandate.MandateTyp == "OW")
            //    {
            //        inp.DbCr = "D";
            //    }
            //    else
            //        inp.DbCr = "C";

            //    inphistory.Add(inp);
            //}

            //else if (mandate.Freq == "DAIL")
            //{
            //    ACHInputFile inp = new ACHInputFile();
            //    inp.ExecDt = null;
            //    inp.MndtId = mandate.Id;
            //    inp.StartDt = Convert.ToDateTime(mandate.FrmPrd);
            //    inp.ExpDt = Convert.ToDateTime(mandate.ToPrd);
            //    inp.DueDt = Convert.ToDateTime(inp.StartDt.AddDays(1));
            //    inp.Frq = mandate.Freq;
            //    inp.Stat = "U";
            //    inp.Remark = "";

            //    if (mandate.MandateTyp == "OW")
            //    {
            //        inp.DbCr = "D";
            //    }
            //    else
            //        inp.DbCr = "C";

            //    inphistory.Add(inp);
            //}

            //else if (mandate.Freq == "QURT")
            //{
            //    ACHInputFile inp = new ACHInputFile();
            //    inp.ExecDt = null;
            //    inp.MndtId = mandate.Id;
            //    inp.StartDt = Convert.ToDateTime(mandate.FrmPrd);
            //    inp.ExpDt = Convert.ToDateTime(mandate.ToPrd);
            //    inp.DueDt = Convert.ToDateTime(inp.StartDt.AddMonths(3));
            //    inp.Frq = mandate.Freq;
            //    inp.Stat = "U";
            //    inp.Remark = "";

            //    if (mandate.MandateTyp == "OW")
            //    {
            //        inp.DbCr = "D";
            //    }
            //    else
            //        inp.DbCr = "C";

            //    inphistory.Add(inp);
            //}

            //else if (mandate.Freq == "YEAR")
            //{
            //    ACHInputFile inp = new ACHInputFile();
            //    inp.ExecDt = null;
            //    inp.MndtId = mandate.Id;
            //    inp.StartDt = Convert.ToDateTime(mandate.FrmPrd);
            //    inp.ExpDt = Convert.ToDateTime(mandate.ToPrd);
            //    inp.DueDt = Convert.ToDateTime(inp.StartDt.AddYears(1));
            //    inp.Frq = mandate.Freq;
            //    inp.Stat = "U";
            //    inp.Remark = "";

            //    if (mandate.MandateTyp == "OW")
            //    {
            //        inp.DbCr = "D";
            //    }
            //    else
            //        inp.DbCr = "C";

            //    inphistory.Add(inp);
            //}

            //new MandateService().UpdateInpHistory(inphistory);

            mandate.NPCIFile = zipFileName;
            mandate.ReqDt = DateTime.Now;
            mandate.Status = "Uploaded";
            new MandateService().Update(mandate);
            response.Message = "Mandate Authenticated Successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }

        public ActionResult ValidateCrAccount(string BrnCd, string GlCd, string AcNo)
        {
            Response response = new Response();

 
            if (string.IsNullOrEmpty(AcNo))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please enter Account No";
                return Json(response);
            }
            var account = AccountService.GetByNo(AcNo);

            if (account == null)
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Account not present";
                return Json(response);
            }
            if (account.AcStatus.Name != "Operative")
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please select Operative account";
                return Json(response);
            }
            Branch branch = new BranchService().GetByCode(account.BaseBrn);
            response.Item = new { Name = account.Name, IFSCCode = branch.OldIFSCCode };
            response.Status = ServiceStatus.Success;
            return Json(response);
        }

        public ActionResult ValidateIFSC(string IFSCCode)
        {
            Response response = new Response();
            IFSC ifscMaster = null;
            if (string.IsNullOrEmpty(IFSCCode))
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "Please enter IFSC code";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            ifscMaster = new IFSCService().GetByCode(IFSCCode.ToUpper());

            if (ifscMaster == null)
            {
                response.Status = ServiceStatus.ValidationError;
                response.Message = "IFSC does not exist!. Please contact administrator.";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            response.Item = new { BnkName = ifscMaster.BnkName, BrnName = ifscMaster.BrnName };
            response.Status = ServiceStatus.Success;
            return Json(response);
        }

        public ActionResult Delete(int Id)
        {
            Response response = new Response();
            IStartUp startUp = StartupManager.StartUp;
            MandateCommonService mandateCommonService = new MandateCommonService();

            var mandate = new MandateService().GetById(Id);

            if (mandate.Clerk == StartUp.LOGIN_ID)
            {
                response.Message = "Same user cannot Authenticate mandate";
                response.Status = ServiceStatus.ValidationError;
                return Json(response);
            }

            mandate.TrnInd = "A";
            mandate.MandateStatus = "Uploaded";
            mandate.AuthUser = StartUp.LOGIN_ID;
            mandate.Status = "Cancel";

            UtilityExtensions.IPersonateuser();

           
           
            new MandateService().Update(mandate);
            response.Message = "Mandate Authenticated Successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }

        public ActionResult DeleteUnauthMandate(int Id)
        {
            Response response = new Response();
            MandateService mandateService = new MandateService();
            ECS mandate = new MandateService().GetById(Id);
            string tempFilePath = Server.MapPath("~/img/Authenticated") + "\\" + StartUp.BRN_CD;
            string DetailFrntImg, FrntImg;

            DetailFrntImg = mandate.DetailFrntImg;
            FrntImg = mandate.FrntImg;

            if (System.IO.File.Exists(tempFilePath + @"/" + DetailFrntImg))
                System.IO.File.Delete(tempFilePath + @"/" + DetailFrntImg);

            if (System.IO.File.Exists(tempFilePath + @"/" + FrntImg))
                System.IO.File.Delete(tempFilePath + @"/" + FrntImg);

            mandate.Deleted = "Y";
            mandate.AuthUser = StartUp.LOGIN_ID;
            mandate.DtStamp = DateTime.Now;
            mandateService.Update(mandate);
            response.Message = "Unauth Mandate deleted Successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }


        public ActionResult RejectECSMandate(int Id)
        {
            Response response = new Response();
            MandateService mandateService = new MandateService();
            ECS mandate = new MandateService().GetById(Id);
        
            
            mandate.Deleted = "Y";
            mandate.MandateStatus = "Rejected";
            mandate.AuthUser = StartUp.LOGIN_ID;
            mandate.DtStamp = DateTime.Now;
            mandateService.Update(mandate);
            response.Message = "Unauth Mandate Rejected Successfully";
            response.Status = ServiceStatus.Success;
            return Json(response);
        }


    }
}