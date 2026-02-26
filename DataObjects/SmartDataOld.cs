//using System;
//using System.Configuration;
//using System.IO;
//using System.Data;
//using System.Data.Common;
//using System.Reflection;
//using System.Collections.Generic;
//using EasyBank.Framework.EasyAttributes;
//using System.Threading;
//using System.Web;
//using EasyBank.Framework.EasyCaching;
//using System.Data.SqlClient;

//namespace EasyBank.DataLayer.DataObjects
//{
//    public class SmartData<T>
//    {
//        DataSet dsSQL, dsReport;
//        //changed bu UP - removed static
//        //static DBEngine dbEngine = new DBEngine();
//        DBEngine dbEngine;

//        public SmartData()
//        {
//            //added by UP  ----------------
//            dbEngine = new DBEngine();
//            //--------------------------

//            dsSQL = new DataSet();
//            string applType = ConfigurationManager.AppSettings.Get("ApplType");
//            if (applType == "Web")
//            {
//                dsSQL = CacheManager.Cache["Sql-Report-QueryXml"] as DataSet;
//                if (null == dsSQL)
//                {
//                    dsSQL = new DataSet();
//                    dsSQL.ReadXml(HttpContext.Current.Server.MapPath("~/Queries/SqlQuery.xml"));

//                    dsReport = new DataSet();
//                    dsReport.ReadXml(HttpContext.Current.Server.MapPath("~/Queries/ReportQuery.xml"));
//                    dsSQL.Merge(dsReport);

//                    CacheManager.Cache.Add("Sql-Report-QueryXml", dsSQL);
//                }
//            }
//            else
//            {
//                dsSQL.ReadXml("SqlQuery.xml");
//                if (File.Exists("ReportQuery.xml"))
//                {
//                    dsReport = new DataSet();
//                    dsReport.ReadXml("ReportQuery.xml");
//                    dsSQL.Merge(dsReport);
//                }
//            }
//        }

//        //[CR366]:Move Signature in DataBase at customer level --start
//        public SmartData(string conString)
//            : this()
//        {
//            dbEngine = new DBEngine(conString);
//        }

//        //Changed by UP --------------------------
//        //public static void OpenConnection()
//        //{
//        //    dbEngine.OpenConnection();
//        //}

//        //public static void CloseConnection()
//        //{
//        //    dbEngine.CloseConnection();
//        //}

//        //public void OpenConnection()
//        //{
//        //    dbEngine.OpenConnection();
//        //}

//        //public void CloseConnection()
//        //{
//        //    dbEngine.CloseConnection();
//        //}
//        //-----------------------------------

//        public int Insert(T obj, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;

//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            ProcessParameter(obj, request);

//            return dbEngine.Insert(request);
//        }

//        public int Insert(T obj, string spName, DbConnection conn)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;

//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            ProcessParameter(obj, request);

//            return dbEngine.Insert(request, conn);
//        }

//        public int Update(T obj, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            ProcessParameter(obj, request);

//            int ret = dbEngine.Update(request);

//            //[CR29]: Changes done for audit trail.
//            SmartAuditData<T> smartAudit = new SmartAuditData<T>();
//            try
//            {
//                smartAudit.Queue(() =>
//                {
//                    smartAudit.CreateAuditTrail(obj);
//                }, null);
//            }
//            catch { }

//            return ret;
//        }

//        public int Update(T obj, string spName, DbConnection conn)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            ProcessParameter(obj, request);

//            int ret = dbEngine.Update(request, conn);

//            //[CR29]: Changes done for audit trail.
//            SmartAuditData<T> smartAudit = new SmartAuditData<T>();
//            try
//            {
//                smartAudit.Queue(() =>
//                {
//                    smartAudit.CreateAuditTrail(obj);
//                }, null);
//            }
//            catch { }

//            return ret;
//        }

//        //[CR114]: Update with parameters
//        public int Update(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

//            return dbEngine.UpdateWithParam(request);
//        }

//        public int Delete(string id, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Id", id));

//            return dbEngine.Update(request);
//        }

//        public T Get(string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            DataRow row = dbEngine.GetDataRow(request);
//            T bankObject = default(T);

//            if (null != row)
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);

//            return bankObject;
//        }

