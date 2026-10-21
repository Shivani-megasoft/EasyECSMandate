using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.Framework.EasyLogging;
using EasyBank.Framework.EasySecurity;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Xml.Linq;

namespace ECSMandate.Controllers
{
	public class UploadDataController : AppController
	{
		string inwardFilePath = ConfigurationManager.AppSettings.Get("Inward_File_Path");
		string inwardImagePath = ConfigurationManager.AppSettings.Get("Inward_Image_Path");
		string prcessedFilePath = ConfigurationManager.AppSettings.Get("Inward_Prcessed_Path");

		MandateService mandateService = new MandateService();
		// GET: UploadData
		public ActionResult Index()
		{
			ViewBag.Path = ConfigStartUp.INWARD_FILE_PATH;
			//ViewBag.Path = @"..\H2H_UPCB\DataSource\BankCode\Verified\";
			return View();
		}

		//public ActionResult UploadMandate(string FilePath)
		//{

		//	EasyLogger.Log("testlog", "General", 1, TraceEventType.Error);
		//	inwardFilePath = ConfigStartUp.INWARD_FILE_PATH;
		//	inwardImagePath = ConfigStartUp.INWARD_IMAGE_PATH;
		//	prcessedFilePath = ConfigStartUp.INWARD_PROCESSED_PATH;
		//	Response response = new Response();
		//	List<ECS> ecsList = new List<ECS>();
		//	//List<MandateFileHistory> manFileHisList = new List<MandateFileHistory>();

		//	string variant;

		//	if (FilePath == string.Empty)
		//	{
		//		response.Message = "File Path Cannot be empty";
		//		response.Status = ServiceStatus.ValidationError;
		//		return Json(response);
		//	}

		//	try
		//	{
		//		UtilityExtensions.IPersonateuser();
		//		var zipFiles = Directory.GetFiles(inwardFilePath, "*zip");

		//		foreach (string orgZip in zipFiles)
		//		{
		//			WriteInText("3." + orgZip);

		//			variant = "Normal";
		//			String zipfldpath = "";
		//			var zipName = Path.GetFileName(orgZip);

		//			var zip = inwardImagePath + @"\" + zipName;
		//			if (zipName.Contains("LVMWI"))
		//			{
		//				variant = "MWI";
		//			}
		//			else if (zipName.Contains("LVESIGN")) continue;
		//			//{
		//			//    variant = "ESIGN";
		//			//}
		//			if (zipName.Contains("CANCEL")) continue;
		//			if (zipName.Contains("RES")) continue;

		//			//commented 31012026
		//			//UtilityExtensions.IPersonateuser();
		//			//WriteInText("4.OrgZip:" + orgZip + " Copy:" + inwardImagePath);
		//			//try
		//			//{
		//			//    System.IO.File.Move(orgZip, zip);
		//			//    ZipFile.ExtractToDirectory(zip, inwardImagePath);
		//			//    System.IO.File.Move(zip, prcessedFilePath + @"\" + zipName);
		//			//}
		//			//catch (Exception ex)
		//			//{
		//			//    WriteInText("5.Error" + ex.Message + "Stack:" + ex.StackTrace + "Inner:" + ex.InnerException);

		//			//    response.Status = ServiceStatus.Failure;
		//			//    response.Message = ex.Message;
		//			//    return Json(new { Response = response }, JsonRequestBehavior.AllowGet);
		//			//}
		//			//string[] xmlFiles = Directory.GetFiles(inwardImagePath, "*.XML");

		//			//if (xmlFiles.Count() == 0)
		//			//{
		//			//    response.Message = "Mandate File Not Found!";
		//			//    response.Status = ServiceStatus.ValidationError;
		//			//    return Json(response);
		//			//}

		//			WriteInText("4.OrgZip:" + orgZip + " Copy:" + inwardImagePath);
		//			try
		//			{
		//				WriteInText("4.1 Start-OrgZip:" + orgZip + " Copy:" + zip);
		//				UtilityExtensions.IPersonateuser();

