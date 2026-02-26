using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using EasyECS.Models;
//using EASYECS.Framework.EasyUtility;
//using EASYECSService;
using System.Security.Cryptography;
//using EASYECS.DataLayer.DataObjects;
//using EASYECS.Framework.EasyUtility;
using EasyBank.Framework.EasyUtility;
using ServiceObjects.Common;
using EasyBank.BusinessLayer.Users.Services;
using EasyBank.BusinessLayer.Users.Objects;
using System.Text;
using EasyBank.Infrastructure.SystemParameters;
using System.Web.Security;
using EasyBank.Framework.EasySecurity;
using System.Configuration;
using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.BusinessLayer.HOAdmin.Contracts;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSMandate;
using ECSService.Service;
using System.Drawing;
using EasyBank.Framework.EasySession;
using EasyBank.Framework.EasyCaching;
using System.IO;

namespace EasyECS.Controllers
{
    public class AccountController : Controller
    {
        //UserMasterService userService = new UserMasterService();
        UserService userService = new UserService();
        ISysControlService sysControlService = new SysControlService();
        ISysDayControlService sysDayControlService = new SysDayControlService();
        MandateCommonService mandateCommonService = new MandateCommonService();

        //
        // GET: /Account/
        public ActionResult Index()
        {
            LoginModel loginModel = new LoginModel();


            CreateCaptchaImage();

            string strCaptcha = MandateCommonService.GetRandomText();
            Session["Captcha"] = strCaptcha;
            ImageConverter converter = new ImageConverter();
            ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String((byte[])converter.ConvertTo(MandateCommonService.CaptchaGeneration(strCaptcha), typeof(byte[])));



            Session["BnkName"] = ConfigurationManager.AppSettings["BankName"].ToString();
            return View("Login");
        }


        public ActionResult ReloadCaptcha()
        {
            Response response = new Response();
            string[] strArray = new string[36];
            strArray = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            Random autoRand = new Random();
            string strCaptcha = string.Empty;
            for (int i = 0; i < 6; i++)
            {
                int j = Convert.ToInt32(autoRand.Next(0, 62));
                strCaptcha += strArray[j].ToString();
            }
            Session["Captcha"] = strCaptcha;
            ImageConverter converter = new ImageConverter();


            response.Item = "data:image/png;base64," + Convert.ToBase64String((byte[])converter.ConvertTo(MandateCommonService.CaptchaGeneration(strCaptcha), typeof(byte[])));

            return Json(response);
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]

