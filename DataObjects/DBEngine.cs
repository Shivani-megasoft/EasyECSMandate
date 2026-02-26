using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using EasyBank.Infrastructure.ApplicationSettings;
using System.Windows.Forms;
using EasyBank.Framework.EasyLogging;
using EasyBank.Framework.EasyExceptions;
using EasyBank.Framework.EasyExtensions;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Security.Cryptography;
using Oracle.DataAccess.Client;

namespace EasyBank.DataLayer.DataObjects
{
    /// <summary>
    /// Manages all lower level ADO.NET data base access.
    /// The Factory Design pattern is used to create database specific instances
    /// of Connection objects, Command objects, etc.
    /// </summary>
    /// <remarks>
    /// It handles all the database access details and shields its complexity from its clients.
    /// </remarks>

    public class DBEngine
    {
        // Note: Static initializers are thread safe.
        // If this class had a static constructor then these initialized
        // would need to be initialized there.
        private static readonly string dataProvider = ConfigurationManager.AppSettings.Get("DataProvider");
        // private static readonly DbProviderFactory factory = DbProviderFactories.GetFactory(dataProvider);
        private static readonly OracleClientFactory factory = new OracleClientFactory();

        private string connectionString = DecryptConnectionString(ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString);
        //private string connectionString = ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString;