		//				System.IO.File.Move(orgZip, zip);
		//				WriteInText("4.1 End-OrgZip:" + orgZip + " Copy:" + zip);
		//				while (!System.IO.File.Exists(zip))
		//				{
		//					// Optionally, you can add a delay between each check to reduce CPU usage
		//					Thread.Sleep(100); 
		//				}
		//				WriteInText("4.1.1 Start-OrgZip:" + DateTime.Now);

		//				WriteInText("4.2 OrgZip:" + orgZip + " Copy:" + inwardImagePath);
		//				using (ZipArchive archive = ZipFile.OpenRead(zip))
		//				{
		//					var zipfldName = zipName.Replace(".zip", "");
		//					zipfldpath = inwardImagePath + "\\" + zipfldName;
		//					if (!Directory.Exists(inwardImagePath + @"\" + zipfldName))
		//					{
		//						Directory.CreateDirectory(inwardImagePath + @"\" + zipfldName);
		//					}
		//					int totalFiles = archive.Entries.Count;
		//					int extractedFiles = 0;
		//					WriteInText("4.2.1 start extract" + DateTime.Now);
		//					ZipFile.ExtractToDirectory(zip, zipfldpath);
		//					WriteInText("4.2.1 wait start" + DateTime.Now);
		//					Thread.Sleep(1000 * totalFiles);
		//					WriteInText("4.2.1 wait complte" + DateTime.Now);
		//					while (extractedFiles < totalFiles)
		//					{
		//						extractedFiles = Directory.GetFiles(zipfldpath).Count();
		//						Thread.Sleep(1000); // Adjust the duration as per your requirement
		//					}
		//				}

		//				System.IO.File.Move(zip, prcessedFilePath + @"\" + zipName);
		//				WriteInText("4.3 OrgZip:" + zip + " Copy:" + prcessedFilePath);
		//			}

		//			catch (IOException ex)
		//			{
		//				if (ex.HResult == -2147024816 || ex.HResult == -2147024713)
		//				{
		//					WriteInText("8.Error" + ex.Message + "Stack:" + ex.StackTrace + "Inner:" + ex.Message);
		//					try
		//					{
		//						System.IO.File.Move(zip, prcessedFilePath + @"\" + zipName);
		//					}
		//					catch
		//					{

		//					}
		//				}
		//				else
		//				{
		//					response.Status = ServiceStatus.Failure;
		//					response.Message = ex.Message;
		//					return Json(new { Response = response }, JsonRequestBehavior.AllowGet);
		//				}

		//			}
		//			catch (Exception ex)
		//			{
		//				WriteInText("4.Error" + ex.Message + "Stack:" + ex.StackTrace + "Inner:" + ex.Message);
		//			}

		//			string[] xmlFiles = Directory.GetFiles(zipfldpath, "*.XML");

		//			if (xmlFiles.Count() == 0)
		//			{
		//				continue;
		//			}


		//			var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "tif", "bmp", "svg" };
		//			var imageFiles = GetFilesFrom(inwardImagePath, filters, false);

		//			string xmlFilePath, xmlFileName;
		//			ECS ecsMandate;

		//			foreach (string xml in xmlFiles)
		//			{
		//				xmlFilePath = xml;
		//				xmlFileName = Path.GetFileNameWithoutExtension(xmlFilePath);

		//				var existMandate = new MandateService().GetECSByFileName(xmlFileName);
		//				if (existMandate != null)
		//				{

		//				}

		//				XDocument doc = XDocument.Load(xmlFilePath);
		//				XNamespace ns = doc.Root.GetDefaultNamespace();
		//				//header Part
		//				ecsMandate = new ECS();
		//				//ecsMandate.EcsID = Guid.NewGuid();

		//				var Header = doc.Descendants(ns + "GrpHdr").ToList();
		//				//New added on 27/11
		//				ecsMandate.MsgId = (string)Header.Descendants(ns + "MsgId").FirstOrDefault();
		//				DateTime creDtTm = (DateTime)Header.Descendants(ns + "CreDtTm").FirstOrDefault();
		//				ecsMandate.UMRDt = creDtTm.Date;
		//				ecsMandate.MandateFile = xmlFileName;

