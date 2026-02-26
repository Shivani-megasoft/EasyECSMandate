using EasyBank.Infrastructure.SystemParameters;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MandateAckMonitoring
{
    public class MandateUsynchService
    {
        //new pritam 
        public void Queue(Action action, Action done)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    action();
                }
                catch (ThreadAbortException) { }
                catch (Exception ex) { } // note: this will not be called if the thread is aborted        

                if (done != null) done();
            });
            Thread.Sleep(1000);
        }

        public void USynchAcknowledge(string zipFilePath) //add 3 parameter
        {
            IStartUp StartUp = StartupManager.StartUp;
            MandateService mandateService = new MandateService();
            UtilityExtensions.IPersonateuser();

            try
            {
                string copyAckPath = ConfigurationManager.AppSettings.Get("ACK_COPY_PATH");
                UtilityExtensions.IPersonateuser();

                if (!Directory.Exists(copyAckPath))
                {
                    Directory.CreateDirectory(copyAckPath);
                }

                var zipName = Path.GetFileName(zipFilePath);
                var copyZipPath = copyAckPath + @"\" + zipName;

                CreateLog(zipName + " Zip created");

                var zipArry = zipName.Split('-');
                System.IO.File.Move(zipFilePath, copyZipPath);

                copyAckPath = copyAckPath + @"\" + zipName.Replace(".zip", "");
                ZipFile.ExtractToDirectory(copyZipPath, copyAckPath);

                // var ackFile = copyAckPath + @"\" + zipName;

                string[] xmlFiles = Directory.GetFiles(copyAckPath, "*.xml");

                if (xmlFiles.Count() == 0)
                {
                    var ackFile = copyAckPath + @"\" + zipName;
                    xmlFiles = Directory.GetFiles(copyAckPath, "*.xml");
                    if (xmlFiles.Count() == 0)
                    {

                    }
                }

                string xmlFile = xmlFiles[0];

                string mndtReqTyp = string.Empty;
                switch (zipArry[6])
                {
                    case "ACCEPT":
                        mndtReqTyp = ECS.ACCEPT_ACK;
                        break;
                    case "CREATE":
                        mndtReqTyp = ECS.CREATE_ACK;
                        break;
                    default:
                        mndtReqTyp = zipArry[6];
                        break;
                }

                XDocument doc = XDocument.Load(xmlFile);
                XNamespace ns = doc.Root.GetDefaultNamespace();
                string umrNo = string.Empty;

                var accepDetails = doc.Descendants(ns + "UndrlygAccptncDtls").ToList();

                var orgnMsgNode = accepDetails.Descendants(ns + "OrgnlMsgInf").ToList();

                var msgId = (string)orgnMsgNode.Descendants(ns + "MsgId").FirstOrDefault();

                var mandate = mandateService.GetMandateByMsgId(msgId);
                string fileNo = string.Empty;
                string[] fileNoArr = zipName.Split('-');
                if (mandate == null)
                {
                    accepDetails = doc.Descendants(ns + "MndtAccptncRpt").ToList();

                    orgnMsgNode = accepDetails.Descendants(ns + "GrpHdr").ToList();

                    msgId = (string)orgnMsgNode.Descendants(ns + "MsgId").FirstOrDefault();

                    mandate = mandateService.GetECSByFileName(fileNoArr[5]);
                }

                if (mandate != null)
                {
                    var accpDetails = doc.Descendants(ns + "UndrlygAccptncDtls").ToList();

                    var accResult = accpDetails.Descendants(ns + "AccptncRslt").ToList();

                    var isAccept = (bool)accResult.Descendants(ns + "Accptd").FirstOrDefault();

                    var rejReason = accResult.Descendants(ns + "RjctRsn").ToList();

                    //reason
                    var reason = (string)rejReason.Descendants(ns + "Prtry").FirstOrDefault();

                    var orgnMndt = accpDetails.Descendants(ns + "OrgnlMndt").ToList();

                    if (isAccept)// &&  mndtReqTyp)
                    {
                        mandate.UMRNo = (string)orgnMndt.Descendants(ns + "MndtId").FirstOrDefault();
                    }

                    if (!isAccept)
                        mandate.MandateStatus = "Reject";

                    MandateFileHistory mandateHistory = null;
                    var xmlFileName = Path.GetFileName(xmlFile);
                    CreateLog("zipName.Contains");
                    //if (zipName.Contains("-ACCEPT-ACK.zip"))
                    mandateHistory = mandateService.GetAckByMandateId(mandate.Id);

                    if (mandateHistory != null)
                    {
                        CreateLog("mandate history");
                    }
                    else
                    {                        
                        CreateLog("mandate history null");
                        mandateHistory = new MandateFileHistory();
                        mandateHistory.MndtId = mandate.Id;
                        mandateHistory.XMLFile = xmlFileName.Replace(".xml", "");
                       // mandateHistory.FileExtn = ".xml";//zipName;
                    }
                    mandateHistory.MndtReqTyp = mndtReqTyp;
                    mandateHistory.AuthUser = "Schedule"; //StartUp.LOGIN_ID;
                    mandateHistory.DtStamp = DateTime.Now;
                    mandateHistory.FilePath = copyAckPath;

                    if (!zipName.Contains("-INP-ACK"))
                        mandateHistory.IsAccept = isAccept;

                    mandateHistory.Reason = reason;

                    //mandateService.UpdateHistory(mandateHistory);
                    //mandateService.Update(mandate);

                    new MandateService().Update(mandate, mandateHistory);

                    System.IO.Directory.Delete(copyAckPath, true);
                }
            }
            catch (Exception ex)
            {
                CreateLog(ex.Message + ex.StackTrace);
            }
        }

        public void GetAllPendingAck()
        {
            string ackPath = ConfigurationManager.AppSettings.Get("ACK_PATH");

            UtilityExtensions.IPersonateuser();

            var zipList = Directory.GetFiles(ackPath);

            try
            {
                foreach (var zip in zipList)
                {
                    if (zip.Contains("INP-ACK"))
                        USynchAcknowledge(zip);
                }

                ackPath = ConfigurationManager.AppSettings.Get("RES_PATH");

                zipList = Directory.GetFiles(ackPath);

                foreach (var zip in zipList)
                {
                    if (zip.Contains("-RES") || zip.Contains("ACCEPT-ACK"))
                        USynchAcknowledge(zip);
                }
            }
            catch (Exception ex)
            {
                CreateLog(ex.Message);
            }
        }
        public static void CreateLog(string message)
        {
            string logText;

            UtilityExtensions.IPersonateuser();

            logText = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            logText += Environment.NewLine;
            logText += message + Environment.NewLine;
            logText += "-----------------------------------------------------------";

            string path = ConfigurationManager.AppSettings["LOG_FILE_PATH"];
            // path += @"\SMSLog.txt";
            string fileExten = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
            path += @"\EcsScheduleLog" + fileExten + ".txt";
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            using (StreamWriter writer = File.AppendText(path))
            {
                writer.WriteLine(logText);
                writer.Flush();
                writer.Close();
            }
        }
    }
}