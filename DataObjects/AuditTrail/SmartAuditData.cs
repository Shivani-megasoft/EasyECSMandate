using System;
using System.IO;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using EasyBank.Framework.EasyAttributes;
using EasyBank.BusinessLayer.Base.Objects;
using System.Threading;
using System.Configuration;
using System.Web;
using System.Web.Security;
using Oracle.DataAccess.Client;

namespace EasyBank.DataLayer.DataObjects
{
    /// <summary>
    /// Used only for audit log.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SmartAuditData<T>
    {
        DataSet dsSQL, dsReport;
        AuditDBEngine dbEngine = new AuditDBEngine();

        public SmartAuditData()
        {
            dsSQL = new DataSet();
            string applType = ConfigurationManager.AppSettings.Get("ApplType");
            if (applType == "Web")
            {
                dsSQL.ReadXml(HttpContext.Current.Server.MapPath("~/Queries/SqlQuery.xml"));
            }
            else
            {
                dsSQL.ReadXml("SqlQuery.xml");
            }

            if (File.Exists("ReportQuery.xml"))
            {
                dsReport = new DataSet();
                dsReport.ReadXml("ReportQuery.xml");
                dsSQL.Merge(dsReport);
            }
        }

        public void OpenConnection()
        {
            dbEngine.OpenConnection();
        }

        public void CloseConnection()
        {
            dbEngine.CloseConnection();
        }

        public int Insert(T obj, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;

            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            ProcessParameter(obj, request);

            return dbEngine.Insert(request);
        }


        /// <summary>
        /// This method quest the logging action.
        /// </summary>
        /// <paramm name="action"></paramm>
        /// <paramm name="done"></paramm>
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
        }

        /// <summary>
        /// Log audit data
        /// </summary>
        /// <paramm name="obj"></paramm>
        public void CreateAuditTrail(T obj, string loginId, string brnCd, string ipAddress)
        {
            try
            { //CR-201
              // DataRow queryRow = dsSQL.Tables["Query"].Select("Id = 'InsertAuditTrail'")[0];
                DataRow queryRow = dsSQL.Tables["Query"].Select("Id = 'InsertAuditData'")[0];
                string commandText = queryRow["CommandText"].ToString();
                string commandType = queryRow["CommandType"].ToString();

                Request request = new Request();
                request.CommandText = commandText;
                request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

                if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(brnCd))
                {
                    HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                    if (authCookie != null)
                    {
                        var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                        var userData = ticket.UserData;
                        loginId = ticket.Name;

                        try
                        {
                            brnCd = userData.Split('|')[0];
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                var auditTrailService = new AuditTrailService();
                AuditTrail log = auditTrailService.GetAuditLog(obj as BaseObject, loginId, brnCd, ipAddress);

                if (null != log)
                {
                    ProcessParameter(log, ref request);
                    dbEngine.Update(request);
                }
            }
            catch (Exception ex)
            {  // Log any exceptions here. 
            }
        }

        //RP: For audit log
        private void ProcessParameter(AuditTrail instance, ref Request request)
        {
            string fieldName = string.Empty;
            Type targetType = instance.GetType();

            PropertyInfo[] pinfo = targetType.GetProperties();
            foreach (PropertyInfo pi in pinfo)
            {
                fieldName = GetColumnName(pi);
                if (!String.IsNullOrEmpty(fieldName))
                {
                    if (pi.GetValue(instance, null) != null)
                        request.Parameters.Add(new OracleParameter("v_" + fieldName, instance.GetType().GetProperty(pi.Name).GetValue(instance, null)));
                    else
                        request.Parameters.Add(new OracleParameter("v_" + fieldName, DBNull.Value));
                }
            }
        }

        private void ProcessParameter(T instance, Request request)
        {
            string fieldName = string.Empty;
            Type targetType = instance.GetType();

            PropertyInfo[] pinfo = targetType.GetProperties();
            foreach (PropertyInfo pi in pinfo)
            {
                fieldName = GetColumnName(pi);
                if (!String.IsNullOrEmpty(fieldName))
                {
                    if (pi.GetValue(instance, null) != null)
                        request.Parameters.Add(new OracleParameter("v_" + fieldName, instance.GetType().GetProperty(pi.Name).GetValue(instance, null)));
                    else
                        request.Parameters.Add(new OracleParameter("v_" + fieldName, DBNull.Value));
                }
            }
        }

        private string GetColumnName(PropertyInfo propInfo)
        {
            string colName = string.Empty;
            object[] attributes = propInfo.GetCustomAttributes(typeof(ColumnAttribute), true);

            if (attributes.GetLength(0) > 0)
            {
                colName = (attributes[0] as ColumnAttribute).Column;
                if (colName == null || colName.Length == 0)
                    colName = propInfo.Name;
            }

            return colName;
        }
    }
}