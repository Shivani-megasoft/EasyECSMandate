using EasyBank.BusinessLayer.Accounts.Contracts;
using EasyBank.BusinessLayer.Accounts.Services;
using EasyBank.BusinessLayer.HOAdmin.Contracts;
using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EasyBank.Framework.EasySecurity;
using ECSMandate.Models;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class SignatureController : AppController
    {
        //ISignatureService signatureService = new SignatureService();
        IBranchService branchService = new BranchService();


        public static void IPersonateuser()
        {
            ImpersonateUser iU = new ImpersonateUser();
            var domain = ConfigurationManager.AppSettings["Domain"];
            var userName = ConfigurationManager.AppSettings["UserName"];
            var pwd = ConfigurationManager.AppSettings["Password"];

            iU.Impersonate(domain, userName, pwd);
        }
        public static void IPersonateSignUser()
        {
            ImpersonateUser iU = new ImpersonateUser();
            var domain = ConfigurationManager.AppSettings["SignDomain"];
            var userName = ConfigurationManager.AppSettings["SignUserName"];
            var pwd = ConfigurationManager.AppSettings["SignPwd"];

            iU.Impersonate(domain, userName, pwd);
        }


        #region "GetSignatureFiles"
        [HttpPost]
        public ActionResult GetSignatureFiles(string brnCd, string glCd, string acNo, string memNo)
        {
            var response = new Response();
            var account = AccountService.GetByNo(acNo);
            List<SignatureModel> signatureList = null;

            string custName = string.Empty;
            if (null != account)
            {
                custName = account.Name;
                if (account.AcStatus.Name != "Operative" && !StartUp.ALLOW_CBS_TRN)
                {
                    response.Message = "A/c is " + account.AcStatus.Name + ", no Authority to view the Signature !";
                    response.Status = ServiceStatus.ValidationError;
                    return Json(response);
                }
                if (!string.IsNullOrWhiteSpace(memNo))
                {
                    var member = MemberService.GetByNo(brnCd, account.CustNo, memNo);
                    if (null == member)
                    {
                        response.Message = "Member not found !";
                        response.Status = ServiceStatus.ValidationError;
                        return Json(response);
                    }
                    custName = member.Name;
                }
                Branch branch = branchService.GetByCode(brnCd);
                if (branch == null)
                {
                    response.Message = "Branch not found !";
                    response.Status = ServiceStatus.ValidationError;
                    return Json(response);

                }


                string signatureFilePath = ConfigurationManager.AppSettings.Get("SIGNATURE_FILE_PATH") + @"\" + brnCd;
                //[CR224]: Signature- In case of Interbranch- Access from DataCenter else access from local network. --start                
                if (StartUp.BRN_CD != brnCd)
                {
                    
                    //string interBranchSignaturePath = StartUp.INTERBRANCH_SIGNATURE_FILE_PATH + account.BrnCd + @"\";
                    //if (!string.IsNullOrEmpty(interBranchSignaturePath))
                    //{
                    //    signatureFilePath = interBranchSignaturePath;
                    //}
                }
                //[CR224]: Signature- In case of Interbranch- Access from DataCenter else access from local network. --end

                //string signatureFileName = brnCd + "-" + glCd + "-" + acNo + ((!string.IsNullOrWhiteSpace(memNo))?memNo.Trim(): "") + "*.jpg";
                string signatureFileName = acNo + "*.jpg";
                if (!string.IsNullOrWhiteSpace(memNo))
                {
                    signatureFileName = brnCd + "-" + account.CustNo + "-" + memNo + "*.jpg";
                }

                IPersonateuser();
                IPersonateSignUser();
                var signFiles = Directory.GetFiles(signatureFilePath, signatureFileName);

                if (signFiles.Length > 0)
                {
                    signatureList = new List<SignatureModel>();
                    foreach (string sig in signFiles.OrderByDescending(s => s))
                    {
                        var sigModel = new SignatureModel();
                        sigModel.FileName = sig.Substring(sig.LastIndexOf(@"\") + 1);
                        sigModel.FilePath = sig.Substring(0, sig.LastIndexOf(sigModel.FileName) - 1);
                        sigModel.Src = "/File/GetFile?file=" + sigModel.FileName + "&path=" + sigModel.FilePath;
                        sigModel.SignFileDt = System.IO.File.GetCreationTime(sig);
                        signatureList.Add(sigModel);
                    }

                    //response.Item = signatureList;
                }

                response.Item = new
                {
                    SignatureList = signatureList,
                    CustName = custName
                };
            }

            return Json(response);
        }
        #endregion
    }
}