		//				//Initiating party
		//				var instgPrty = Header.Descendants(ns + "InitgPty").ToList();
		//				if (instgPrty != null)
		//				{
		//					var OrgIdList = instgPrty.Descendants(ns + "OrgId").ToList();
		//					ecsMandate.UserNACHId = (string)OrgIdList.Descendants(ns + "Id").FirstOrDefault();
		//				}
		//				//Instructing Agent
		//				var instrAgnt = Header.Descendants(ns + "InstgAgt").ToList();
		//				ecsMandate.SpnsrBankName = (string)instrAgnt.Descendants(ns + "Nm").FirstOrDefault();
		//				ecsMandate.SpnsrBnkCd = (string)instrAgnt.Descendants(ns + "MmbId").FirstOrDefault();

		//				//Instructed Agent
		//				var WithBank = Header.Descendants(ns + "InstdAgt").ToList();
		//				ecsMandate.BankName = (string)WithBank.Descendants(ns + "Nm").FirstOrDefault();
		//				string bankCd = (string)WithBank.Descendants(ns + "MmbId").FirstOrDefault();
		//				ecsMandate.BnkCd = bankCd;

		//				//Data Part
		//				var Data = doc.Descendants(ns + "Mndt").ToList();
		//				ecsMandate.UMRNo = (string)Data.Descendants(ns + "MndtId").FirstOrDefault();
		//				ecsMandate.MndtReqId = (string)Data.Descendants(ns + "MndtReqId").FirstOrDefault();

		//				//Type
		//				var typList = Data.Descendants(ns + "Tp").ToList();
		//				var catList = typList.Descendants(ns + "SvcLvl").ToList();
		//				ecsMandate.Category = (string)catList.Descendants(ns + "Prtry").FirstOrDefault();

		//				var proList = typList.Descendants(ns + "LclInstrm").ToList();
		//				ecsMandate.Proprietary = (string)proList.Descendants(ns + "Prtry").FirstOrDefault();

		//				if (Data.Descendants(ns + "ColltnAmt").FirstOrDefault() != null)
		//					ecsMandate.Amt = (Decimal)Data.Descendants(ns + "ColltnAmt").FirstOrDefault();
		//				else if (Data.Descendants(ns + "MaxAmt").FirstOrDefault() != null)
		//					ecsMandate.Amt = (decimal)Data.Descendants(ns + "MaxAmt").FirstOrDefault();

		//				var Ocrncs = Data.Descendants(ns + "Ocrncs").ToList();

		//				var Drtn = Ocrncs.Descendants(ns + "Drtn").FirstOrDefault();
		//				DateTime? FrDt = null;
		//				if (Drtn != null)
		//				{
		//					var FrDtElement = Drtn.Descendants(ns + "FrDt").FirstOrDefault();
		//					if (FrDtElement != null)
		//					{
		//						FrDt = (DateTime)FrDtElement;
		//					}
		//				}
		//				if (FrDt != null)
		//				{
		//					ecsMandate.DateOfMandate = (DateTime)Drtn.Descendants(ns + "FrDt").FirstOrDefault();
		//				}

		//				ecsMandate.Freq = (string)Ocrncs.Descendants(ns + "Frqcy").FirstOrDefault();
		//				ecsMandate.Seq = (string)Ocrncs.Descendants(ns + "SeqTp").FirstOrDefault();

		//				var FrstColltnDtchk = (string)Ocrncs.Descendants(ns + "FrstColltnDt").FirstOrDefault();
		//				var FnlColltnDtchk = (string)Ocrncs.Descendants(ns + "FnlColltnDt").FirstOrDefault();
		//				if (FrstColltnDtchk != null)
		//				{
		//					ecsMandate.FrmPrd = (DateTime)Ocrncs.Descendants(ns + "FrstColltnDt").FirstOrDefault();
		//				}
		//				if (FnlColltnDtchk != null)
		//				{
		//					ecsMandate.ToPrd = (DateTime)Ocrncs.Descendants(ns + "FnlColltnDt").FirstOrDefault();
		//				}
		//				//Service Provider name
		//				var serviceProvider = Data.Descendants(ns + "Cdtr").ToList();
		//				ecsMandate.UtilityName = (string)serviceProvider.Descendants(ns + "Nm").FirstOrDefault();