        internal static string GenerateSalt()
        {
            byte[] buf = new byte[16];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            Session["BnkName"] = ConfigurationManager.AppSettings["BankName"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel loginModel, string returnUrl)
        {
            string error = "";
            Response response = new Response();

            if (!ModelState.IsValid)
            {
                CreateCaptchaImage();
                ModelState.AddModelError("Password", "Please enter required Details..!");
                return Index();
            }

            //string empty = string.Empty;


            if (string.IsNullOrEmpty(loginModel.Captcha))
            {
                ModelState.AddModelError("Captcha", "Please enter captcha.");
                CreateCaptchaImage();
                loginModel.Captcha = string.Empty;
                return View(loginModel);
            }
            else if (Session["CAPTCHA"].ToString() != loginModel.Captcha)
            {
                ModelState.Clear();
                CreateCaptchaImage();
                loginModel.Captcha = string.Empty;
                ModelState.AddModelError("Captcha", "Please enter correct captcha.");
                return View(loginModel);
            }

            try
            {
                var userList = userService.GetUsersByLoginId(loginModel.LoginId.ToUpper());
                if (userList.Count == 0)
                {
                    error = "Please enter correct login Id or password !";
                    ViewBag.Error = error;

                    ModelState.Clear();
                    ModelState.AddModelError("LoginId", error);
                    return View("Login");
                }
                else if (userList.Count > 1)
                {
                    error = "User has multiple logins !";
                    ViewBag.Error = error;
                    ModelState.Clear();
                    ModelState.AddModelError("LoginId", error);
                    return View("Login");
                }

                User user = userList[0];

                LoginHistory log = null;
                //FPSChecking 19062025
                if (StartupManager.StartUp.FPS_AUTH)
                {
                    ViewBag.UserId = user.Id;

                    if (string.IsNullOrWhiteSpace(user.FPS1) && string.IsNullOrWhiteSpace(user.FPS2) && string.IsNullOrWhiteSpace(user.FPS3))
                    {
                        error = "Kindlly Register fingerprint in CBS !";
                        ModelState.AddModelError("LoginId", error);

                    }
                }
                if (user == null)
                {
                    error = "Please enter correct login Id or password !";
                    ViewBag.Error = error;

                    ModelState.AddModelError("LoginId", error);
                    return View();
                }

                else if (loginModel.LoginId.ToLower().Trim() != "admin" && user.Logged /*&& (null != isLoggedWarningShown && !Convert.ToBoolean(isLoggedWarningShown))*/)
                {
                    //error = "User already logged on another Terminal ! \n Forcefully Login will kill the previous user's session. This will might result in incomplete transaction.";
                    //TempData["Logged"] = true;
                    CreateCaptchaImage();
                    loginModel.Captcha = string.Empty;
                    ModelState.AddModelError("LoginId", "User already logged on another Terminal ! \n Forcefully Login will kill the previous user's session. This will might result in incomplete transaction.");

                    //ViewBag.Logged = "True";
                    return View();
                }

                else if (user.Locked)
                {
                    error = "User is Locked. Please contact the Administrator !";
                    ModelState.AddModelError("LoginId", error);
                }
                else if (user.Status == "Disabled")
                {
                    error = "User is Disabled. Please contact the Administrator !";
                    ModelState.AddModelError("LoginId", error);
                }
                else
                {
                    //shyamal01012025
                    string pass1 = EasyHelper.EncodePassword(DecryptStringAES(loginModel.Pass), 1, user.PasswordSalt);
                    //string pass1 = EncodePassword(DecryptStringAES(loginModel.Pass), user.PasswordFormat, user.PasswordSalt);

                    if (pass1.Length > 128 || user.Pass != pass1)
                    {
                        error = "Please enter correct Password !";
                        ModelState.AddModelError("Pass", "Password is incorrect!. Please contact to administrator.");
                    }
                    else
                    {


                        //Allow One session per user

                        UserDetails userDetails = null;
                        if (StartupManager.StartUp.ALLOW_ONE_SESSION_PER_USER)
                        {
                            FormsAuthTokenStore formsAuthTokenStore = new FormsAuthTokenStore();
                            user.Logged = true;
                            //user.Token = formsAuthTokenStore.GenerateNewToken(loginModel.LoginId);
                            userDetails = userService.GetUserDetailsByUserId(user.Id.ToString());
                            if (null == userDetails)
                            {
                                userDetails = new UserDetails();
                                userDetails.UserId = user.Id.ToString();
                                //userDetails.IsNew = true;
                            }
                            userDetails.Token = formsAuthTokenStore.GenerateNewToken(loginModel.LoginId);
                            formsAuthTokenStore.SetClientAccess(userDetails.Token);


                            FormsAuthTokenStore formsAuthTokenStore1 = new FormsAuthTokenStore();
                            formsAuthTokenStore1.RemoveClientAccess();
                            user.FailAttempt = 0;
                            user.Locked = false;
                            user.Logged = true;

                            log = new LoginHistory();

                            log.LoginDate = DateTime.Now.Date + DateTime.Now.TimeOfDay;
                            log.LoginTime = DateTime.Now.TimeOfDay.Hours.ToString() + ":" + DateTime.Now.TimeOfDay.Minutes.ToString() + ":" + DateTime.Now.TimeOfDay.Seconds.ToString();
                            //log.LogOffTime = DateTime.Now.TimeOfDay.Hours.ToString() + ":" + DateTime.Now.TimeOfDay.Minutes.ToString() + ":" + DateTime.Now.TimeOfDay.Seconds.ToString();
                            //log.LogOffTime = Session["LogOffTime"].ToString();
                            log.UserId = user.Id;
                            log.IPAddr = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                            log.Stat = true;

                            //log.AppId = ConfigurationManager.AppSettings["ApplId"];
                            SessionManager<string>.Add("IPAddress", loginModel.IPAddress);

                            user.LastLoginDate = DateTime.Now.Date;
                            //userService.InsertLoginHistory(log);
                            if (StartupManager.StartUp.ALLOW_ONE_SESSION_PER_USER)
                            {
                                userService.Update(user, userDetails);
                            }
                            else
                            {
                                userService.Update(user);
                            }

                            //amlServices.Update(userMaster);



                            //add value n session
                            SessionManager<string>.Add("IPAddress", log.IPAddr);
                            StartupManager.StartUp.LOGIN_ID = user.LoginId;
                            //[CR210]: Apply Permissions in Web
                            StartupManager.StartUp.SYS_ADMIN = user.SysAdmin;
                            StartupManager.StartUp.USER_CATEGORY = user.UserCat;
                            StartupManager.StartUp.BRN_CD = user.BrnCd;
                            //StartupManager.StartUp.BRN_LIST = userService.GetAllBranches();
                            //StartupManager.StartUp.BRN_DIST = branch.Dist;
                            //StartupManager.StartUp.BRN_STATE = branch.State;

                            Session["LoginId"] = user.LoginId;

                            //if (branch != null)
                            //    Session["BrName"] = branch.Name;

                            Session["BnkName"] = StartupManager.StartUp.BNK_NAME;
                        }

                        if (string.IsNullOrWhiteSpace(error))
                        {
                            FormsAuthenticationTicket authenticationTicket = new FormsAuthenticationTicket(1, loginModel.LoginId, DateTime.Now, DateTime.Now.AddMinutes(60), false, user.BrnCd + "|" + user.Id.ToString());
                            string ticket = FormsAuthentication.Encrypt(authenticationTicket);
                            HttpCookie authenticationCookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticket);
                            Response.Cookies.Add(authenticationCookie);

                            EasyBankPrincipal principal = new EasyBankPrincipal(new EasyBankIdentity(user.LoginId));//LoginId
                            principal.BrnCd = user.BrnCd;

                            principal.InterBranch = user.InterBranch;
                            HttpContext.User = principal;
                            System.Threading.Thread.CurrentPrincipal = principal;

                            StartupManager.StartUp.LOGIN_ID = user.LoginId;
                            //[CR210]: Apply Permissions in Web
                            StartupManager.StartUp.SYS_ADMIN = user.SysAdmin;
                            StartupManager.StartUp.USER_CATEGORY = user.UserCat;
                            StartupManager.StartUp.USER_BRN_CD = user.BrnCd;
                            StartupManager.StartUp.BRN_CD = user.BrnCd;
                            StartupManager.StartUp.BRN_LIST = BranchService.GetAll().Where(b => b.BrnCd == user.BrnCd || b.CBS).ToList();


                            CheckDayStatus();

                            //Mar19:35
                            Session["UserName"] = user.UserName;
                            StartupManager.StartUp.LOGIN_ID = user.LoginId;
                            Session["UserName"] = user.UserName;
                            Session["LoginId"] = user.LoginId;
                            return RedirectToAction("Index", "Home");
                            loginModel.Captcha = string.Empty;
                            CreateCaptchaImage();
                        }

                    }
                }
                
            }
            catch (Exception ex)
            {
                response.Item = ex.Message + ex.StackTrace;
                return Json(response);
            }
            return View();
        }
        //shyamal01012025
        private static string DecryptStringAES(string cipherText)
        {
            var keybytes = Encoding.UTF8.GetBytes("8080808080808080");
            var iv = Encoding.UTF8.GetBytes("8080808080808080");

            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            return string.Format(decriptedFromJavascript);
        }
        //shyamal01012025
        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.  
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold  
            // the decrypted text.  
            string plaintext = null;