//        public T GetById(string id, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];
//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Id", id));

//            DataRow row = dbEngine.GetDataRow(request);
//            T bankObject = default(T);

//            if (null != row)
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);

//            return bankObject;
//        }

//        public T GetByCode(string code, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Code", code));

//            DataRow row = dbEngine.GetDataRow(request);
//            T bankObject = default(T);

//            if (null != row)
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);

//            return bankObject;
//        }

//        public T GetByCriteria(DateTime criteria, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Criteria", criteria));

//            DataRow row = dbEngine.GetDataRow(request);
//            T bankObject = default(T);

//            if (null != row)
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);

//            return bankObject;
//        }

//        public T GetByCriteria(string criteria1, DateTime criteria2, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Criteria1", criteria1));
//            request.Parameters.Add(new Parameter("@Criteria2", criteria2));

//            DataRow row = dbEngine.GetDataRow(request);
//            T bankObject = default(T);

//            if (null != row)
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);

//            return bankObject;
//        }

//        public T GetByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

//            DataRow row = dbEngine.GetDataRow(request);
//            T bankObject = default(T);

//            if (null != row)
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);

//            return bankObject;
//        }

//        public List<T> GetAll(string spName)
//        {
//            List<T> list = new List<T>();

//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            DataTable table = dbEngine.GetDataTable(request);
//            T bankObject = default(T);

//            foreach (DataRow row in table.Rows)
//            {
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);
//                list.Add(bankObject);
//            }

//            return list;
//        }

//        public List<T> GetAllByCriteria(string criteria, string spName)
//        {
//            List<T> list = new List<T>();

//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Criteria", criteria));

//            DataTable table = dbEngine.GetDataTable(request);
//            T bankObject = default(T);

//            foreach (DataRow row in table.Rows)
//            {
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);
//                list.Add(bankObject);
//            }

//            return list;
//        }

//        public List<T> GetAllByCriteria(DateTime criteria, string spName)
//        {
//            List<T> list = new List<T>();

//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Criteria", criteria));

//            DataTable table = dbEngine.GetDataTable(request);
//            T bankObject = default(T);

//            foreach (DataRow row in table.Rows)
//            {
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);
//                list.Add(bankObject);
//            }

//            return list;
//        }

//        public List<T> GetAllByCriteria(string criteria1, DateTime criteria2, string spName)
//        {
//            List<T> list = new List<T>();

//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
//            request.Parameters.Add(new Parameter("@Criteria1", criteria1));
//            request.Parameters.Add(new Parameter("@Criteria2", criteria2));

//            DataTable table = dbEngine.GetDataTable(request);
//            T bankObject = default(T);

//            foreach (DataRow row in table.Rows)
//            {
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);
//                list.Add(bankObject);
//            }

//            return list;
//        }

//        public List<T> GetAllByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            List<T> list = new List<T>();

//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

//            DataTable table = dbEngine.GetDataTable(request);
//            T bankObject = default(T);

//            foreach (DataRow row in table.Rows)
//            {
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);
//                list.Add(bankObject);
//            }

//            return list;
//        }

//        public DataTable GetTable(string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            return dbEngine.GetDataTable(request);
//        }

//        public DataTable GetTable(string criteria1, DateTime? criteria2, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (criteria1 != null)
//                request.Parameters.Add(new Parameter("@Criteria" + i++, criteria1));

//            if (criteria2 != null)
//                request.Parameters.Add(new Parameter("@Criteria" + i, criteria2));

//            return dbEngine.GetDataTable(request);
//        }

//        public List<T> GetAllByCriteria1(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            List<T> list = new List<T>();

//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                {
//                    if (dt == DateTime.MinValue)
//                    {
//                        request.Parameters.Add(new Parameter("@Criteria" + i++, DBNull.Value));
//                    }
//                    else
//                    {
//                        request.Parameters.Add(new Parameter("@Criteria" + i++, dt));
//                    }
//                }

//            DataTable table = dbEngine.GetDataTable(request);
//            T bankObject = default(T);

//            foreach (DataRow row in table.Rows)
//            {
//                bankObject = (T)Activator.CreateInstance(typeof(T), row);                
//                list.Add(bankObject);
//            }

//            return list;
//        }

