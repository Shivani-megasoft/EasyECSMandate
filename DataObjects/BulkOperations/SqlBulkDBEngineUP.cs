using System;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using EasyBank.Framework.EasyExceptions;
using System.Web;
using EasyBank.Framework.EasyCaching;
using EasyBank.Framework.EasyExtensions;
using EasyBank.Framework.EasyLogging;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EasyBank.DataLayer.DataObjects
{
    public class SqlBulkDBEngine
    {
        private static readonly string dataProvider = ConfigurationManager.AppSettings.Get("DataProvider");
        //private static string connectionString = ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString;
        private static string connectionString = DecryptConnectionString(ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString);
        private static int bulkCopyTimeout = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("BulkCopyTimeout")) ? 2000 : ConfigurationManager.AppSettings.Get("BulkCopyTimeout").ToInt();
        private static int bulkBatchSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("BulkBatchSize")) ? 3000 : ConfigurationManager.AppSettings.Get("BulkBatchSize").ToInt();
        private static int commandTimeout = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("CommandTimeout")) ? 300 : ConfigurationManager.AppSettings.Get("CommandTimeout").ToInt();

        DataSet dsSQL;

        public SqlBulkDBEngine()
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
        }
        private static string DecryptConnectionString(string path)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap() { ExeConfigFilename = path };
            string hash = "f0xle@rn";


            byte[] data = Convert.FromBase64String(path);

            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                using (TripleDESCryptoServiceProvider tripleDesc = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform transform = tripleDesc.CreateDecryptor();
                    byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                    return UTF8Encoding.UTF8.GetString(result);
                }
            }

        }
        public void Insert(DataTable sourceTable)
        {
            var sqlConnection = DecryptConnectionString(ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString);
          //  var sqlConnection = ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString;
            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();

                // Create the SqlBulkCopy object. 
                // Note that the column positions in the source DataTable 
                // match the column positions in the destination table so 
                // there is no need to map columns. 

                //new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null);   // Check this for more performance, but will lock the table.

                //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --start
                string correctColumnsOrder = GetColumns(sourceTable);
                if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection))
                {
                    // throw exception if table structure is invalid
                    string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
                    msg += "The correct order is [" + correctColumnsOrder + "]";

                    connection.Dispose();
                    throw new TableStructureException(msg);
                }
                //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --end
                //changed by UP - default
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    bulkCopy.DestinationTableName = sourceTable.TableName;
                    bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                    bulkCopy.BatchSize = bulkBatchSize;
                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(sourceTable);
                    }
                    catch (Exception ex)
                    {
                        // LOG the exceptions here.
                        EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);
                        //  throw;
                    }
                }
            }
        }

        public void Insert(DataTable sourceTable, SqlConnection connection, SqlTransaction tx)
        {

            // Create the SqlBulkCopy object. 
            // Note that the column positions in the source DataTable 
            // match the column positions in the destination table so 
            // there is no need to map columns. 

            //new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null);   // Check this for more performance, but will lock the table.

            //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --start
            string correctColumnsOrder = GetColumns(sourceTable);
            if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection, tx))
            {
                // throw exception if table structure is invalid
                string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
                msg += "The correct order is [" + correctColumnsOrder + "]";

                //connection.Dispose();
                throw new TableStructureException(msg);
            }
            //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --end
            //changed by UP - default
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tx))
            {
                bulkCopy.DestinationTableName = sourceTable.TableName;
                bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                bulkCopy.BatchSize = bulkBatchSize;
                try
                {
                    // Write from the source to the destination.
                    bulkCopy.WriteToServer(sourceTable);
                }
                catch (Exception ex)
                {
                    // LOG the exceptions here.
                    EasyLogger.Log(ex.GetDetails(), "General", 1, TraceEventType.Critical);

                    throw;
                }
            }
        }

        /// <summary>
        /// Batch update sourceTable
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="targetTable"></param>
        public void Update(DataTable sourceTable, string spName)
        {
            // First insert in the staging table, then bulk update
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create the SqlBulkCopy object. 
                // Note that the column positions in the source DataTable 
                // match the column positions in the destination table so 
                // there is no need to map columns. 

                //new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null);   // Check this for more performance, but will lock the table.

                //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --start
                string correctColumnsOrder = GetColumns(sourceTable);
                if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection))
                {
                    // throw exception if table structure is invalid
                    string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
                    msg += "The correct order is [" + correctColumnsOrder + "]";

                    connection.Dispose();
                    throw new TableStructureException(msg);
                }
                //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --end

                string tempTableName = "#" + sourceTable.TableName;
                SqlCommand command1 = new SqlCommand("SET ROWCOUNT 1; SELECT * INTO " + tempTableName + " FROM " + sourceTable.TableName + " WHERE 1=0; SET ROWCOUNT 0;", connection);
                command1.CommandType = CommandType.Text;
                command1.ExecuteNonQuery();

                //changed by UP - default
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    //bulkCopy.DestinationTableName = "_" + sourceTable.TableName;
                    bulkCopy.DestinationTableName = tempTableName;
                    bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                    bulkCopy.BatchSize = bulkBatchSize;

                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(sourceTable);
                    }
                    catch (Exception ex)
                    {
                        // LOG the exceptions here.
                        throw;
                    }
                }

                DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

                string commandText = queryRow["CommandText"].ToString();
                string commandType = queryRow["CommandType"].ToString();

                SqlCommand command = new SqlCommand(commandText, connection);
                command.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
                command.Connection = connection;
                command.CommandTimeout = commandTimeout;
                command.ExecuteNonQuery();
            }
        }

        public void Update(DataTable sourceTable, string spName, SqlConnection connection, SqlTransaction tx)
        {
            // First insert in the staging table, then bulk update

            // Create the SqlBulkCopy object. 
            // Note that the column positions in the source DataTable 
            // match the column positions in the destination table so 
            // there is no need to map columns. 

            //new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null);   // Check this for more performance, but will lock the table.

            //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --start
            string correctColumnsOrder = GetColumns(sourceTable);
            if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection, tx))
            {
                // throw exception if table structure is invalid
                string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
                msg += "The correct order is [" + correctColumnsOrder + "]";

                connection.Dispose();
                throw new TableStructureException(msg);
            }
            //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --end

            string tempTableName = "#" + sourceTable.TableName;
            SqlCommand command1 = new SqlCommand("SET ROWCOUNT 1; SELECT * INTO " + tempTableName + " FROM " + sourceTable.TableName + " WHERE 1=0; SET ROWCOUNT 0;", connection);
            command1.CommandType = CommandType.Text;
            command1.Transaction = tx;
            command1.ExecuteNonQuery();

            //changed by UP - default
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tx))
            {
                //bulkCopy.DestinationTableName = "_" + sourceTable.TableName;
                bulkCopy.DestinationTableName = tempTableName;
                bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                bulkCopy.BatchSize = bulkBatchSize;

                try
                {
                    // Write from the source to the destination.
                    bulkCopy.WriteToServer(sourceTable);
                }
                catch (Exception ex)
                {
                    // LOG the exceptions here.
                    throw;
                }
            }

            DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

            string commandText = queryRow["CommandText"].ToString();
            string commandType = queryRow["CommandType"].ToString();

            SqlCommand command = new SqlCommand(commandText, connection);
            command.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
            command.Connection = connection;
            command.Transaction = tx;
            command.CommandTimeout = commandTimeout;
            command.ExecuteNonQuery();

        }

        //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --start
        private string GetColumns(DataTable table)
        {
            string columns = "";
            foreach (DataColumn col in table.Columns)
            {
                columns += col.ColumnName.ToLower() + ",";
            }
            columns = columns.Substring(0, columns.Length - 1);

            return columns;
        }

        private bool IsTableStructureValid(string tableName, string columns, SqlConnection connection)
        {
            bool result = true;

            //changed by UP - Caching
            var tableColumns = CacheManager.Cache.Get(tableName + "-Columns");
            if (null == tableColumns || tableColumns.ToString().Length == 0)
            {
                //select STUFF(COLUMN_NAME, 1, 1, '') AS COLUMN_NAME from sys.tables t CROSS APPLY  (SELECT ',' + name AS [text()] FROM sys.columns c WHERE c.object_id = t.object_id FOR XML PATH('') ) o (COLUMN_NAME) where name='ebgltransaction'
                string query = "select STUFF(COLUMN_NAME, 1, 1, '') AS COLUMN_NAME from sys.tables t CROSS APPLY  (SELECT ',' + name AS [text()] FROM sys.columns c WHERE c.object_id = t.object_id FOR XML PATH('') ) o (COLUMN_NAME)";
                query += "where name='" + tableName + "'";
                SqlCommand command1 = new SqlCommand(query, connection);
                command1.CommandType = CommandType.Text;
                //var tableColumns = command1.ExecuteScalar();
                tableColumns = command1.ExecuteScalar();
                CacheManager.Cache.Add(tableName + "-Columns", tableColumns);
            }
            if (!columns.ToLower().Equals(tableColumns.ToString().ToLower()))
            {
                result = false;
            }
            return result;
        }

        private bool IsTableStructureValid(string tableName, string columns, SqlConnection connection, SqlTransaction tx)
        {
            bool result = true;

            //changed by UP - Caching
            var tableColumns = CacheManager.Cache.Get(tableName + "-Columns");
            if (null == tableColumns || tableColumns.ToString().Length == 0)
            {
                //select STUFF(COLUMN_NAME, 1, 1, '') AS COLUMN_NAME from sys.tables t CROSS APPLY  (SELECT ',' + name AS [text()] FROM sys.columns c WHERE c.object_id = t.object_id FOR XML PATH('') ) o (COLUMN_NAME) where name='ebgltransaction'
                string query = "select STUFF(COLUMN_NAME, 1, 1, '') AS COLUMN_NAME from sys.tables t CROSS APPLY  (SELECT ',' + name AS [text()] FROM sys.columns c WHERE c.object_id = t.object_id FOR XML PATH('') ) o (COLUMN_NAME)";
                query += "where name='" + tableName + "'";
                SqlCommand command1 = new SqlCommand(query, connection, tx);
                command1.CommandType = CommandType.Text;
                //var tableColumns = command1.ExecuteScalar();
                tableColumns = command1.ExecuteScalar();
                CacheManager.Cache.Add(tableName + "-Columns", tableColumns);
            }
            if (!columns.ToLower().Equals(tableColumns.ToString().ToLower()))
            {
                result = false;
            }
            return result;
        }
        //[CR128]: Validations added for Bulk insert to check the sequence of table columns.  --end

    }
}
