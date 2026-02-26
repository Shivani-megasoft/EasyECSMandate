using System;
using System.Text;
using System.Reflection;
using EasyBank.Framework.EasyAttributes;
using EasyBank.BusinessLayer.Base.Objects;
using System.Collections.Generic;
using EasyBank.Framework.EasyCaching;
using EasyBank.Framework.EasySession;
using System.Net;


namespace EasyBank.DataLayer.DataObjects
{
    /// <summary>
    /// alter table ebAuditLog add   Entity varchar(100)
    /// alter table ebAuditLog add UpdatedBy varchar(20) null
    /// alter table ebAuditLog add KeyName varchar(10) null
    /// alter table ebAuditLog add BrnCd varchar(10) null
    /// 
    /*
        ALTER PROCEDURE [dbo].[uspGetAuditTrail]  
        @Criteria1 datetime,  
        @Criteria2 datetime  
        AS  
        SELECT * FROM ebAuditLog where Entity is not null 
	        AND Date between  @Criteria1 and @Criteria2
	        ORDER BY Date DESC, UpdatedBy, Entity
    */

    /*
        ALTER PROCEDURE [dbo].[uspInsertAuditTrail]
            @Id uniqueidentifier,
            @Date datetime,
            @Entity varchar(100),
            @Key varchar(100),
            @Data varchar(8000),
            @UpdatedBy varchar(20),
            @KeyName varchar(10),
	        @BrnCd varchar(10)
        AS
            SET NOCOUNT ON
            INSERT INTO ebAuditLog(Id, Date, [Key], Data, Entity, UpdatedBy, KeyName, BrnCd)
		        VALUES (@Id, GetDate(), @Key, @Data, @Entity, @UpdatedBy, @KeyName, @BrnCd)
            RETURN
     */

    /// </summary>
    internal class AuditTrailService
    {
        private static List<string> skipAuditFields = GetAuditSkipFields();

        private static List<string> GetAuditSkipFields()
        {
            var skipFields = new List<string>();
            //comment by pritam
            skipFields.Add("operation");
            //skipFields.Add("deleted");
            skipFields.Add("logged");
            //skipFields.Add("pass");
            return skipFields;
        }

        //CR-201 Old method commited--29/01/2016-
        //public AuditTrail GetAuditLog(BaseObject orgObj, string loginId, string brnCd)
        //{
        //    AuditTrail auditLog = new AuditTrail();

        //    //string loggedInUser = string.Empty;

        //    // PRE-CONDITIONS
        //    if (null == orgObj)
        //        return null;

        //    if (null == orgObj.Snapshot)
        //        return null;

        //    object[] attributes = null;
        //    object orgVal = null, logVal = null;

        //    StringBuilder sb = new StringBuilder();

        //    PropertyInfo[] pInfo = orgObj.GetType().GetProperties();

        //    object key = null;

        //    try
        //    {
        //        foreach (PropertyInfo pi in pInfo)
        //        {
        //            // Check if ColumnAttribute present.  This is required for audit logging.
        //            attributes = pi.GetCustomAttributes(typeof(ColumnAttribute), true);

        //            if (attributes.GetLength(0) == 0)
        //            {
        //                continue;
        //            }

        //            // Check if any property needs to be skipped
        //            attributes = pi.GetCustomAttributes(typeof(SkipAuditAttribute), true);
        //            if (attributes.GetLength(0) > 0)
        //            {
        //                continue;
        //            }

        //            // Also check skip fields in the local variable.  This is the shortcut route for skiping any
        //            // fields being logged.
        //            if (skipAuditFields.Contains(pi.Name.ToLower()))
        //            {
        //                continue;
        //            }

        //            // Get the primay key.  This is used to log the key and its value to the DB.
        //            attributes = pi.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);

        //            if (attributes.GetLength(0) > 0)
        //            {
        //                key = pi.GetValue(orgObj, null);
        //                if (null != key)
        //                {
        //                    auditLog.Key = key.ToString();
        //                }

        //                auditLog.KeyName = pi.Name;
        //            }


        //            if (pi.GetValue(orgObj, null) != null)
        //            {
        //                logVal = pi.GetValue(orgObj, null);

        //                //if (null != logVal)
        //                //{
        //                //    // Get the logged in user.  Loggedin user is stored in the "authuser" property.
        //                //    if (pi.Name.ToLower() == "authuser")
        //                //    {
        //                //        loggedInUser = logVal.ToString();
        //                //    }
        //                //}


        //                // Skp if the orginal value is null.
        //                orgVal = pi.GetValue(orgObj.Snapshot, null);

        //                if (null == orgVal)
        //                {
        //                    orgVal = "";
        //                }

        //                // Check if Lazy loading required.  IF yes, fetch the values from cache.
        //                attributes = pi.GetCustomAttributes(typeof(LazyAttribute), true);

        //                if (attributes.GetLength(0) > 0)
        //                {
        //                    LazyAttribute attr = attributes[0] as LazyAttribute;

        //                    string newCacheId = attr.TypeName + "~" + logVal.ToString();
        //                    string oldCacheId = attr.TypeName + "~" + orgVal.ToString();

        //                    object newVal = CacheManager.Cache.Get(newCacheId);
        //                    object oldVal = CacheManager.Cache.Get(oldCacheId);

