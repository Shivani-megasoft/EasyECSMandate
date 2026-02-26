using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using EasyBank.Infrastructure.ApplicationSettings;
using System.Windows.Forms;
using EasyBank.Framework.EasyExceptions;
using System.Security.Cryptography;
using Oracle.DataAccess.Client;
using EasyBank.Framework.EasyLogging;
using EasyBank.Framework.EasyExtensions;
using System.Diagnostics;

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

    public class AuditDBEngine
    {
        private static readonly string dataProvider = ConfigurationManager.AppSettings.Get("DataProvider");
        private static readonly OracleClientFactory factory = new OracleClientFactory();
        private string connectionString = DecryptConnectionString(ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString);
        private OracleConnection connection;

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
        public void OpenConnection()
        {
            connection = new OracleConnection();
            connection.ConnectionString = connectionString;

            try
            {
                connection.Open();
            }
            catch
            {
                string applType = ConfigurationManager.AppSettings.Get("ApplType");
                if (applType == "Web")
                {
                    throw new DBConnectException("Unable to connect Database.");
                }

                if (MessageBox.Show("The Main Server is not available, Connect to Alternate ?", "DB Connection", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    connectionString = ConfigurationManager.ConnectionStrings["Alternate.Server"].ConnectionString;
                    connection.ConnectionString = connectionString;
                    connection.Open();
                }
            }

            AppSettings.SERVER = connection.DataSource;
            AppSettings.DATABASE = connection.OracleLogicalTransaction.UserId;//connection.Database;
        }

        public void CloseConnection()
        {
            connection.Close();
            connection.Dispose();
        }

        public int Insert(Request request)
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
            }
            finally
            {
                CloseConnection();
            }

            return ret;
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
            using (OracleCommand command = new OracleCommand())
            {
                command.Connection = connection;
                command.CommandText = sql;
                command.ExecuteNonQuery();

                int id = -1;

                if (getId)
                {
                    command.CommandText = "SELECT MySequence.NEXTVAL FROM DUAL";
                    id = int.Parse(command.ExecuteScalar().ToString());
                }

                return id;
            }
        }

        public void Insert(string sql)
        {
            Insert(sql, false);
        }

        public DataSet GetDataSet(Request request)
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
                        command.Parameters.Add(p1);
                    }
                }

                command.CommandText = request.CommandText;
                command.CommandType = request.CommandType;
                command.CommandTimeout = 0;
                command.BindByName = true;

                using (DbDataAdapter adapter = factory.CreateDataAdapter())
                {
                    adapter.SelectCommand = command;

                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    return ds;
                }
            }
        }

        public DataSet GetDataSet(string sql)
        {
            if (null == connection || connection.State != ConnectionState.Open)
                OpenConnection();

            using (OracleCommand command = new OracleCommand())
            {
                command.Connection = connection;
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 0;

                using (DbDataAdapter adapter = factory.CreateDataAdapter())
                {
                    adapter.SelectCommand = command;

                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    return ds;
                }
            }
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