using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.BusinessLayer.Users.Objects;
using EasyBank.BusinessLayer.Users.Services;
using EasyBank.Framework.EasyAttributes;
using EasyBank.Framework.EasyExceptions;
using EasyBank.Framework.EasyExtensions;
using EasyBank.Framework.EasyLogging;
using EasyBank.Framework.EasySecurity;
using EasyBank.Infrastructure.SystemParameters;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;

namespace ECSMandate.Controllers
{
    [HandleError]
    //[NoCache]
    public class AppController : Controller
    {
        public IStartUp StartUp
        {
            get
            {
                return StartupManager.StartUp;
            }
        }

        public IConfigStartUp ConfigStartUp
        {
            get
            {
                return StartUpConfigManager.ConfigStartUp;
            }
        }

        public object RouteTable { get; private set; }

        protected ViewResult AjaxView(string viewName, object model)
        {
            if (Request.Headers["X-PJAX"] != null)
            {
                return View(viewName, model);
            }
            else
            {
                return View(viewName, "_Layout", model);
            }
        }
        protected ViewResult AjaxView(string viewName)
        {
            if (Request.Headers["X-PJAX"] != null)
            {
                return View(viewName);
            }
            else
            {
                return View(viewName, "_Layout");
            }
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is TableStructureException ||
                    filterContext.Exception is StartUpParameterException)
            {
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                filterContext.Result = new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Status = ServiceStatus.ValidationError,
                        filterContext.Exception.Message
                    }
                };
                filterContext.ExceptionHandled = true;
            }
            else if (filterContext.Exception is AuthenticationTimeoutException)
            {
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                string type = "timeout";
                filterContext.Result = new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        type,
                        filterContext.Exception.Message,
                        Status = ServiceStatus.ValidationError
                    }
                };
                filterContext.ExceptionHandled = true;
            }
            else
            {
                EasyLogger.Log(filterContext.Exception.GetDetails(), "General", 1, TraceEventType.Error);
                Trace.WriteLine("Error Teaced");
                Trace.TraceError(filterContext.Exception.Message);
               
                string Message = filterContext.Exception.Message;
                //Message = filterContext.Exception.Message;  // todo: remove this;
                if (filterContext.Exception is UserRightsException)
                {
                    Message = filterContext.Exception.Message;
                }

                filterContext.Result = new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Status = ServiceStatus.ValidationError,
                        Message
                    }
                };
                filterContext.ExceptionHandled = true;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string loginFormName = @"Account\Index";
            string userId = string.Empty;
            HttpCookie userCookie = Response.Cookies["CBSUserDetails"];
            if (null == userCookie || (null != userCookie && userCookie.Values.Count == 0))
            {
                userCookie = Request.Cookies["CBSUserDetails"];
            }
            if (null != userCookie)  //&& !string.IsNullOrWhiteSpace(userCookie.Value)
            {
                if (null != userCookie["UserCategory"])
                {
                    loginFormName = userCookie["UserCategory"] == UserCategoryType.DCUser ? "DCUserLogin" : "Login";
                }
                if (null != userCookie["UserId"])
                {
                  userId = userCookie["UserId"];
                }
            }

            HttpCookie authCookie = filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
            {
                ClearFormsAuthenticationTicket(true, true);

                if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                {
                    throw new AuthenticationTimeoutException("Authentication ticket expired.");
                }
                else
                {
                    RedirectToLoginPage(loginFormName, userId);
                    return;
                }
            }

            var ticket = FormsAuthentication.Decrypt(authCookie.Value);
            var userData = ticket.UserData;
            var loginId = ticket.Name;
            string brnCd = string.Empty;

            try
            {
                brnCd = userData.Split('|')[0];
                userId = userData.Split('|')[1];
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Error);
                RedirectToLoginPage(loginFormName, userId);
                return;
            }

            ViewBag.EBVersion = "Ver EM 10.0(18.09.24)";

            if (StartUp == null)
            {
                RedirectToLoginPage(loginFormName, userId);
                return;
            }

            if (ConfigStartUp == null)
            {
                RedirectToLoginPage(loginFormName, userId);
                return;
            }
            //added to improve performance - avoid on database call for each request
            bool interBranch = false;
            try
            {
                interBranch = StartUp.IS_INTERBRANCH;
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Error);
                RedirectToLoginPage(loginFormName, userId);
                return;
            }
            //var userBrnCd = StartUp.USER_BRN_CD;

            //EasyBankPrincipal principal = new EasyBankPrincipal(new EasyBankIdentity(user.LoginId));
            EasyBankPrincipal principal = new EasyBankPrincipal(new EasyBankIdentity(loginId));
            //principal.BrnCd = user.BrnCd;
            principal.BrnCd = brnCd;
            //principal.User = user;
            //[CR356]: Inter branch transactions should be based on rights for user
            //principal.InterBranch = user.InterBranch;
            //CR1825 Add DBFlag            
            //   principal.DBFlag = StartUp.DB_FLAG;
            principal.InterBranch = interBranch;
            HttpContext.User = principal;
            System.Threading.Thread.CurrentPrincipal = principal;
            CheckSessionExpiry(filterContext, brnCd, loginId, loginFormName);
                                  
            base.OnActionExecuting(filterContext);
            userCookie.Expires = ticket.Expiration;
            Response.Cookies.Set(userCookie);
        }

        private void CheckSessionExpiry(ActionExecutingContext filterContext, string brnCd, string loginId, string loginFormName)
        {
            HttpContextBase ctx = filterContext.HttpContext;

            // check if session is supported
            if (ctx.Session != null)
            {
                // check if a new session id was generated
                if (ctx.Session.IsNewSession)
                {
                    // If it says it is a new session, but an existing cookie exists, then it must
                    // have timed out
                    string sessionCookie = ctx.Request.Headers["Cookie"];
                    if ((null != sessionCookie) && (sessionCookie.IndexOf("ASP.NET_SessionId") >= 0))
                    {
                        if (loginFormName == "Login")
                        {
                            LogOffUser(brnCd, loginId);
                        }
                        else if (loginFormName == "DCUserLogin")
                        {
                            LogOffDCUser(loginId); //EmpNo as LoginId
                        }
                        RedirectToLoginPage(loginFormName);
                    }
                }
            }
        }
        protected static void LogOffDCUser(string loginId)
        {
            var userService = new UserService();
            //var userList = userService.GetUserByEmpNo(loginId);
            var userList = userService.GetUsersByLoginId(loginId);
            if (userList.Count == 1)
            {
                userList[0].Logged = false;
                userService.Update(userList[0]);
            }
        }
        protected static void LogOffUser(string brnCd, string loginId)
        {
            var userService = new UserService();
            User user = null;
            try
            {
                user = userService.GetByLoginId(brnCd, loginId);
            }
            catch
            {

            }
            if (user != null)
            {
                user.Logged = false;
                userService.Update(user);
            }
        }
        private void ClearFormsAuthenticationTicket(bool severeError, bool removeAllCookies)
        {
            FormsAuthentication.SignOut();
            if (HttpContext.Response != null && HttpContext.Response.Cookies != null && FormsAuthentication.FormsCookieName != null)
            {
                HttpContext.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            }
            if (severeError && HttpContext.Session != null)
            {
                HttpContext.Session.Abandon();
            }
            if (removeAllCookies)
            {
                RemoveCookies();
            }
        }
        private void RedirectToLoginPage(string loginFormName, string userId)
        {
            ClearFormsAuthenticationTicket(false, true);

            var userService = new UserService();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                //var user = userService.GetById(Convert.ToInt32(userId)); change for SQL
                var user = userService.GetById(userId);
                if (null != user)
                {
                    user.Logged = false;
                    userService.Update(user);
                }
            }

            if (!string.IsNullOrWhiteSpace(loginFormName))
            {
                Response.Redirect("~/" + loginFormName);
            }
            else
            {
                Response.Redirect(FormsAuthentication.LoginUrl);
            }
        }
        private void RedirectToLoginPage(string loginFormName)
        {
            ClearFormsAuthenticationTicket(false, true);

            if (!string.IsNullOrWhiteSpace(loginFormName))
            {
                Response.Redirect("~/" + loginFormName);
            }
            else
            {
                Response.Redirect(FormsAuthentication.LoginUrl);
            }
        }
        private void RemoveCookies()
        {
            try
            {
                if (HttpContext.Response != null && HttpContext.Response.Cookies != null)
                {
                    HttpContext.Response.Cookies.Clear();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult ShowError(string message)
        {
            Response response = new Response();
            response.Message = message;
            response.Status = ServiceStatus.ValidationError;
            return Json(response);
        }

        public BaseObject PopulateNewObject(BaseObject orgObj, BaseObject newObj, bool isCheckForColumnAttribute)
        {           
            // PRE-CONDITIONS
            if (null == orgObj)
                return null;

            if (null == newObj)
                return null;

            object[] attributes = null;

            PropertyInfo[] pInfo = newObj.GetType().GetProperties();

            object key = null;

            try
            {
                foreach (PropertyInfo pi in pInfo)
                {
                    // Check if ColumnAttribute present.  This is required for audit logging.
                    if (isCheckForColumnAttribute)
                    {
                        attributes = pi.GetCustomAttributes(typeof(ColumnAttribute), true);

                        if (attributes.GetLength(0) == 0)
                        {
                            continue;
                        }
                    }
                    //key = pi.GetValue(orgObj, null);
                    var orgProp = orgObj.GetType().GetProperties().Where(p => p.Name == pi.Name).SingleOrDefault();
                    if (null != orgProp)
                    {
                        key = orgProp.GetValue(orgObj, null);
                        if (null != key)
                        {
                            //pi.SetValue(newObj, key, null);
                            if (pi.PropertyType.Name != "Int32")
                            {
                                Type t = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                                if (pi.Name == "Id") //|| pi.PropertyType.Name != orgProp.PropertyType.Name
                                {
                                    pi.SetValue(newObj, key.ToString(), null);
                                }
                                else
                                {
                                    pi.SetValue(newObj, Convert.ChangeType(key, t), null);
                                }
                            }
                            else
                            {
                                pi.SetValue(newObj, Convert.ToInt32(key.ToString()), null);
                            }
                        }
                        else if (pi.PropertyType.Name == "String")
                        {
                            pi.SetValue(newObj, string.Empty, null);
                        }
                    }
                    else if (pi.PropertyType.Name == "String")
                    {
                        pi.SetValue(newObj, string.Empty, null);

                    }
                }
            }
            catch (Exception ex)
            {
            }
            return newObj;
        }
        public void SaveXMLToDir(XmlDocument xmlDoc, string fileName)
        {
            var imagePath = ConfigurationManager.AppSettings.Get("MandateFilePath");
            xmlDoc.Save(imagePath + @"\" + fileName + ".xml");
        }       
    }
}