//        public DataTable GetTable(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

//            return dbEngine.GetDataTable(request);
//        }

//        private void ProcessParameter(T instance, Request request)
//        {
//            string fieldName = string.Empty;
//            Type targetType = instance.GetType();

//            PropertyInfo[] pinfo = targetType.GetProperties();
//            foreach (PropertyInfo pi in pinfo)
//            {
//                fieldName = GetColumnName(pi);
//                if (!String.IsNullOrEmpty(fieldName))
//                {
//                    if (pi.GetValue(instance, null) != null)
//                    {
//                        if (pi.PropertyType.FullName.ToLower() == "system.byte[]")
//                        {
//                            request.Parameters.Add(new Parameter("@" + fieldName, instance.GetType().GetProperty(pi.Name).GetValue(instance, null), pi.PropertyType.ToString()));
//                        }
//                        else
//                        {
//                            request.Parameters.Add(new Parameter("@" + fieldName, instance.GetType().GetProperty(pi.Name).GetValue(instance, null).ToString(), pi.PropertyType.ToString()));
//                        }
//                    }
//                    else
//                        request.Parameters.Add(new Parameter("@" + fieldName, DBNull.Value));
//                }
//            }
//        }

//        private string GetColumnName(PropertyInfo propInfo)
//        {
//            string colName = string.Empty;
//            object[] attributes = propInfo.GetCustomAttributes(typeof(ColumnAttribute), true);

//            if (attributes.GetLength(0) > 0)
//            {
//                colName = (attributes[0] as ColumnAttribute).Column;
//                if (colName == null || colName.Length == 0)
//                    colName = propInfo.Name;
//            }

//            return colName;
//        }

//        //[CR152]: GL Code + AcNo Performance improvement
//        public T GetByDataSet(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));


//            T bankObject = default(T);
//            DataSet ds = dbEngine.GetDataSet(request);
//            if (ds.Tables[0].Rows.Count > 0)
//            {
//                //[CR166]:Performance Improvement for Merge. 
//                bankObject = (T)Activator.CreateInstance(typeof(T), ds);
//            }

//            return bankObject;
//        }

//        //For report performance
//        public DataSet GetDataSet(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

//            return dbEngine.GetDataSet(request);
//        }

//        //[CR166]:Performance Improvement for Merge. 
//        // For selective fields
//        public List<Dictionary<string, string>> GetDictionaryByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
//        {
//            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
//            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];
//            string commandText = queryRow["CommandText"].ToString();
//            string commandType = queryRow["CommandType"].ToString();

//            Request request = new Request();
//            request.CommandText = commandText;
//            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

//            int i = 1;
//            if (stringList != null)
//                foreach (string s in stringList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

//            if (dateList != null)
//                foreach (DateTime dt in dateList)
//                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

//            DataSet ds = dbEngine.GetDataSet(request);
//            if (ds.Tables[0].Rows.Count > 0)
//            {
//                list = ToDictionary(ds.Tables[0]);
//            }
//            return list;
//        }

//        //[CR166]:Performance Improvement for Merge. 
//        /// <summary>
//        /// Converts DataTable to Dict<string, string>
//        /// </summary>
//        /// <param name="table"></param>
//        /// <returns></returns>
//        private static List<Dictionary<string, string>> ToDictionary(DataTable table)
//        {
//            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
//            var dict = new Dictionary<string, string>();

//            foreach (DataRow row in table.Rows)
//            {
//                foreach (DataColumn column in table.Columns)
//                {
//                    if (row[column] != DBNull.Value)
//                    {
//                        dict[column.ColumnName] = row[column].ToString();
//                    }
//                    else
//                    {
//                        dict[column.ColumnName] = "";
//                    }
//                }
//                list.Add(dict);
//            }

//            return list;
//        }

//    }
//}





//#region Commented for activator

////using System;
////using System.Linq;
////using System.Configuration;
////using System.IO;
////using System.Data;
////using System.Data.Common;
////using System.Reflection;
////using System.Collections.Generic;
////using EasyBank.Framework.EasyAttributes;
////using System.Threading;
////using System.Web;
////using EasyBank.Framework.EasyCaching;
////using System.Data.SqlClient;
////using System.Linq.Expressions;
////using System.Collections;
////using System.Diagnostics;