		//				//Creditor Account
		//				var Utilitylist = Data.Descendants(ns + "CdtrAcct").ToList();
		//				//below Id is desendent of Othr tag ....need to check
		//				ecsMandate.UtilityCd = (string)Utilitylist.Descendants(ns + "Id").FirstOrDefault();

		//				//Financial institution servicing an account for the creditor.
		//				var SponserCodelist = Data.Descendants(ns + "CdtrAgt").ToList();
		//				//ecsMandate.SpnsrBnkCd = (string)SponserCodelist.Descendants(ns + "MmbId").FirstOrDefault();
		//				ecsMandate.CredtrAgentCode = (string)SponserCodelist.Descendants(ns + "MmbId").FirstOrDefault();
		//				ecsMandate.CredtrAgentName = (string)SponserCodelist.Descendants(ns + "Nm").FirstOrDefault();

		//				//Party that signs the mandate and owes an amount of money to the (ultimate) creditor
		//				var Dbtr = Data.Descendants(ns + "Dbtr").ToList();
		//				ecsMandate.CrdtrName = (string)Dbtr.Descendants(ns + "Nm").FirstOrDefault();

		//				var Refno = Dbtr.Descendants(ns + "PrvtId").ToList();
		//				ecsMandate.RefNo1 = (string)Refno.Descendants(ns + "Id").FirstOrDefault();
		//				ecsMandate.RefNo2 = (string)Refno.Descendants(ns + "Prtry").FirstOrDefault();//New

		//				//Contact deatils
		//				var CtctDtls = Dbtr.Descendants(ns + "CtctDtls").ToList();
		//				ecsMandate.PhoneNo = (string)CtctDtls.Descendants(ns + "PhneNb").FirstOrDefault();
		//				ecsMandate.Mobile = (string)CtctDtls.Descendants(ns + "MobNb").FirstOrDefault();
		//				ecsMandate.EMail = (string)CtctDtls.Descendants(ns + "EmailAdr").FirstOrDefault();
		//				ecsMandate.OthrDetail = (string)CtctDtls.Descendants(ns + "Othr").FirstOrDefault();

		//				//Debtor Account
		//				var DbtrAcct = Data.Descendants(ns + "DbtrAcct").ToList();
		//				var Othr = DbtrAcct.Descendants(ns + "Othr").ToList();
		//				ecsMandate.AcNo = (string)Othr.Descendants(ns + "Id").FirstOrDefault();
		//				var TpSaving = DbtrAcct.Descendants(ns + "Tp").ToList();
		//				ecsMandate.AcTyp = (string)TpSaving.Descendants(ns + "Prtry").FirstOrDefault();

		//				//Financial institution servicing an account for the debtor
		//				var dbtrAgnt = Data.Descendants(ns + "DbtrAgt").ToList();
		//				var memId = dbtrAgnt.Descendants(ns + "ClrSysMmbId").ToList();

		//				ecsMandate.DebtrCode = (string)memId.Descendants(ns + "MmbId").FirstOrDefault();
		//				ecsMandate.DebtrBnk = (string)dbtrAgnt.Descendants(ns + "Nm").FirstOrDefault();//need to check

		//				//Move images in another folder
		//				xmlFileName = xmlFileName.Replace("-INP", "");

		//				if (!Directory.Exists(inwardImagePath))
		//				{
		//					Directory.CreateDirectory(inwardImagePath);
		//				}
		//				var mandateImages = Array.FindAll(imageFiles, s => s.Contains(xmlFileName));
		//				if (mandateImages.Count() > 0)
		//				{
		//					var detailImages = Array.FindAll(mandateImages, s => s.Contains("_detailfront"));
		//					if (detailImages.Count() > 0)
		//					{
		//						ecsMandate.DetailFrntImg = Path.GetFileName(detailImages[0]);
		//						//System.IO.File.Copy(detailImages[0], imagePath + "\\" + ecsMandate.DetailFrntImg, true);
		//					}

