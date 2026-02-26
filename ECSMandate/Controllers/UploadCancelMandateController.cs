using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.Framework.EasySecurity;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;

namespace ECSMandate.Controllers
{
    public class UploadCancelMandateController : AppController
    {
        string inwardFilePath = ConfigurationManager.AppSettings.Get("Inward_File_Path");
        string inwardImagePath = ConfigurationManager.AppSettings.Get("Inward_Image_Path");
        string prcessedFilePath = ConfigurationManager.AppSettings.Get("Inward_Prcessed_Path");
        List<string> mmsfilelist = new List<string>();
        MandateService mandateService = new MandateService();
        // GET: UploadData
        public ActionResult Index()
        {
            ViewBag.Path = ConfigStartUp.INWARD_FILE_PATH;
            if (!string.IsNullOrEmpty(ConfigStartUp.INWARD_FILE_PATH))
            {
                UtilityExtensions.IPersonateuser();
                var zipFiles = Directory.GetFiles(ConfigStartUp.INWARD_FILE_PATH, "*zip");
                foreach (string orgZip in zipFiles)
                {
                    var zipName = Path.GetFileName(orgZip);
                    if (!zipName.StartsWith("MMS")) continue;
                    mmsfilelist.Add(zipName);
                }
            }
            ViewBag.mmsfilelist = mmsfilelist;
            return View();
        }