////namespace EasyBank.DataLayer.DataObjects
////{
////    public class SmartData<T>
////    {
////        DataSet dsSQL, dsReport;
////        //changed bu UP - removed static
////        //static DBEngine dbEngine = new DBEngine();
////        DBEngine dbEngine;

////        //static Dictionary<Type, object> types = new Dictionary<Type, object>();
////        static readonly Hashtable types = Hashtable.Synchronized(new Hashtable());

////        ConstructorInfo ctor;
////        ObjectActivator<T> createdActivator;

////        public SmartData()
////        {
////            //added by UP  ----------------
////            dbEngine = new DBEngine();
////            //--------------------------

////            dsSQL = new DataSet();
////            string applType = ConfigurationManager.AppSettings.Get("ApplType");
////            if (applType == "Web")
////            {
////                dsSQL = CacheManager.Cache["Sql-Report-QueryXml"] as DataSet;
////                if (null == dsSQL)
////                {
////                    dsSQL = new DataSet();
////                    dsSQL.ReadXml(HttpContext.Current.Server.MapPath("~/Queries/SqlQuery.xml"));

////                    dsReport = new DataSet();
////                    dsReport.ReadXml(HttpContext.Current.Server.MapPath("~/Queries/ReportQuery.xml"));
////                    dsSQL.Merge(dsReport);

////                    CacheManager.Cache.Add("Sql-Report-QueryXml", dsSQL);
////                }
////            }
////            else
////            {
////                dsSQL.ReadXml("SqlQuery.xml");
////                if (File.Exists("ReportQuery.xml"))
////                {
////                    dsReport = new DataSet();
////                    dsReport.ReadXml("ReportQuery.xml");
////                    dsSQL.Merge(dsReport);
////                }
////            }

////            if (types.ContainsKey(typeof(T)))
////            {
////                createdActivator = types[typeof(T)] as ObjectActivator<T>;
////            }
////            else
////            {
////                ctor = typeof(T).GetConstructors()[1];
////                createdActivator = InstanceManager.GetActivator<T>(ctor);
////                types.Add(typeof(T), createdActivator);
////            }
            
////        }

////        //[CR366]:Move Signature in DataBase at customer level --start
////        public SmartData(string conString)
////            : this()
////        {
////            dbEngine = new DBEngine(conString);
////        }

////        //Changed by UP --------------------------
////        //public static void OpenConnection()
////        //{
////        //    dbEngine.OpenConnection();
////        //}

////        //public static void CloseConnection()
////        //{
////        //    dbEngine.CloseConnection();
////        //}

////        //public void OpenConnection()
////        //{
////        //    dbEngine.OpenConnection();
////        //}

////        //public void CloseConnection()
////        //{
////        //    dbEngine.CloseConnection();
////        //}
////        //-----------------------------------

////        public int Insert(T obj, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;

////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            ProcessParameter(obj, request);

////            return dbEngine.Insert(request);
////        }

////        public int Insert(T obj, string spName, DbConnection conn)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;

////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            ProcessParameter(obj, request);

////            return dbEngine.Insert(request, conn);
////        }

////        public int Update(T obj, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            ProcessParameter(obj, request);

////            int ret= dbEngine.Update(request);

////            //[CR29]: Changes done for audit trail.
////            SmartAuditData<T> smartAudit = new SmartAuditData<T>();
////            try
////            {
////                smartAudit.Queue(() =>
////                {
////                    smartAudit.CreateAuditTrail(obj);
////                }, null);
////            }
////            catch { }
            
////            return ret;
////        }

////        public int Update(T obj, string spName, DbConnection conn)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            ProcessParameter(obj, request);

////            int ret = dbEngine.Update(request, conn);

////            //[CR29]: Changes done for audit trail.
////            SmartAuditData<T> smartAudit = new SmartAuditData<T>();
////            try
////            {
////                smartAudit.Queue(() =>
////                {
////                    smartAudit.CreateAuditTrail(obj);
////                }, null);
////            }
////            catch { }

////            return ret;
////        }

////        //[CR114]: Update with parameters
////        public int Update(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

////            return dbEngine.UpdateWithParam(request);
////        }

