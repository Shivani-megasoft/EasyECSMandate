using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Collections.Generic;
using EasyBank.Framework.EasyAttributes;
using System.Threading;
using System.Web;
using EasyBank.Framework.EasyCaching;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Collections;
using System.Diagnostics;
using EasyBank.Framework.EasySession;
using Oracle.DataAccess.Client;

namespace EasyBank.DataLayer.DataObjects
{
    public class SmartData<T>
    {
        DataSet dsSQL, dsReport;
        DBEngine dbEngine;
        ConstructorInfo ctor;
        ObjectActivator<T> createdActivator;
        static readonly Hashtable types = Hashtable.Synchronized(new Hashtable());

        public SmartData()
        {
            dbEngine = new DBEngine();
            dsSQL = new DataSet();

            string applType = ConfigurationManager.AppSettings.Get("ApplType");
            if (applType == "Web")
            {
                dsSQL = CacheManager.Cache["Sql-Report-QueryXml"] as DataSet;
                if (null == dsSQL)
                {
                    dsSQL = new DataSet();
                    dsSQL.ReadXml(HttpContext.Current.Server.MapPath("~/Queries/SqlQuery.xml"));

                    dsReport = new DataSet();
                    dsReport.ReadXml(HttpContext.Current.Server.MapPath("~/Queries/ReportQuery.xml"));
                    dsSQL.Merge(dsReport);

                    CacheManager.Cache.Add("Sql-Report-QueryXml", dsSQL);
                }
            }
            else
            {
                dsSQL.ReadXml("SqlQuery.xml");
                if (File.Exists("ReportQuery.xml"))
                {
                    dsReport = new DataSet();
                    dsReport.ReadXml("ReportQuery.xml");
                    dsSQL.Merge(dsReport);
                }
            }

            if (types.ContainsKey(typeof(T)))
            {
                createdActivator = types[typeof(T)] as ObjectActivator<T>;
            }
            else
            {
                ctor = typeof(T).GetConstructors()[1];
                createdActivator = InstanceManager.GetActivator<T>(ctor);
                types.Add(typeof(T), createdActivator);
            }

        }

        public SmartData(string conString) : this()
        {
            dbEngine = new DBEngine(conString);
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

        public int Insert(T obj, string spName, OracleConnection conn)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;

            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            ProcessParameter(obj, request);

            return dbEngine.Insert(request, conn);
        }

        public int Insert(T obj, string spName, OracleConnection conn, OracleTransaction tx)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;

            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            ProcessParameter(obj, request);

            return dbEngine.Insert(request, conn, tx);
        }

        public int Update(T obj, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            ProcessParameter(obj, request);

            int ret = dbEngine.Update(request);

            string loginId = string.Empty;
            string brnCd = string.Empty;
            string ipAddress = string.Empty;
            var httpContext = HttpContext.Current;
            try
            {
                ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                loginId = SessionManager<string>.Get("LOGIN_ID");
                brnCd = SessionManager<string>.Get("BRN_CD");
            }
            catch (Exception ex)
            { }

            //[CR29]: Changes done for audit trail.
            SmartAuditData<T> smartAudit = new SmartAuditData<T>();
            try
            {
                smartAudit.Queue(() =>
                {
                    HttpContext.Current = httpContext;
                    smartAudit.CreateAuditTrail(obj, loginId, brnCd, ipAddress);
                }, null);
            }
            catch (Exception ex) { }

            return ret;
        }

        public int Update(T obj, string spName, OracleConnection conn)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            ProcessParameter(obj, request);

            int ret = dbEngine.Update(request, conn);
            string loginId = string.Empty;
            string ipAddress = string.Empty;
            string brnCd = string.Empty;
            var httpContext = HttpContext.Current;

            try
            {
                ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                loginId = SessionManager<string>.Get("LOGIN_ID");
                brnCd = SessionManager<string>.Get("BRN_CD");
            }
            catch (Exception ex)
            { }

            SmartAuditData<T> smartAudit = new SmartAuditData<T>();