		//					var frntImages = Array.FindAll(mandateImages, s => s.Contains("_front"));
		//					if (frntImages.Count() > 0)
		//					{
		//						ecsMandate.FrntImg = Path.GetFileName(frntImages[0]);
		//						//System.IO.File.Copy(frntImages[0], imagePath + "\\" + ecsMandate.FrntImg, true);
		//					}
		//				}

		//				ecsMandate.TrnInd = "S"; //"S";//need to chk new change for double upload //""
		//				ecsMandate.Deleted = "N";
		//				ecsMandate.Clerk = StartUp.LOGIN_ID;
		//				ecsMandate.DtStamp = StartUp.DAY_OP_DT; //DateTime.Now.Date;
		//				ecsMandate.MandateStatus = "Uploaded";
		//				ecsMandate.MandateTyp = "IW";
		//				ecsMandate.NPCIFile = zipName;
		//				ecsMandate.Variant = variant;

		//				GlobalIdGenerator globalIdGenerator = ecsMandate.GlobalIdGenerator;
		//				var ecsId = Convert.ToInt32(globalIdGenerator.LastNo);
		//				ecsMandate.Id = ecsId;

		//				ecsList.Add(ecsMandate);

		//				System.IO.File.Delete(xml);
		//			}
		//		}
		//		new MandateService().BulkUpdate(ecsList, null);

		//		Session["MANDATE_ECS_LIST"] = ecsList;

		//		response.Status = ServiceStatus.Success;

		//	}
		//	catch (Exception ex)
		//	{
		//		EasyLogger.Log(ex.Message, "General", 1, TraceEventType.Error);
		//		WriteInText(ex.Message + ex.StackTrace);
		//		response.Status = ServiceStatus.Failure;
		//		response.Message = ex.Message;
		//		return Json(new { Response = response }, JsonRequestBehavior.AllowGet);

		//	}

		//	return Json(new { Response = response, partialView = MvcHelpers.RenderPartialView(this, "_ECSMandateList", ecsList, null) }, JsonRequestBehavior.AllowGet);
		//}