////        public int Delete(string id, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Id", id));

////            return dbEngine.Update(request);
////        }

////        public T Get(string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            DataRow row = dbEngine.GetDataRow(request);
////            T bankObject = default(T);

////            if (null != row)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////            }

////            return bankObject;
////        }

////        public T GetById(string id, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];
////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Id", id));

////            DataRow row = dbEngine.GetDataRow(request);
////            T bankObject = default(T);

////            if (null != row)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////            }

////            return bankObject;
////        }

////        public T GetByCode(string code, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Code", code));

////            DataRow row = dbEngine.GetDataRow(request);
////            T bankObject = default(T);

////            if (null != row)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////            }

////            return bankObject;
////        }

////        public T GetByCriteria(DateTime criteria, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Criteria", criteria));

////            DataRow row = dbEngine.GetDataRow(request);
////            T bankObject = default(T);

////            if (null != row)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////            }

////            return bankObject;
////        }

////        public T GetByCriteria(string criteria1, DateTime criteria2, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Criteria1", criteria1));
////            request.Parameters.Add(new Parameter("@Criteria2", criteria2));

////            DataRow row = dbEngine.GetDataRow(request);
////            T bankObject = default(T);

////            if (null != row)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////            }

////            return bankObject;
////        }

////        public T GetByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

////            DataRow row = dbEngine.GetDataRow(request);
////            T bankObject = default(T);
           
////            if (null != row)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////            }

////            return bankObject;
////        }

////        public List<T> GetAll(string spName)
////        {
////            List<T> list = new List<T>();

////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            DataTable table = dbEngine.GetDataTable(request);
////            T bankObject = default(T);

////            foreach (DataRow row in table.Rows)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);                
////                bankObject = createdActivator(row);
////                list.Add(bankObject);
////            }

////            return list;
////        }

////        public List<T> GetAllByCriteria(string criteria, string spName)
////        {
////            List<T> list = new List<T>();

////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Criteria", criteria));

////            DataTable table = dbEngine.GetDataTable(request);
////            T bankObject = default(T);
           
////            //var watch = Stopwatch.StartNew();
////            foreach (DataRow row in table.Rows)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////                list.Add(bankObject);
////            }
////            //watch.Stop();
////            //long timeTaken = watch.ElapsedMilliseconds;

////            //var watch2 = Stopwatch.StartNew();
////            //foreach (DataRow row in table.Rows)
////            //{
////            //    bankObject = (T)Activator.CreateInstance(typeof(T), row);
////            //    //bankObject = createdActivator(row);
////            //    list.Add(bankObject);
////            //}
////            //watch2.Stop();
////            //long timeTaken2 = watch2.ElapsedMilliseconds;

////            //var watch3 = Stopwatch.StartNew();
////            //foreach (DataRow row in table.Rows)
////            //{
////            //    bankObject = (T)typeof(T).GetInstance<DataRow>(row);
////            //    //bankObject = createdActivator(row);
////            //    list.Add(bankObject);
////            //}
////            //watch3.Stop();
////            //long timeTaken3 = watch3.ElapsedMilliseconds;

////            return list;
////        }     

////        public List<T> GetAllByCriteria(DateTime criteria, string spName)
////        {
////            List<T> list = new List<T>();

////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Criteria", criteria));

////            DataTable table = dbEngine.GetDataTable(request);
////            T bankObject = default(T);

////            foreach (DataRow row in table.Rows)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////                list.Add(bankObject);
////            }

////            return list;
////        }

////        public List<T> GetAllByCriteria(string criteria1, DateTime criteria2, string spName)
////        {
////            List<T> list = new List<T>();

////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
////            request.Parameters.Add(new Parameter("@Criteria1", criteria1));
////            request.Parameters.Add(new Parameter("@Criteria2", criteria2));

////            DataTable table = dbEngine.GetDataTable(request);
////            T bankObject = default(T);

////            foreach (DataRow row in table.Rows)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////                list.Add(bankObject);
////            }

////            return list;
////        }

////        public List<T> GetAllByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            List<T> list = new List<T>();

////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

////            DataTable table = dbEngine.GetDataTable(request);
////            T bankObject = default(T);
           
////            foreach (DataRow row in table.Rows)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////                list.Add(bankObject);
////            }
           