            try
            {
                smartAudit.Queue(() =>
                {
                    HttpContext.Current = httpContext;
                    smartAudit.CreateAuditTrail(obj, loginId, brnCd,ipAddress);
                }, null);
            }
            catch (Exception ex) { }

            return ret;
        }

        public int Update(T obj, string spName, OracleConnection conn, OracleTransaction tx)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            ProcessParameter(obj, request);

            int ret = dbEngine.Update(request, conn, tx);
            string loginId = string.Empty;
            string brnCd = string.Empty;

            string ipAddress = string.Empty;
            var httpContext = HttpContext.Current;

            try
            {
                ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                loginId = SessionManager<string>.Get("LOGIN_ID");
                brnCd = SessionManager<string>.Get("BRN_CD");
            }
            catch (Exception ex) { }

            SmartAuditData<T> smartAudit = new SmartAuditData<T>();
            try
            {
                smartAudit.Queue(() =>
                {
                    HttpContext.Current = httpContext;
                    smartAudit.CreateAuditTrail(obj, loginId, brnCd,ipAddress);
                }, null);
            }
            catch (Exception ex) { }

            return ret;
        }

        public int Update(List<string> stringList, List<DateTime> dateList, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            return dbEngine.UpdateWithParam(request);
        }

        public int Update(List<string> stringList, List<DateTime> dateList, string spName, OracleConnection conn)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            return dbEngine.UpdateWithParam(request);
        }

        public int Delete(string id, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Id", id));

            return dbEngine.Update(request);
        }

        public int Delete(string id, string spName, OracleConnection connection)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Id", id));

            return dbEngine.Update(request, connection);
        }

        public T Get(string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            DataRow row = dbEngine.GetDataRow(request);
            T bankObject = default(T);

            if (null != row)
            {
                bankObject = createdActivator(row);
            }

            return bankObject;
        }

        public T GetById(Int32 id, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];
            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Id", id));

            DataRow row = dbEngine.GetDataRow(request);
            T bankObject = default(T);

            if (null != row)
            {
                bankObject = createdActivator(row);
            }

            return bankObject;
        }

        public T GetById(string id, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];
            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Id", id));

            DataRow row = dbEngine.GetDataRow(request);
            T bankObject = default(T);

            if (null != row)
            {
                bankObject = createdActivator(row);
            }

            return bankObject;
        }

        public T GetByCode(string code, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Code", code));

            DataRow row = dbEngine.GetDataRow(request);
            T bankObject = default(T);

            if (null != row)
            {
                bankObject = createdActivator(row);
            }

            return bankObject;
        }

        public T GetByCriteria(DateTime criteria, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Criteria", criteria));

            DataRow row = dbEngine.GetDataRow(request);
            T bankObject = default(T);

            if (null != row)
            {
                bankObject = createdActivator(row);
            }

            return bankObject;
        }

        public T GetByCriteria(string criteria1, DateTime criteria2, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Criteria1", criteria1));
            request.Parameters.Add(new OracleParameter("v_Criteria2", criteria2));

            DataRow row = dbEngine.GetDataRow(request);
            T bankObject = default(T);

            if (null != row)
            {
                bankObject = createdActivator(row);
            }

            return bankObject;
        }

        public T GetByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            DataRow row = dbEngine.GetDataRow(request);
            T bankObject = default(T);

            if (null != row)
            {
                bankObject = createdActivator(row);
            }

            return bankObject;
        }

        public List<T> GetAll(string spName)
        {
            List<T> list = new List<T>();

            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            DataTable table = dbEngine.GetDataTable(request);
            T bankObject = default(T);

            foreach (DataRow row in table.Rows)
            {
                bankObject = createdActivator(row);
                list.Add(bankObject);
            }

            return list;
        }

        public List<T> GetAllByCriteria(string criteria, string spName)
        {
            List<T> list = new List<T>();

            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Criteria", criteria));

            DataTable table = dbEngine.GetDataTable(request);
            T bankObject = default(T);

            foreach (DataRow row in table.Rows)
            {
                bankObject = createdActivator(row);
                list.Add(bankObject);
            }

            return list;
        }

        public List<T> GetAllByCriteria(DateTime criteria, string spName)
        {
            List<T> list = new List<T>();

            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Criteria", criteria));

            DataTable table = dbEngine.GetDataTable(request);
            T bankObject = default(T);

            foreach (DataRow row in table.Rows)
            {
                bankObject = createdActivator(row);
                list.Add(bankObject);
            }

            return list;
        }

        public List<T> GetAllByCriteria(string criteria1, DateTime criteria2, string spName)
        {
            List<T> list = new List<T>();

            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            request.Parameters.Add(new OracleParameter("v_Criteria1", criteria1));
            request.Parameters.Add(new OracleParameter("v_Criteria2", criteria2));

            DataTable table = dbEngine.GetDataTable(request);
            T bankObject = default(T);

            foreach (DataRow row in table.Rows)
            {
                bankObject = createdActivator(row);
                list.Add(bankObject);
            }

            return list;
        }

        public List<T> GetAllByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
        {
            List<T> list = new List<T>();

            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            DataTable table = dbEngine.GetDataTable(request);
            T bankObject = default(T);

            foreach (DataRow row in table.Rows)
            {
                bankObject = createdActivator(row);
                list.Add(bankObject);
            }

            return list;
        }

        public List<T> GetAllByCriteria1(List<string> stringList, List<DateTime> dateList, string spName)
        {
            List<T> list = new List<T>();

            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                {
                    if (dt == DateTime.MinValue)
                        request.Parameters.Add(new OracleParameter("v_Criteria" + i++, DBNull.Value));
                    else
                        request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));
                }

            DataTable table = dbEngine.GetDataTable(request);
            T bankObject = default(T);

            foreach (DataRow row in table.Rows)
            {
                bankObject = createdActivator(row);
                list.Add(bankObject);
            }

            return list;
        }


        public DataTable GetTable(string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            return dbEngine.GetDataTable(request);
        }

        public DataTable GetTable(string criteria1, DateTime? criteria2, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (criteria1 != null)
                request.Parameters.Add(new OracleParameter("v_Criteria" + i++, criteria1));

            if (criteria2 != null)
                request.Parameters.Add(new OracleParameter("v_Criteria" + i, criteria2));

            return dbEngine.GetDataTable(request);
        }

        public DataTable GetTable(List<string> stringList, List<DateTime> dateList, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            return dbEngine.GetDataTable(request);
        }

        public void AuditDataUpdate(T obj)
        {
            SmartAuditData<T> smartAudit = new SmartAuditData<T>();
            var httpContext = HttpContext.Current;
            string loginId = string.Empty;
            string ipAddress = string.Empty;
            string brnCd = string.Empty;

            try
            {
                ipAddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                loginId = SessionManager<string>.Get("LOGIN_ID");
                brnCd = SessionManager<string>.Get("BRN_CD");
            }
            catch (Exception ex) { }

            try
            {
                smartAudit.Queue(() =>
                {
                    HttpContext.Current = httpContext;
                    smartAudit.CreateAuditTrail(obj, loginId, brnCd,ipAddress);
                }, null);
            }
            catch (Exception ex) { }
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

        public T GetByDataSet(List<string> stringList, List<DateTime> dateList, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            T bankObject = default(T);
            DataSet ds = dbEngine.GetDataSet(request);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var key = typeof(T).Name + "-DS";

                if (types.ContainsKey(key))
                {
                    createdActivator = types[key] as ObjectActivator<T>;
                }
                else
                {
                    ctor = typeof(T).GetConstructors()[2];
                    createdActivator = InstanceManager.GetActivator<T>(ctor);
                    types.Add(key, createdActivator);
                }

                bankObject = createdActivator(ds);
            }

            return bankObject;
        }

        public DataSet GetDataSet(List<string> stringList, List<DateTime> dateList, string spName)
        {
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            return dbEngine.GetDataSet(request);
        }

        public List<Dictionary<string, string>> GetDictionaryByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];
            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            Request request = new Request();
            request.CommandText = commandText;
            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

            int i = 1;
            if (stringList != null)
                foreach (string s in stringList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, s));

            if (dateList != null)
                foreach (DateTime dt in dateList)
                    request.Parameters.Add(new OracleParameter("v_Criteria" + i++, dt));

            DataSet ds = dbEngine.GetDataSet(request);
            if (ds.Tables[0].Rows.Count > 0)
            {
                list = ToDictionary(ds.Tables[0]);
            }

            return list;
        }

        private static List<Dictionary<string, string>> ToDictionary(DataTable table)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            var dict = new Dictionary<string, string>();

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    if (row[column] != DBNull.Value)
                        dict[column.ColumnName] = row[column].ToString();
                    else
                        dict[column.ColumnName] = "";
                }

                list.Add(dict);
            }

            return list;
        }
    }

    public static class TypeExtensions
    {
        public static object GetInstance(this Type type)
        {
            return GetInstance<TypeToIgnore>(type, null);
        }

        public static object GetInstance<TArg>(this Type type, TArg argument)
        {
            return GetInstance<TArg, TypeToIgnore>(type, argument, null);
        }

        public static object GetInstance<TArg1, TArg2>(this Type type, TArg1 argument1, TArg2 argument2)
        {
            return GetInstance<TArg1, TArg2, TypeToIgnore>(type, argument1, argument2, null);
        }

        public static object GetInstance<TArg1, TArg2, TArg3>(this Type type, TArg1 argument1, TArg2 argument2, TArg3 argument3)
        {
            return InstanceCreationFactory<TArg1, TArg2, TArg3>
                .CreateInstanceOf(type, argument1, argument2, argument3);
        }

        private class TypeToIgnore
        {

        }

        private static class InstanceCreationFactory<TArg1, TArg2, TArg3>
        {
            private static readonly Dictionary<Type, Func<TArg1, TArg2, TArg3, object>> _instanceCreationMethods =
                new Dictionary<Type, Func<TArg1, TArg2, TArg3, object>>();

            public static object CreateInstanceOf(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            {
                CacheInstanceCreationMethodIfRequired(type);

                return _instanceCreationMethods[type].Invoke(arg1, arg2, arg3);
            }

            private static void CacheInstanceCreationMethodIfRequired(Type type)
            {
                if (_instanceCreationMethods.ContainsKey(type))
                    return;

                var argumentTypes = new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };

                Type[] constructorArgumentTypes = argumentTypes.Where(t => t != typeof(TypeToIgnore)).ToArray();

                var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis,
                    constructorArgumentTypes, new ParameterModifier[0]);

                var lamdaparammeterExpressions = new[]
            {
                Expression.Parameter(typeof(TArg1), "param1"),
                Expression.Parameter(typeof(TArg2), "param2"),
                Expression.Parameter(typeof(TArg3), "param3")
            };

                var constructorparammeterExpressions = lamdaparammeterExpressions.Take(constructorArgumentTypes.Length).ToArray();

                // Get an Expression representing the constructor call, passing in the constructor parammeters:
                var constructorCallExpression = Expression.New(constructor, constructorparammeterExpressions);

                // Compile the Expression into a Func which takes three arguments and returns the constructed object:
                var constructorCallingLambda = Expression
                    .Lambda<Func<TArg1, TArg2, TArg3, object>>(constructorCallExpression, lamdaparammeterExpressions)
                    .Compile();

                _instanceCreationMethods[type] = constructorCallingLambda;
            }
        }
    }

    public delegate T ObjectActivator<T>(params object[] args);

    public static class InstanceManager
    {
        public static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] parammsInfo = ctor.GetParameters();

            ParameterExpression paramm =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[parammsInfo.Length];

            for (int i = 0; i < parammsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type parammType = parammsInfo[i].ParameterType;

                Expression parammAccessorExp =
                    Expression.ArrayIndex(paramm, index);

                Expression parammCastExp =
                    Expression.Convert(parammAccessorExp, parammType);

                argsExp[i] = parammCastExp;
            }

            NewExpression newExp = Expression.New(ctor, argsExp);

            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator<T>), newExp, paramm);

            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
            return compiled;
        }
    }
}