        private static string DecryptConnectionString(string path)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap() { ExeConfigFilename = path };
            string hash = "AAECAwQFBgcICQoLDA0ODw==";
            byte[] data = Convert.FromBase64String(path);

            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                byte[] keys = sha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                using (AesManaged tripleDesc = new AesManaged() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform transform = tripleDesc.CreateDecryptor();
                    byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                    return UTF8Encoding.UTF8.GetString(result);
                }
            }
        }

        //changed by UP
        private OracleConnection connection;

        public DBEngine()
        {

        }

        public DBEngine(string conString)
        {
            connectionString = conString;
        }

        public void OpenConnection()
        {
            connection = new OracleConnection();
            connection.ConnectionString = connectionString;

            try
            {
                connection.Open();
            }
            catch (OracleException ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);

                string applType = ConfigurationManager.AppSettings.Get("ApplType");
                if (applType == "Web")
                {
                    throw new DBConnectException("Unable to connect Database.");
                }
            }

            AppSettings.SERVER = connection.DataSource;
            AppSettings.DATABASE = connection.OracleLogicalTransaction.UserId;//  connection.Database;
        }

        public static OracleConnection CreateAndOpenConnection()
        {
            OracleConnection connection = new OracleConnection();
            connection.ConnectionString = DecryptConnectionString(ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString);
            connection.Open();

            AppSettings.SERVER = connection.DataSource;
            AppSettings.DATABASE = connection.OracleLogicalTransaction.UserId;//connection.Database;

            return connection;
        }

        public void CloseConnection()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        public int Insert(Request request)
        {
            int ret = 0;
            try
            {
                using (OracleCommand command = new OracleCommand())
                {
                    OracleGlobalization.GetClientInfo();
                    if (null == connection || connection.State != ConnectionState.Open)
                    {
                        OpenConnection();
                        command.Connection = connection;
                    }

                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                            {
                                p1.OracleDbType = OracleDbType.Date;
                                p1.Value = param.Value;
                            }
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                p1.OracleDbType = OracleDbType.Int16;
                                if (p1.Value.ToString().ToLower() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;
                            }
                            else if (param.Value != null && param.Value.ToString().Length > 4000)
                            {
                                p1.OracleDbType = OracleDbType.Clob;
                                p1.Value = param.Value;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.Connection = connection;
                    command.BindByName = true;
                    ret = Convert.ToInt32(command.ExecuteScalar());
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ret;
        }

        public int Insert(Request request, OracleConnection conn)
        {
            int ret = 0;

            try
            {
                using (OracleCommand command = conn.CreateCommand())
                {
                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                            {
                                p1.OracleDbType = OracleDbType.Date;
                                p1.Value = param.Value;
                            }
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                p1.OracleDbType = OracleDbType.Int16;
                                if (p1.Value.ToString().ToLower() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;
                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.Connection = conn;
                    command.BindByName = true;

                    ret = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ret;
        }

        public int Insert(Request request, OracleConnection conn, OracleTransaction tx)
        {
            int id = 0;

            try
            {
                using (OracleCommand command = new OracleCommand())
                {
                    command.Transaction = tx;
                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                                p1.OracleDbType = OracleDbType.Date;
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                if (p1.Value.ToString().ToLower() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;

                                p1.OracleDbType = OracleDbType.Int16;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.BindByName = true;
                    command.Connection = conn;

                    id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return id;
        }

        public int Update(Request request)
        {
            int ret = 0;

            try
            {
                using (OracleCommand command = new OracleCommand())
                {
                    if (null == connection || connection.State != ConnectionState.Open)
                        OpenConnection();

                    command.Connection = connection;

                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                                p1.OracleDbType = OracleDbType.Date;
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                if (p1.Value.ToString().ToLower() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;

                                p1.OracleDbType = OracleDbType.Int16;
                            }
                            else if (param.Value != null && param.Value.ToString().Length > 4000)
                            {
                                p1.OracleDbType = OracleDbType.Clob;
                                p1.Value = param.Value;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.Connection = connection;
                    command.BindByName = true;

                    ret = command.ExecuteNonQuery();
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ret;
        }

        public int Update(Request request, OracleConnection conn)
        {
            int ret = 0;

            try
            {
                using (OracleCommand command = new OracleCommand())
                {
                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                                p1.OracleDbType = OracleDbType.Date;
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                if (p1.Value.ToString().ToLower() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;

                                p1.OracleDbType = OracleDbType.Int16;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.Connection = conn;
                    command.BindByName = true;

                    ret = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ret;
        }

        public int Update(Request request, OracleConnection conn, OracleTransaction tx)
        {
            int ret = 0;

            try
            {
                using (OracleCommand command = conn.CreateCommand())
                {
                    command.Transaction = tx;

                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                                p1.OracleDbType = OracleDbType.Date;
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                if (p1.Value.ToString().ToLower() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;

                                p1.OracleDbType = OracleDbType.Int16;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.Connection = conn;
                    command.BindByName = true;

                    ret = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ret;
        }

        public int Update(string sql)
        {
            using (OracleCommand command = new OracleCommand())
            {
                command.Connection = connection;
                command.CommandText = sql;

                return command.ExecuteNonQuery();
            }
        }

        public int Insert(string sql, bool getId)
        {
            int id = -1;

            try
            {
                using (OracleCommand command = new OracleCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.ExecuteNonQuery();

                    if (getId)
                    {
                        command.CommandText = "SELECT MySequence.NEXTVAL FROM DUAL";
                        id = int.Parse(command.ExecuteScalar().ToString());
                    }

                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return id;
        }

        public void Insert(string sql)
        {
            Insert(sql, false);
        }

        public DataSet GetDataSet(Request request)
        {
            DataSet ds = new DataSet();

            try
            {
                if (null == connection || connection.State != ConnectionState.Open)
                    OpenConnection();

                using (OracleCommand command = new OracleCommand())
                {
                    command.Connection = connection;
                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                                p1.OracleDbType = OracleDbType.Date;
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                if (p1.Value.ToString().ToLower() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;

                                p1.OracleDbType = OracleDbType.Int16;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.BindByName = true;

                    using (OracleDataAdapter adapter = new OracleDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        adapter.Fill(ds);
                    }

                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ds;
        }

        public DataSet GetDataSet(string sql)
        {
            DataSet ds = new DataSet();

            try
            {
                if (null == connection || connection.State != ConnectionState.Open)
                    OpenConnection();

                using (OracleCommand command = new OracleCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 0;

                    using (OracleDataAdapter adapter = new OracleDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        adapter.Fill(ds);
                    }

                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();

            }

            return ds;
        }

        public DataTable GetDataTable(string sql)
        {
            return GetDataSet(sql).Tables[0];
        }

        public DataTable GetDataTable(Request request)
        {
            return GetDataSet(request).Tables[0];
        }

        public DataRow GetDataRow(string sql)
        {
            DataRow row = null;
            DataTable dt = GetDataTable(sql);

            if (dt.Rows.Count > 0)
            {
                row = dt.Rows[0];
            }

            return row;
        }

        public DataRow GetDataRow(Request request)
        {
            DataRow row = null;
            DataTable dt = GetDataTable(request);

            if (dt.Rows.Count > 0)
                row = dt.Rows[0];

            return row;
        }

        public object GetScalar(string sql)
        {
            if (null == connection || connection.State != ConnectionState.Open)
                OpenConnection();

            using (OracleCommand command = new OracleCommand())
            {
                command.Connection = connection;
                command.CommandText = sql;
                command.CommandTimeout = 0;

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public int UpdateWithParam(Request request)
        {
            int ret = 0;

            try
            {
                using (OracleCommand command = new OracleCommand())
                {
                    if (null == connection || connection.State != ConnectionState.Open)
                        OpenConnection();

                    command.Connection = connection;

                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                                p1.OracleDbType = OracleDbType.Date;
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                if (p1.Value.ToString() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;

                                p1.OracleDbType = OracleDbType.Int16;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.Connection = connection;
                    ret = command.ExecuteNonQuery();
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ret;
        }

        public int UpdateWithParam(Request request, OracleConnection conn, OracleTransaction tx)
        {
            int ret = 0;

            try
            {
                using (OracleCommand command = conn.CreateCommand())
                {

                    command.Transaction = tx;

                    if (request.Parameters.Count > 0)
                    {
                        OracleParameter p1;
                        foreach (OracleParameter param in request.Parameters)
                        {
                            p1 = new OracleParameter(param.ParameterName, param.Value);

                            if (param.OracleDbType.ToString() == "TimeStamp")
                                p1.OracleDbType = OracleDbType.Date;
                            else if (p1.OracleDbType.ToString() == "Boolean")
                            {
                                if (p1.Value.ToString() == "false")
                                    p1.Value = 0;
                                else
                                    p1.Value = 1;

                                p1.OracleDbType = OracleDbType.Int16;
                            }
                            else
                                p1.OracleDbType = param.OracleDbType;

                            command.Parameters.Add(p1);
                        }
                    }

                    command.CommandText = request.CommandText;
                    command.CommandType = request.CommandType;
                    command.CommandTimeout = 0;
                    command.BindByName = true;

                    ret = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }

            return ret;
        }

        public static string Escape(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
                return "'" + s.Trim().Replace("'", "''") + "'";
        }

        public static string Escape(string s, int maxLength)
        {
            if (String.IsNullOrEmpty(s))
                return "NULL";
            else
            {
                s = s.Trim();
                if (s.Length > maxLength) s = s.Substring(0, maxLength - 1);
                return "'" + s.Trim().Replace("'", "''") + "'";
            }
        }
    }
}