////            return list;
////        }

////        public List<T> GetAllByCriteria1(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            List<T> list = new List<T>();

////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                {
////                    if (dt == DateTime.MinValue)
////                    {
////                        request.Parameters.Add(new Parameter("@Criteria" + i++, DBNull.Value));
////                    }
////                    else
////                    {
////                        request.Parameters.Add(new Parameter("@Criteria" + i++, dt));
////                    }
////                }

////            DataTable table = dbEngine.GetDataTable(request);
////            T bankObject = default(T);

////            foreach (DataRow row in table.Rows)
////            {
////                //bankObject = (T)Activator.CreateInstance(typeof(T), row);
////                bankObject = createdActivator(row);
////                list.Add(bankObject);
////            }

////            return list;
////        }


////        public DataTable GetTable(string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            return dbEngine.GetDataTable(request);
////        }

////        public DataTable GetTable(string criteria1, DateTime? criteria2, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (criteria1 != null)
////                request.Parameters.Add(new Parameter("@Criteria" + i++, criteria1));

////            if (criteria2 != null)
////                request.Parameters.Add(new Parameter("@Criteria" + i, criteria2));

////            return dbEngine.GetDataTable(request);
////        }

////        public DataTable GetTable(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

////            return dbEngine.GetDataTable(request);
////        }

////        private void ProcessParameter(T instance, Request request)
////        {
////            string fieldName = string.Empty;
////            Type targetType = instance.GetType();

////            PropertyInfo[] pinfo = targetType.GetProperties();
////            foreach (PropertyInfo pi in pinfo)
////            {
////                fieldName = GetColumnName(pi);
////                if (!String.IsNullOrEmpty(fieldName))
////                {
////                    if (pi.GetValue(instance, null) != null)
////                    {
////                        if (pi.PropertyType.FullName.ToLower() == "system.byte[]")
////                        {
////                            request.Parameters.Add(new Parameter("@" + fieldName, instance.GetType().GetProperty(pi.Name).GetValue(instance, null), pi.PropertyType.ToString()));
////                        }
////                        else
////                        {
////                            request.Parameters.Add(new Parameter("@" + fieldName, instance.GetType().GetProperty(pi.Name).GetValue(instance, null).ToString(), pi.PropertyType.ToString()));
////                        }
////                    }
////                    else
////                        request.Parameters.Add(new Parameter("@" + fieldName, DBNull.Value));
////                }
////            }
////        }

////        private string GetColumnName(PropertyInfo propInfo)
////        {
////            string colName = string.Empty;
////            object[] attributes = propInfo.GetCustomAttributes(typeof(ColumnAttribute), true);

////            if (attributes.GetLength(0) > 0)
////            {
////                colName = (attributes[0] as ColumnAttribute).Column;
////                if (colName == null || colName.Length == 0)
////                    colName = propInfo.Name;
////            }

////            return colName;
////        }

////        //[CR152]: GL Code + AcNo Performance improvement
////        public T GetByDataSet(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));


////            T bankObject = default(T);
////            DataSet ds = dbEngine.GetDataSet(request);
////            if (ds.Tables[0].Rows.Count > 0)
////            {
////                //[CR166]:Performance Improvement for Merge. 
////                //bankObject = (T)Activator.CreateInstance(typeof(T), ds);
////                //bankObject = createdActivator(ds);

////                var key = typeof(T).Name + "-DS";
////                if (types.ContainsKey(key))
////                {
////                    createdActivator = types[key] as ObjectActivator<T>;
////                }
////                else
////                {
////                    ctor = typeof(T).GetConstructors()[2];
////                    createdActivator = InstanceManager.GetActivator<T>(ctor);
////                    types.Add(key, createdActivator);                    
////                }
////                bankObject = createdActivator(ds);
////            }

////            return bankObject;
////        }

////        //For report performance
////        public DataSet GetDataSet(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

////            return dbEngine.GetDataSet(request);
////        }

////        //[CR166]:Performance Improvement for Merge. 
////        // For selective fields
////        public List<Dictionary<string, string>> GetDictionaryByCriteria(List<string> stringList, List<DateTime> dateList, string spName)
////        {
////            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
////            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];
////            string commandText = queryRow["CommandText"].ToString();
////            string commandType = queryRow["CommandType"].ToString();