        public async Task<ActionResult> UploadMandate(string FilePath)
        {
            inwardFilePath = ConfigStartUp.INWARD_FILE_PATH;
            inwardImagePath = ConfigStartUp.INWARD_IMAGE_PATH;
            prcessedFilePath = ConfigStartUp.INWARD_PROCESSED_PATH;
            var domain = ConfigurationManager.AppSettings["Domain"];
            var userName = ConfigurationManager.AppSettings["UserName"];
            var pwd = ConfigurationManager.AppSettings["Password"];
            Response response = new Response();
            List<ECS> ecsList = new List<ECS>();
            List<MandateFileHistory> manFileHisList = new List<MandateFileHistory>();
            List<ECSHistory> ecsHistoryList = new List<ECSHistory>();
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
                    var zipName = Path.GetFileName(orgZip);
                    if (!zipName.StartsWith("MMS")) continue;
                    if (!zipName.Contains("CANCEL")) continue;
                    String zipfldpath = "";
                    ecsList = new List<ECS>();
                    manFileHisList = new List<MandateFileHistory>();
                    WriteInText("3." + orgZip);
                    var zip = inwardImagePath + @"\" + zipName;
                    if (zipName.Contains("LVMWI")) continue;
                    if (zipName.Contains("LVESIGN")) continue;
                    if (zipName.Contains("RES")) continue;

                    WriteInText("4.OrgZip:" + orgZip + " Copy:" + inwardImagePath);
                    try
                    {

                        WriteInText("4.1 Start-OrgZip:" + orgZip + " Copy:" + zip);
                        UtilityExtensions.IPersonateuser();

                        System.IO.File.Move(orgZip, zip);
                        WriteInText("4.1 End-OrgZip:" + orgZip + " Copy:" + zip);
                        while (!System.IO.File.Exists(zip))
                        {
                            // Optionally, you can add a delay between each check to reduce CPU usage
                            Thread.Sleep(100); // Uncomment this line if desired
                        }
                        WriteInText("4.1.1 Start-OrgZip:" + DateTime.Now);

                        WriteInText("4.2 OrgZip:" + orgZip + " Copy:" + inwardImagePath);
                        using (ZipArchive archive = ZipFile.OpenRead(zip))
                        {
                            var zipfldName = zipName.Replace(".zip", "");
                            zipfldpath = inwardImagePath + "\\" + zipfldName;
                            if (!Directory.Exists(inwardImagePath + @"\" + zipfldName))
                            {
                                Directory.CreateDirectory(inwardImagePath + @"\" + zipfldName);
                            }
                            int totalFiles = archive.Entries.Count;
                            int extractedFiles = 0;
                            WriteInText("4.2.1 start extract" + DateTime.Now);
                            ZipFile.ExtractToDirectory(zip, zipfldpath);
                            WriteInText("4.2.1 wait start" + DateTime.Now);
                            Thread.Sleep(1000 * totalFiles);
                            WriteInText("4.2.1 wait complte" + DateTime.Now);
                            while (extractedFiles < totalFiles)
                            {
                                extractedFiles = Directory.GetFiles(zipfldpath).Count();
                                Thread.Sleep(1000); // Adjust the duration as per your requirement
                            }
                        }
                        System.IO.File.Move(zip, prcessedFilePath + @"\" + zipName);
                        WriteInText("4.3 OrgZip:" + zip + " Copy:" + prcessedFilePath);
                    }

                    catch (IOException ex)
                    {
                        if (ex.HResult == -2147024816 || ex.HResult == -2147024713)
                        {
                            WriteInText("8.Error" + ex.Message + "Stack:" + ex.StackTrace + "Inner:" + ex.Message);
                            try
                            {
                                System.IO.File.Move(zip, prcessedFilePath + @"\" + zipName);
                            }
                            catch
                            {

                            }
                            //continue;
                        }
                        else
                        {
                            response.Status = ServiceStatus.Failure;
                            response.Message = ex.Message;
                            return Json(new { Response = response }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    catch (Exception ex)
                    {
                        WriteInText("4.Error" + ex.Message + "Stack:" + ex.StackTrace + "Inner:" + ex.Message);
                    }

                    string[] xmlFiles = Directory.GetFiles(zipfldpath, "*.XML");

                    if (xmlFiles.Count() == 0)
                    {
                        continue;
                    }
                    var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "tif", "bmp", "svg" };
                    var imageFiles = GetFilesFrom(zipfldpath, filters, false);

                    string xmlFilePath, xmlFileName;
                    ECS ecsMandate;

                    if (zipName.Contains("CANCEL"))
                    {
                        foreach (string xml in xmlFiles)
                        {
                            xmlFilePath = xml;
                            UtilityExtensions.IPersonateuser();
                            XDocument doc = XDocument.Load(xmlFilePath);
                            XNamespace ns = doc.Root.GetDefaultNamespace();
                            ecsMandate = new ECS();
                            var Header = doc.Descendants(ns + "GrpHdr").ToList();
                            ecsMandate.MsgId = (string)Header.Descendants(ns + "MsgId").FirstOrDefault();
                            DateTime creDtTm = (DateTime)Header.Descendants(ns + "CreDtTm").FirstOrDefault();
                            ecsMandate.UMRDt = creDtTm.Date;
                            var instgPrty = Header.Descendants(ns + "InitgPty").ToList();
                            if (instgPrty != null)
                            {
                                var OrgIdList = instgPrty.Descendants(ns + "OrgId").ToList();
                                ecsMandate.UserNACHId = (string)OrgIdList.Descendants(ns + "Id").FirstOrDefault();
                            }
                            var instrAgnt = Header.Descendants(ns + "InstgAgt").ToList();
                            ecsMandate.SpnsrBankName = (string)instrAgnt.Descendants(ns + "Nm").FirstOrDefault();
                            ecsMandate.SpnsrBnkCd = (string)instrAgnt.Descendants(ns + "MmbId").FirstOrDefault();
                            var WithBank = Header.Descendants(ns + "InstdAgt").ToList();
                            ecsMandate.BankName = (string)WithBank.Descendants(ns + "Nm").FirstOrDefault();
                            string bankCd = (string)WithBank.Descendants(ns + "MmbId").FirstOrDefault();
                            ecsMandate.BnkCd = bankCd;
                            var Data = doc.Descendants(ns + "OrgnlMndt").ToList();
                            ecsMandate.UMRNo = (string)Data.Descendants(ns + "OrgnlMndtId").FirstOrDefault();
                            // ecsMandate.MndtReqId = (string)Data.Descendants(ns + "MndtReqId").FirstOrDefault();

                            var existMandate = new MandateService().GetByUmrn(ecsMandate.UMRNo);
                            if (existMandate == null)
                            {
                                System.IO.File.Delete(xml);
                                continue;
                            }
                            existMandate.AuthUser = StartUp.LOGIN_ID;
                            existMandate.DtStamp = StartUp.DAY_OP_DT; //DateTime.Now.Date;
                            existMandate.Status = "Cancel";
                            // existMandate.Variant = "CANCEL";
                            var ecsHistory = CommonService.Cast<ECSHistory>(existMandate);
                            ecsList.Add(existMandate);
                            ecsHistoryList.Add(ecsHistory);
                            System.IO.File.Delete(xml);
                        }
                    }
                    new MandateService().BulkUpdate(ecsList, null, ecsHistoryList);
                }
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

            return Json(new { Response = response, partialView = MvcHelpers.RenderPartialView(this, "_ECSCancelMandateList", ecsList, null) }, JsonRequestBehavior.AllowGet);
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



    }
}