            // Create an RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.  
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream  
                                // and place them in a string.  
                                plaintext = srDecrypt.ReadToEnd();

                            }

                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }


        //public static string DecryptStringAES(string cipherText)
        //{
        //    var keybytes = Encoding.UTF8.GetBytes("8080808080808080");
        //    var iv = Encoding.UTF8.GetBytes("8080808080808080");

        //    var encrypted = Convert.FromBase64String(cipherText);
        //    var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
        //    return string.Format(decriptedFromJavascript);
        //}
        //private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        //{
        //    // Check arguments.  
        //    if (cipherText == null || cipherText.Length <= 0)
        //    {
        //        throw new ArgumentNullException("cipherText");
        //    }
        //    if (key == null || key.Length <= 0)
        //    {
        //        throw new ArgumentNullException("key");
        //    }
        //    if (iv == null || iv.Length <= 0)
        //    {
        //        throw new ArgumentNullException("key");
        //    }

        //    // Declare the string used to hold  
        //    // the decrypted text.  
        //    string plaintext = null;

        //    // Create an RijndaelManaged object  
        //    // with the specified key and IV.  
        //    using (var rijAlg = new RijndaelManaged())
        //    {
        //        //Settings  
        //        rijAlg.Mode = CipherMode.CBC;
        //        rijAlg.Padding = PaddingMode.PKCS7;
        //        rijAlg.FeedbackSize = 128;

        //        rijAlg.Key = key;
        //        rijAlg.IV = iv;

        //        // Create a decrytor to perform the stream transform.  
        //        var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

        //        try
        //        {
        //            // Create the streams used for decryption.  
        //            using (var msDecrypt = new MemoryStream(cipherText))
        //            {
        //                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //                {

        //                    using (var srDecrypt = new StreamReader(csDecrypt))
        //                    {
        //                        // Read the decrypted bytes from the decrypting stream  
        //                        // and place them in a string.  
        //                        plaintext = srDecrypt.ReadToEnd();

        //                    }

        //                }
        //            }
        //        }
        //        catch
        //        {
        //            plaintext = "keyError";
        //        }
        //    }

        //    return plaintext;
        //}
        private void CheckDayStatus()
        {
            SysControl sysControl = sysControlService.GetMax();
            SysDayControl sysDayControl = null;
            //if (StartupManager.StartUp.CBS_BRN)
            //{
            sysDayControl = sysDayControlService.GetMax();

            if (sysDayControl == null)
            {
                return;
            }
            if (sysControl != null)
            {
                StartupManager.StartUp.PREV_DT = sysControlService.GetPrev().DayOpDt;
                StartupManager.StartUp.DAY_OP_DT = sysControl.DayOpDt;

                if (sysControl.DayOpInd == "O")
                {
                    StartupManager.StartUp.DAY_OPEN = true;
                }
                else
                {
                    StartupManager.StartUp.DAY_OPEN = false;
                    //lblDayStat.Text = StartUp.DAY_OP_DT.ToShortDateString() + "  [CLOSED]";
                }
            }

            // Added by Achyut
            //[CR385]: Day Open, Day Close in CBS
            if (StartupManager.StartUp.CBS_BRN)
            {
                StartupManager.StartUp.CBS_DAY_OP_DT = sysDayControl.DayOpDt;
                if (sysDayControl.DayOpInd == "O")
                {
                    StartupManager.StartUp.DAY_OPEN = true;
                    //lblDayStat.Text = StartUp.CBS_DAY_OP_DT.ToShortDateString() + "  [OPEN]";
                }
                else
                {
                    StartupManager.StartUp.DAY_OPEN = false;
                }
            }

            if (sysControl.SessInd == "O")
            {
                StartupManager.StartUp.SESSION_OPEN = true;
            }
            else if (StartupManager.StartUp.CBS_BRN)
            {
                StartupManager.StartUp.DAY_OP_DT = StartupManager.StartUp.CBS_DAY_OP_DT;
            }
            StartupManager.StartUp.SESSION_OPEN = false;
        }
        //public ActionResult Logout()
        //{
        //	Session.Clear();
        //	return RedirectToAction("Login", "Account");
        //}

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            //Commented tha cache clear to imrpove the performance.
            CreateCaptchaImage();



            //model.Captcha = string.Empty;
            CacheManager.Cache.RemoveAll();


            HttpCookie authCookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null && !string.IsNullOrWhiteSpace(authCookie.Value))
            {

                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var userData = ticket.UserData;
                var loginId = ticket.Name;

                string brnCd = userData.Split('|')[0];

                var userService = new UserService();

                var log = new LoginHistory();
                User user = null;

                var userList = userService.GetUsersByLoginId(loginId);
                user = userList[0];
                if (user != null)
                {
                    string sKey = user.LoginId;
                    string sUser = "";
                    sUser = Convert.ToString(System.Web.HttpContext.Current.Cache[sKey]);
                    if (sUser == null || sUser == String.Empty)
                    {

                        TimeSpan SessTimeOut = new TimeSpan(0, 0, System.Web.HttpContext.Current.Session.Timeout, 0, 0);
                        System.Web.HttpContext.Current.Cache.Insert(sKey, sKey, null, DateTime.MaxValue, SessTimeOut,
                        System.Web.Caching.CacheItemPriority.Default, null);

                        Session["samename"] = user.LoginId;
                    }
                    else
                    {
                        ViewBag.username = user.LoginId;
                    }

                    log = userService.GetLastLoginHistory(user.Id.ToString());


                    user.Logged = false;

                    userService.Update(user);
                    log.LogOffTime = DateTime.Now.TimeOfDay.Hours.ToString() + ":" + DateTime.Now.TimeOfDay.Minutes.ToString() + ":" + DateTime.Now.TimeOfDay.Seconds.ToString();
                    //amlServices.UpdateLoginHistory(log);
                }
            }



            authCookie.Expires = DateTime.Today.AddDays(-1);
            //Session.Abandon();
            if (StartupManager.StartUp.ALLOW_ONE_SESSION_PER_USER)
            {
                FormsAuthTokenStore tokenStore = new FormsAuthTokenStore();
                tokenStore.RemoveClientAccess();
            }
            //return RedirectToAction("Login", "Account");
            return View("Login", new LoginModel());
        }

        internal static string EncodePassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0) // MembershipPasswordFormat.Clear
                return pass;

            byte[] bIn = Encoding.Unicode.GetBytes(pass);
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            byte[] bRet = null;

            Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
            if (passwordFormat == 1)
            { // MembershipPasswordFormat.Hashed
                HashAlgorithm s = HashAlgorithm.Create("SHA1");
                bRet = s.ComputeHash(bAll);
            }
            else
            {
                bRet = (bAll);
            }
            return Convert.ToBase64String(bRet);
        }
        internal void LoadConfiguration()
        {
            MandateService mandateService = new MandateService();
            Session["Inward_File_Path"] = mandateService.GetEhConfigByField("Inward_File_Path");
            Session["Inward_Image_Path"] = mandateService.GetEhConfigByField("Inward_File_Path");
            Session["Inward_Response_Path"] = mandateService.GetEhConfigByField("Inward_File_Path");
            Session["Inward_Prcessed_Path"] = mandateService.GetEhConfigByField("Inward_File_Path");
            Session["Mandate_Scan_Path"] = mandateService.GetEhConfigByField("Mandate_Scan_Path");
            Session["Inward_Prcessed_Path"] = mandateService.GetEhConfigByField("Inward_File_Path");
            Session["Inward_Response_Path"] = mandateService.GetEhConfigByField("Inward_File_Path");
            Session["Inward_Prcessed_Path"] = mandateService.GetEhConfigByField("Inward_File_Path");
        }

        private void CreateCaptchaImage()
        {
            Session["Captcha"] = MandateCommonService.GetRandomText();
            string strCaptcha = Session["Captcha"].ToString();
            ImageConverter converter = new ImageConverter();
            ViewBag.Base64String = "data:image/png;base64," + Convert.ToBase64String((byte[])converter.ConvertTo(MandateCommonService.CaptchaGeneration(strCaptcha), typeof(byte[])));
        }
    }
}