////            Request request = new Request();
////            request.CommandText = commandText;
////            request.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);

////            int i = 1;
////            if (stringList != null)
////                foreach (string s in stringList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, s));

////            if (dateList != null)
////                foreach (DateTime dt in dateList)
////                    request.Parameters.Add(new Parameter("@Criteria" + i++, dt));

////            DataSet ds = dbEngine.GetDataSet(request);
////            if (ds.Tables[0].Rows.Count > 0)
////            {
////                list = ToDictionary(ds.Tables[0]);
////            }
////            return list;
////        }

////        //[CR166]:Performance Improvement for Merge. 
////        /// <summary>
////        /// Converts DataTable to Dict<string, string>
////        /// </summary>
////        /// <param name="table"></param>
////        /// <returns></returns>
////        private static List<Dictionary<string, string>> ToDictionary(DataTable table)
////        {
////            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
////            var dict = new Dictionary<string, string>();

////            foreach (DataRow row in table.Rows)
////            {
////                foreach (DataColumn column in table.Columns)
////                {
////                    if (row[column] != DBNull.Value)
////                    {
////                        dict[column.ColumnName] = row[column].ToString();  
////                    }
////                    else
////                    {
////                        dict[column.ColumnName] = "";
////                    }
////                }
////                list.Add(dict);
////            }

////            return list;
////        }

////    }



////    public static class TypeExtensions
////    {
////        /// <summary>
////        /// Returns an instance of the <paramref name="type"/> on which the method is invoked.
////        /// </summary>
////        /// <param name="type">The type on which the method was invoked.</param>
////        /// <returns>An instance of the <paramref name="type"/>.</returns>
////        public static object GetInstance(this Type type)
////        {
////            return GetInstance<TypeToIgnore>(type, null);
////        }

////        /// <summary>
////        /// Returns an instance of the <paramref name="type"/> on which the method is invoked.
////        /// </summary>
////        /// <typeparam name="TArg">The type of the argument to pass to the constructor.</typeparam>
////        /// <param name="type">The type on which the method was invoked.</param>
////        /// <param name="argument">The argument to pass to the constructor.</param>
////        /// <returns>An instance of the given <paramref name="type"/>.</returns>
////        public static object GetInstance<TArg>(this Type type, TArg argument)
////        {
////            return GetInstance<TArg, TypeToIgnore>(type, argument, null);
////        }

////        /// <summary>
////        /// Returns an instance of the <paramref name="type"/> on which the method is invoked.
////        /// </summary>
////        /// <typeparam name="TArg1">The type of the first argument to pass to the constructor.</typeparam>
////        /// <typeparam name="TArg2">The type of the second argument to pass to the constructor.</typeparam>
////        /// <param name="type">The type on which the method was invoked.</param>
////        /// <param name="argument1">The first argument to pass to the constructor.</param>
////        /// <param name="argument2">The second argument to pass to the constructor.</param>
////        /// <returns>An instance of the given <paramref name="type"/>.</returns>
////        public static object GetInstance<TArg1, TArg2>(this Type type, TArg1 argument1, TArg2 argument2)
////        {
////            return GetInstance<TArg1, TArg2, TypeToIgnore>(type, argument1, argument2, null);
////        }

////        /// <summary>
////        /// Returns an instance of the <paramref name="type"/> on which the method is invoked.
////        /// </summary>
////        /// <typeparam name="TArg1">The type of the first argument to pass to the constructor.</typeparam>
////        /// <typeparam name="TArg2">The type of the second argument to pass to the constructor.</typeparam>
////        /// <typeparam name="TArg3">The type of the third argument to pass to the constructor.</typeparam>
////        /// <param name="type">The type on which the method was invoked.</param>
////        /// <param name="argument1">The first argument to pass to the constructor.</param>
////        /// <param name="argument2">The second argument to pass to the constructor.</param>
////        /// <param name="argument3">The third argument to pass to the constructor.</param>
////        /// <returns>An instance of the given <paramref name="type"/>.</returns>
////        public static object GetInstance<TArg1, TArg2, TArg3>(
////            this Type type,
////            TArg1 argument1,
////            TArg2 argument2,
////            TArg3 argument3)
////        {
////            return InstanceCreationFactory<TArg1, TArg2, TArg3>
////                .CreateInstanceOf(type, argument1, argument2, argument3);
////        }