		public ActionResult UploadMandate(string FilePath)
		{
			inwardFilePath = ConfigStartUp.INWARD_FILE_PATH;
			inwardImagePath = ConfigStartUp.INWARD_IMAGE_PATH;
			prcessedFilePath = ConfigStartUp.INWARD_PROCESSED_PATH;
			Response response = new Response();
			List<ECS> ecsList = new List<ECS>();
			//List<MandateFileHistory> manFileHisList = new List<MandateFileHistory>();

			string variant;

			if (FilePath == string.Empty)
			{
				response.Message = "File Path Cannot be empty";
				response.Status = ServiceStatus.ValidationError;
				return Json(response);
			}

			try
			{
				UtilityExtensions.IPersonateuser();
				var zipFiles = Directory.GetFiles(inwardFilePath, "*zip");

				foreach (string orgZip in zipFiles)
				{
					WriteInText("3." + orgZip);

					variant = "Normal";

					var zipName = Path.GetFileName(orgZip);

					var zip = inwardImagePath + @"\" + zipName;
					if (zipName.Contains("LVMWI"))
					{
						variant = "MWI";
					}
					else if (zipName.Contains("LVESIGN")) continue;
					//{
					//    variant = "ESIGN";
					//}
					if (zipName.Contains("CANCEL")) continue;
					if (zipName.Contains("RES")) continue;

					UtilityExtensions.IPersonateuser();

					WriteInText("4.OrgZip:" + orgZip + " Copy:" + inwardImagePath);
					try
					{
						System.IO.File.Move(orgZip, zip);
						ZipFile.ExtractToDirectory(zip, inwardImagePath);
						System.IO.File.Move(zip, prcessedFilePath + @"\" + zipName);
					}
					catch (Exception ex)
					{
						WriteInText("5.Error" + ex.Message + "Stack:" + ex.StackTrace + "Inner:" + ex.InnerException);

						response.Status = ServiceStatus.Failure;
						response.Message = ex.Message;
						return Json(new { Response = response }, JsonRequestBehavior.AllowGet);
					}
					string[] xmlFiles = Directory.GetFiles(inwardImagePath, "*.XML");

					if (xmlFiles.Count() == 0)
					{
						response.Message = "Mandate File Not Found!";
						response.Status = ServiceStatus.ValidationError;
						return Json(response);
					}
					var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "tif", "bmp", "svg" };
					var imageFiles = GetFilesFrom(inwardImagePath, filters, false);

					string xmlFilePath, xmlFileName;
					ECS ecsMandate;
					int ecsIdCounter = 0;

					foreach (string xml in xmlFiles)
					{
						xmlFilePath = xml;
						xmlFileName = Path.GetFileNameWithoutExtension(xmlFilePath);

						var existMandate = new MandateService().GetECSByFileName(xmlFileName);
						if (existMandate != null)
						{

						}

						XDocument doc = XDocument.Load(xmlFilePath);
						XNamespace ns = doc.Root.GetDefaultNamespace();
						//header Part
						ecsMandate = new ECS();
						//ecsMandate.EcsID = Guid.NewGuid();

						var Header = doc.Descendants(ns + "GrpHdr").ToList();
						//New added on 27/11
						ecsMandate.MsgId = (string)Header.Descendants(ns + "MsgId").FirstOrDefault();
						DateTime creDtTm = (DateTime)Header.Descendants(ns + "CreDtTm").FirstOrDefault();
						ecsMandate.UMRDt = creDtTm.Date;
						ecsMandate.MandateFile = xmlFileName;

						//Initiating party
						var instgPrty = Header.Descendants(ns + "InitgPty").ToList();
						if (instgPrty != null)
						{
							var OrgIdList = instgPrty.Descendants(ns + "OrgId").ToList();
							ecsMandate.UserNACHId = (string)OrgIdList.Descendants(ns + "Id").FirstOrDefault();
						}
						//Instructing Agent
						var instrAgnt = Header.Descendants(ns + "InstgAgt").ToList();
						ecsMandate.SpnsrBankName = (string)instrAgnt.Descendants(ns + "Nm").FirstOrDefault();
						ecsMandate.SpnsrBnkCd = (string)instrAgnt.Descendants(ns + "MmbId").FirstOrDefault();

						//Instructed Agent
						var WithBank = Header.Descendants(ns + "InstdAgt").ToList();
						ecsMandate.BankName = (string)WithBank.Descendants(ns + "Nm").FirstOrDefault();
						string bankCd = (string)WithBank.Descendants(ns + "MmbId").FirstOrDefault();
						ecsMandate.BnkCd = bankCd;

						//Data Part
						var Data = doc.Descendants(ns + "Mndt").ToList();
						ecsMandate.UMRNo = (string)Data.Descendants(ns + "MndtId").FirstOrDefault();
						ecsMandate.MndtReqId = (string)Data.Descendants(ns + "MndtReqId").FirstOrDefault();

						//Type
						var typList = Data.Descendants(ns + "Tp").ToList();
						var catList = typList.Descendants(ns + "SvcLvl").ToList();
						ecsMandate.Category = (string)catList.Descendants(ns + "Prtry").FirstOrDefault();

						var proList = typList.Descendants(ns + "LclInstrm").ToList();
						ecsMandate.Proprietary = (string)proList.Descendants(ns + "Prtry").FirstOrDefault();

						if (Data.Descendants(ns + "ColltnAmt").FirstOrDefault() != null)
							ecsMandate.Amt = (Decimal)Data.Descendants(ns + "ColltnAmt").FirstOrDefault();
						else if (Data.Descendants(ns + "MaxAmt").FirstOrDefault() != null)
							ecsMandate.Amt = (decimal)Data.Descendants(ns + "MaxAmt").FirstOrDefault();

						var Ocrncs = Data.Descendants(ns + "Ocrncs").ToList();

						var Drtn = Ocrncs.Descendants(ns + "Drtn").FirstOrDefault();
						DateTime? FrDt = null;
						if (Drtn != null)
						{
							var FrDtElement = Drtn.Descendants(ns + "FrDt").FirstOrDefault();
							if (FrDtElement != null)
							{
								FrDt = (DateTime)FrDtElement;
							}
						}
						if (FrDt != null)
						{
							ecsMandate.DateOfMandate = (DateTime)Drtn.Descendants(ns + "FrDt").FirstOrDefault();
						}

						ecsMandate.Freq = (string)Ocrncs.Descendants(ns + "Frqcy").FirstOrDefault();
						ecsMandate.Seq = (string)Ocrncs.Descendants(ns + "SeqTp").FirstOrDefault();

						var FrstColltnDtchk = (string)Ocrncs.Descendants(ns + "FrstColltnDt").FirstOrDefault();
						var FnlColltnDtchk = (string)Ocrncs.Descendants(ns + "FnlColltnDt").FirstOrDefault();
						if (FrstColltnDtchk != null)
						{
							ecsMandate.FrmPrd = (DateTime)Ocrncs.Descendants(ns + "FrstColltnDt").FirstOrDefault();
						}
						if (FnlColltnDtchk != null)
						{
							ecsMandate.ToPrd = (DateTime)Ocrncs.Descendants(ns + "FnlColltnDt").FirstOrDefault();
						}
						//Service Provider name
						var serviceProvider = Data.Descendants(ns + "Cdtr").ToList();
						ecsMandate.UtilityName = (string)serviceProvider.Descendants(ns + "Nm").FirstOrDefault();

						//Creditor Account
						var Utilitylist = Data.Descendants(ns + "CdtrAcct").ToList();
						//below Id is desendent of Othr tag ....need to check
						ecsMandate.UtilityCd = (string)Utilitylist.Descendants(ns + "Id").FirstOrDefault();

						//Financial institution servicing an account for the creditor.
						var SponserCodelist = Data.Descendants(ns + "CdtrAgt").ToList();
						//ecsMandate.SpnsrBnkCd = (string)SponserCodelist.Descendants(ns + "MmbId").FirstOrDefault();
						ecsMandate.CredtrAgentCode = (string)SponserCodelist.Descendants(ns + "MmbId").FirstOrDefault();
						ecsMandate.CredtrAgentName = (string)SponserCodelist.Descendants(ns + "Nm").FirstOrDefault();

						//Party that signs the mandate and owes an amount of money to the (ultimate) creditor
						var Dbtr = Data.Descendants(ns + "Dbtr").ToList();
						ecsMandate.CrdtrName = (string)Dbtr.Descendants(ns + "Nm").FirstOrDefault();

						var Refno = Dbtr.Descendants(ns + "PrvtId").ToList();
						ecsMandate.RefNo1 = (string)Refno.Descendants(ns + "Id").FirstOrDefault();
						ecsMandate.RefNo2 = (string)Refno.Descendants(ns + "Prtry").FirstOrDefault();//New

						//Contact deatils
						var CtctDtls = Dbtr.Descendants(ns + "CtctDtls").ToList();
						ecsMandate.PhoneNo = (string)CtctDtls.Descendants(ns + "PhneNb").FirstOrDefault();
						ecsMandate.Mobile = (string)CtctDtls.Descendants(ns + "MobNb").FirstOrDefault();
						ecsMandate.EMail = (string)CtctDtls.Descendants(ns + "EmailAdr").FirstOrDefault();
						ecsMandate.OthrDetail = (string)CtctDtls.Descendants(ns + "Othr").FirstOrDefault();

						//Debtor Account
						var DbtrAcct = Data.Descendants(ns + "DbtrAcct").ToList();
						var Othr = DbtrAcct.Descendants(ns + "Othr").ToList();
						ecsMandate.AcNo = (string)Othr.Descendants(ns + "Id").FirstOrDefault();
						var TpSaving = DbtrAcct.Descendants(ns + "Tp").ToList();
						ecsMandate.AcTyp = (string)TpSaving.Descendants(ns + "Prtry").FirstOrDefault();

						//Financial institution servicing an account for the debtor
						var dbtrAgnt = Data.Descendants(ns + "DbtrAgt").ToList();
						var memId = dbtrAgnt.Descendants(ns + "ClrSysMmbId").ToList();

						ecsMandate.DebtrCode = (string)memId.Descendants(ns + "MmbId").FirstOrDefault();
						ecsMandate.DebtrBnk = (string)dbtrAgnt.Descendants(ns + "Nm").FirstOrDefault();//need to check

						//Move images in another folder
						xmlFileName = xmlFileName.Replace("-INP", "");

						if (!Directory.Exists(inwardImagePath))
						{
							Directory.CreateDirectory(inwardImagePath);
						}
						var mandateImages = Array.FindAll(imageFiles, s => s.Contains(xmlFileName));
						if (mandateImages.Count() > 0)
						{
							var detailImages = Array.FindAll(mandateImages, s => s.Contains("_detailfront"));
							if (detailImages.Count() > 0)
							{
								ecsMandate.DetailFrntImg = Path.GetFileName(detailImages[0]);
								//System.IO.File.Copy(detailImages[0], imagePath + "\\" + ecsMandate.DetailFrntImg, true);
							}

							var frntImages = Array.FindAll(mandateImages, s => s.Contains("_front"));
							if (frntImages.Count() > 0)
							{
								ecsMandate.FrntImg = Path.GetFileName(frntImages[0]);
								//System.IO.File.Copy(frntImages[0], imagePath + "\\" + ecsMandate.FrntImg, true);
							}
						}

						ecsMandate.TrnInd = "S"; //"S";//need to chk new change for double upload //""
						ecsMandate.Deleted = "N";
						ecsMandate.Clerk = StartUp.LOGIN_ID;
						ecsMandate.DtStamp = StartUp.DAY_OP_DT; //DateTime.Now.Date;
						ecsMandate.MandateStatus = "Uploaded";
						ecsMandate.MandateTyp = "IW";
						ecsMandate.NPCIFile = zipName;
						ecsMandate.Variant = variant;

						//GlobalIdGenerator globalIdGenerator = ecsMandate.GlobalIdGenerator;
						//var ecsId = Convert.ToInt32(globalIdGenerator.LastNo);
						//ecsMandate.Id = ecsId;

						GlobalIdGenerator globalIdGenerator = ecsMandate.GlobalIdGenerator;
						var ecsId = Convert.ToInt32(globalIdGenerator.LastNo);

						ecsMandate.Id = ecsId + ecsIdCounter;
						ecsIdCounter++;

						ecsList.Add(ecsMandate);

						System.IO.File.Delete(xml);
					}
				}
				new MandateService().BulkUpdate(ecsList, null);

				Session["MANDATE_ECS_LIST"] = ecsList;

				response.Status = ServiceStatus.Success;

			}
			catch (Exception ex)
			{
				WriteInText(ex.Message + ex.StackTrace);
				response.Status = ServiceStatus.Failure;
				response.Message = ex.Message;
				return Json(new { Response = response }, JsonRequestBehavior.AllowGet);
			}

			return Json(new { Response = response, partialView = MvcHelpers.RenderPartialView(this, "_ECSMandateList", ecsList, null) }, JsonRequestBehavior.AllowGet);
		}

		public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
		{
			List<String> filesFound = new List<String>();
			var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			foreach (var filter in filters)
			{
				filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
			}
			return filesFound.ToArray();
		}

		public void WriteInText(string text)
		{
			var path = Server.MapPath("~/img/test.txt");

			using (StreamWriter writetext = System.IO.File.AppendText(path))
			{
				writetext.WriteLine(text);
			}
		}

		public ActionResult DBUploadList(string[] MandateList)
		{
			Response response = new Response();

			List<string> updatedList = new List<string>();

			string mList = MandateList[0].Remove(0, 2);

			//var newArray = mList.Split(',');
			var newArray = mList.Split(new string[] { "],[" }, StringSplitOptions.None);

			var distinctArray = newArray.Distinct();
			foreach (string arr in distinctArray)
			{
				if (arr == "null") continue;
				var rowDetail = arr.Replace("\"", "").Replace("]]", "");

				//var ecsList= mandateService.GetAllMandateByNPCI(rowDetail);
				mandateService.UpdateMandateStatusByNPCIFile(rowDetail, "S");
			}
			response.Status = ServiceStatus.Success;
			response.Message = "Upload data successfully!";
			return Json(response);
		}
	}
}