        //                    if (null != newVal)
        //                    {
        //                        if (oldVal != newVal)
        //                        {
        //                            sb.Append(pi.Name + " : " + oldVal + " | " + newVal + " ^ ");
        //                        }
        //                    }
        //                }
        //                else  // otherwise log the values as is.
        //                {
        //                    if (!orgVal.Equals(logVal))
        //                    {
        //                        sb.Append(pi.Name + " : " + orgVal + " | " + logVal + " ^ ");
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {	// do nothing here. 
        //    }

        //    if (sb.ToString() == string.Empty)
        //    {
        //        return null;
        //    }

        //    //loggedInUser = SessionManager<string>.Get("LOGIN_ID"); // CacheManager.Cache["LOGGED_IN_USER"] != null ? CacheManager.Cache["LOGGED_IN_USER"].ToString() : string.Empty;

        //    auditLog.BrnCd = brnCd;// SessionManager<string>.Get("BRN_CD");//CacheManager.Cache["BRANCH_CODE"].ToString();
        //    auditLog.Date = DateTime.Now;
        //    auditLog.UpdatedBy = loginId; //loggedInUser;
        //    auditLog.Entity = orgObj.GetType().Name;
        //    auditLog.Data = sb.ToString();

        //    return auditLog;
        //}
        public AuditTrail GetAuditLog(BaseObject orgObj, string loginId, string brnCd, string ipAddress)
        {
            AuditTrail auditLog = new AuditTrail();

            //string loggedInUser = string.Empty;

            // PRE-CONDITIONS
            if (null == orgObj)
                return null;

            if (null == orgObj.Snapshot)
                return null;

            object[] attributes = null;
            object orgVal = null, logVal = null;

            StringBuilder sb = new StringBuilder();

            PropertyInfo[] pInfo = orgObj.GetType().GetProperties();

            object key = null;

            try
            {
                foreach (PropertyInfo pi in pInfo)
                {
                    // Check if ColumnAttribute present.  This is required for audit logging.
                    attributes = pi.GetCustomAttributes(typeof(ColumnAttribute), true);

                    if (attributes.GetLength(0) == 0)
                    {
                        continue;
                    }

                    // Check if any property needs to be skipped
                    attributes = pi.GetCustomAttributes(typeof(SkipAuditAttribute), true);
                    if (attributes.GetLength(0) > 0)
                    {
                        continue;
                    }

                    // Also check skip fields in the local variable.  This is the shortcut route for skiping any
                    // fields being logged.
                    if (skipAuditFields.Contains(pi.Name.ToLower()))
                    {
                        continue;
                    }

                    // Get the primay key.  This is used to log the key and its value to the DB.
                    attributes = pi.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);

                    if (attributes.GetLength(0) > 0)
                    {
                        key = pi.GetValue(orgObj, null);
                        if (null != key)
                        {
                            auditLog.Key = key.ToString();
                        }

                        auditLog.KeyName = pi.Name;
                    }


                    if (pi.GetValue(orgObj, null) != null)
                    {
                        logVal = pi.GetValue(orgObj, null);

                        //if (null != logVal)
                        //{
                        //    // Get the logged in user.  Loggedin user is stored in the "authuser" property.
                        //    if (pi.Name.ToLower() == "authuser")
                        //    {
                        //        loggedInUser = logVal.ToString();
                        //    }
                        //}


                        // Skp if the orginal value is null.
                        orgVal = pi.GetValue(orgObj.Snapshot, null);

                        if (null == orgVal)
                        {
                            orgVal = "";
                        }

                        // Check if Lazy loading required.  IF yes, fetch the values from cache.
                        attributes = pi.GetCustomAttributes(typeof(LazyAttribute), true);

                        //temp comment to get values from master table
                        //if (attributes.GetLength(0) > 0)
                        //{
                        //    LazyAttribute attr = attributes[0] as LazyAttribute;

                        //    string newCacheId = attr.TypeName + "~" + logVal.ToString();
                        //    string oldCacheId = attr.TypeName + "~" + orgVal.ToString();

                        //    object newVal = CacheManager.Cache.Get(newCacheId);
                        //    object oldVal = CacheManager.Cache.Get(oldCacheId);

                        //    if (null != newVal)
                        //    {
                        //        if (oldVal != newVal)
                        //        {
                        //           // sb.Append(pi.Name + " : " + oldVal + " | " + newVal + " ^ ");
                        //            auditLog.Header += pi.Name + "|";
                        //            auditLog.NewData += logVal + "|";
                        //            auditLog.OldData += orgVal + "|";

                        //        }
                        //    }
                        //}
                        //else  // otherwise log the values as is.
                        //{
                        if (!orgVal.Equals(logVal))
                        {
                            // sb.Append(pi.Name + " : " + orgVal + " | " + logVal + " ^ ");
                            auditLog.Header += pi.Name + "|";
                            auditLog.NewData += logVal + "|";
                            auditLog.OldData += orgVal + "|";

                        }
                        //}
                    }

                }
            }
            catch (Exception ex)
            {	// do nothing here. 
            }

            //if (sb.ToString() == string.Empty)
            //{
            //    return null;
            //}
            if (string.IsNullOrEmpty(auditLog.Header))
                return null;
            //loggedInUser = SessionManager<string>.Get("LOGIN_ID"); // CacheManager.Cache["LOGGED_IN_USER"] != null ? CacheManager.Cache["LOGGED_IN_USER"].ToString() : string.Empty;

            auditLog.BrnCd = brnCd;// SessionManager<string>.Get("BRN_CD");//CacheManager.Cache["BRANCH_CODE"].ToString();
            auditLog.Date_ = DateTime.Now;
            auditLog.UpdatedBy = loginId; //loggedInUser;
            auditLog.Entity = orgObj.GetType().Name;

            //New IP Address
            //ujala added
            //string ipAddress = string.Empty;
            //Nov18:19 try catch block added 
            try
            {
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            catch (Exception ex)
            {
            }
            //ujala commented all code for testing
            //string strHostName = System.Net.Dns.GetHostName();
            //IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            //IPAddress[] addr = ipEntry.AddressList;
            //string ip = addr[1].ToString();
            auditLog.IPAddr = ipAddress;

            //END
            return auditLog;
        }
    }
}