////        // To allow for overloads with differing numbers of arguments, we flag arguments which should be 
////        // ignored by using this Type:
////        private class TypeToIgnore
////        {
////        }

////        private static class InstanceCreationFactory<TArg1, TArg2, TArg3>
////        {
////            // This dictionary will hold a cache of object-creation functions, keyed by the Type to create:
////            private static readonly Dictionary<Type, Func<TArg1, TArg2, TArg3, object>> _instanceCreationMethods =
////                new Dictionary<Type, Func<TArg1, TArg2, TArg3, object>>();

////            public static object CreateInstanceOf(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
////            {
////                CacheInstanceCreationMethodIfRequired(type);

////                return _instanceCreationMethods[type].Invoke(arg1, arg2, arg3);
////            }

////            private static void CacheInstanceCreationMethodIfRequired(Type type)
////            {
////                // Bail out if we've already cached the instance creation method:
////                if (_instanceCreationMethods.ContainsKey(type))
////                {
////                    return;
////                }

////                var argumentTypes = new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };

////                // Get a collection of the constructor argument Types we've been given; ignore any 
////                // arguments which are of the 'ignore this' Type:
////                Type[] constructorArgumentTypes = argumentTypes.Where(t => t != typeof(TypeToIgnore)).ToArray();

////                // Get the Constructor which matches the given argument Types:
////                var constructor = type.GetConstructor(
////                    BindingFlags.Instance | BindingFlags.Public,
////                    null,
////                    CallingConventions.HasThis,
////                    constructorArgumentTypes,
////                    new ParameterModifier[0]);

////                // Get a set of Expressions representing the parameters which will be passed to the Func:
////                var lamdaParameterExpressions = new[]
////            {
////                Expression.Parameter(typeof(TArg1), "param1"),
////                Expression.Parameter(typeof(TArg2), "param2"),
////                Expression.Parameter(typeof(TArg3), "param3")
////            };

////                // Get a set of Expressions representing the parameters which will be passed to the constructor:
////                var constructorParameterExpressions = lamdaParameterExpressions
////                    .Take(constructorArgumentTypes.Length)
////                    .ToArray();

////                // Get an Expression representing the constructor call, passing in the constructor parameters:
////                var constructorCallExpression = Expression.New(constructor, constructorParameterExpressions);

////                // Compile the Expression into a Func which takes three arguments and returns the constructed object:
////                var constructorCallingLambda = Expression
////                    .Lambda<Func<TArg1, TArg2, TArg3, object>>(constructorCallExpression, lamdaParameterExpressions)
////                    .Compile();

////                _instanceCreationMethods[type] = constructorCallingLambda;
////            }
////        }
////    }

////    public delegate T ObjectActivator<T>(params object[] args);

////    //added to improve performance while creating the objects runtime
////    public static class InstanceManager
////    {
////        public static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
////        {
////            Type type = ctor.DeclaringType;
////            ParameterInfo[] paramsInfo = ctor.GetParameters();

////            //create a single param of type object[]
////            ParameterExpression param =
////                Expression.Parameter(typeof(object[]), "args");

////            Expression[] argsExp =
////                new Expression[paramsInfo.Length];

////            //pick each arg from the params array 
////            //and create a typed expression of them
////            for (int i = 0; i < paramsInfo.Length; i++)
////            {
////                Expression index = Expression.Constant(i);
////                Type paramType = paramsInfo[i].ParameterType;

////                Expression paramAccessorExp =
////                    Expression.ArrayIndex(param, index);

////                Expression paramCastExp =
////                    Expression.Convert(paramAccessorExp, paramType);

////                argsExp[i] = paramCastExp;
////            }

////            //make a NewExpression that calls the
////            //ctor with the args we just created
////            NewExpression newExp = Expression.New(ctor, argsExp);

////            //create a lambda with the New
////            //Expression as body and our param object[] as arg
////            LambdaExpression lambda =
////                Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

////            //compile it
////            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
////            return compiled;
////        }
////    }
////}
//#endregion