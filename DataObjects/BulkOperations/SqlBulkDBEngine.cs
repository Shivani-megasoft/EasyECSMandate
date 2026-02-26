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
using System.Security.Cryptography;
using System.Text;
using Oracle.DataAccess.Client;
using System.Linq;

namespace EasyBank.DataLayer.DataObjects
{
    public class OracleBulkDBEngine
    {
        private static readonly string dataProvider = ConfigurationManager.AppSettings.Get("DataProvider");
        private static string connectionString = DecryptConnectionString(ConfigurationManager.ConnectionStrings[dataProvider].ConnectionString);
        private static int bulkCopyTimeout = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("BulkCopyTimeout")) ? 2000 : ConfigurationManager.AppSettings.Get("BulkCopyTimeout").ToInt();
        private static int bulkBatchSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("BulkBatchSize")) ? 3000 : ConfigurationManager.AppSettings.Get("BulkBatchSize").ToInt();
        private static int commandTimeout = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("CommandTimeout")) ? 300 : ConfigurationManager.AppSettings.Get("CommandTimeout").ToInt();

        DataSet dsSQL;

        public OracleBulkDBEngine()
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

        public void Insert(DataTable sourceTable)
        {
            //P-Oracle Changes
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleBulkInsert(sourceTable, connection);

                //Comment for Oracle Changes
                //connection.Open();
                //string correctColumnsOrder = GetColumns(sourceTable);

                //if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection))
                //{
                //    string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
                //    msg += "The correct order is [" + correctColumnsOrder + "]";

                //    connection.Dispose();
                //    throw new TableStructureException(msg);
                //}

                //using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
                //{
                //    bulkCopy.DestinationTableName = sourceTable.TableName;
                //    bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                //    bulkCopy.BatchSize = bulkBatchSize;

                //    try
                //    {
                //        bulkCopy.WriteToServer(sourceTable);
                //    }
                //    catch (Exception ex)
                //    {
                //        throw;
                //    }
                //}
            }
        }

        public void Insert(DataTable sourceTable, OracleConnection connection)
        {
            //P-Oracle Changes
            OracleBulkInsert(sourceTable, connection);

            //Comment For Oracle changes
            //string correctColumnsOrder = GetColumns(sourceTable);

            //if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection))
            //{
            //    string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
            //    msg += "The correct order is [" + correctColumnsOrder + "]";

            //    throw new TableStructureException(msg);
            //}

            //using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
            //{
            //    bulkCopy.DestinationTableName = sourceTable.TableName;
            //    bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
            //    bulkCopy.BatchSize = bulkBatchSize;

            //    try
            //    {
            //        bulkCopy.WriteToServer(sourceTable);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw;
            //    }
            //}
        }

        public void Update(DataTable sourceTable, string spName)
        {
            // First insert in the staging table, then bulk update
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();

                string correctColumnsOrder = GetColumns(sourceTable);
                if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection))
                {
                    string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
                    msg += "The correct order is [" + correctColumnsOrder + "]";

                    connection.Dispose();
                    throw new TableStructureException(msg);
                }

                //P-Change
                //string tempTableName = "#" + sourceTable.TableName;
                string tempTableName = "tt_" + sourceTable.TableName;

                //OracleCommand command1 = new OracleCommand("SET ROWCOUNT 1; SELECT * INTO " + tempTableName + " FROM " + sourceTable.TableName + " WHERE 1=0; SET ROWCOUNT 0;", connection);
                //OracleCommand command1 = new OracleCommand(CreateTempTable(), connection);
                //command1.CommandType = CommandType.Text;
                //command1.ExecuteNonQuery();

                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
                {
                    bulkCopy.DestinationTableName = tempTableName;
                    bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                    bulkCopy.BatchSize = bulkBatchSize;

                    try
                    {
                        string commandText1 = "Delete from " + tempTableName;
                        string commandType1 = "Text";
                        OracleCommand command1 = new OracleCommand(commandText1, connection);
                        command1.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType1);
                        command1.Connection = connection;
                        command1.CommandTimeout = commandTimeout;
                        command1.ExecuteNonQuery();

                        bulkCopy.WriteToServer(sourceTable);

                        ////
                        //string commandText1 = "Select * from tt_ebProduct";
                        //string commandType1 = "Text";
                        //OracleCommand command1 = new Oracl00000000000000000000000000000000000...eCommand(commandText1, connection);
                        //command1.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType1);
                        //command1.Connection = connection;
                        //command1.CommandTimeout = commandTimeout;
                        //DataSet ds = new DataSet();
                        //using (OracleDataAdapter adapter = new OracleDataAdapter())
                        //{
                        //    adapter.SelectCommand = command1;
                        //    adapter.Fill(ds);
                        //}
                        ////
                        DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

                        string commandText = queryRow["CommandText"].ToString();
                        string commandType = queryRow["CommandType"].ToString();

                        OracleCommand command = new OracleCommand(commandText, connection);
                        command.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
                        command.Connection = connection;
                        command.CommandTimeout = commandTimeout;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }


            }
        }

        //P-Change
        private string CreateTempTable()
        {
            string query = "CREATE global TEMPORARY TABLE tt_ebProduct on commit delete rows as select* from ebProduct where 1 = 2 ";
            return query;
        }
        public void Update(DataTable sourceTable, string spName, OracleConnection connection)
        {
            string correctColumnsOrder = GetColumns(sourceTable);
            if (!IsTableStructureValid(sourceTable.TableName, correctColumnsOrder, connection))
            {
                string msg = "The column order for " + sourceTable.TableName + " is not correct.  Please verify." + Environment.NewLine;
                msg += "The correct order is [" + correctColumnsOrder + "]";

                connection.Dispose();
                throw new TableStructureException(msg);
            }

            //P-Change
            //string tempTableName = "#" + sourceTable.TableName;
            string tempTableName = "tt_" + sourceTable.TableName;

            //OracleCommand command1 = new OracleCommand("SET ROWCOUNT 1; SELECT * INTO " + tempTableName + " FROM " + sourceTable.TableName + " WHERE 1=0; SET ROWCOUNT 0;", connection);
            //command1.CommandType = CommandType.Text;
            //command1.ExecuteNonQuery();

            using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection, OracleBulkCopyOptions.Default))
            {
                bulkCopy.DestinationTableName = tempTableName;
                bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                bulkCopy.BatchSize = bulkBatchSize;

                try
                {
                    string commandText1 = "Delete from " + tempTableName;
                    string commandType1 = "Text";
                    OracleCommand command1 = new OracleCommand(commandText1, connection);
                    command1.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType1);
                    command1.Connection = connection;
                    command1.CommandTimeout = commandTimeout;
                    command1.ExecuteNonQuery();

                    bulkCopy.WriteToServer(sourceTable);

                    DataRow queryRow = dsSQL.Tables["Query"].Select("Id = '" + spName + "'")[0];

                    string commandText = queryRow["CommandText"].ToString();
                    string commandType = queryRow["CommandType"].ToString();

                    OracleCommand command = new OracleCommand(commandText, connection);
                    command.CommandType = (CommandType)Enum.Parse(typeof(CommandType), commandType);
                    command.Connection = connection;
                    command.CommandTimeout = commandTimeout;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        private string GetColumns(DataTable table)
        {
            string columns = "";
            foreach (DataColumn col in table.Columns)
                columns += col.ColumnName.ToLower() + ",";

            columns = columns.Substring(0, columns.Length - 1);
            return columns;
        }

        private bool IsTableStructureValid(string tableName, string columns, OracleConnection connection)
        {
            bool result = true;

            var tableColumns = CacheManager.Cache.Get(tableName + "-Columns");
            if (null == tableColumns || tableColumns.ToString().Length == 0)
            {

                string query = "select listagg(COLUMN_NAME,',') within group (order by  column_id) COLUMN_NAME  from user_tab_columns where table_name ='" + tableName.ToUpper() + "'";


                //string query = "select STUFF(COLUMN_NAME, 1, 1, '') AS COLUMN_NAME from sys.tables t CROSS APPLY  (SELECT ',' + name AS [text()] FROM sys.columns c WHERE c.object_id = t.object_id FOR XML PATH('') ) o (COLUMN_NAME)";
                //query += "where name='" + tableName + "'";
                OracleCommand command1 = new OracleCommand(query, connection);
                command1.CommandType = CommandType.Text;
                try
                {

                    tableColumns = command1.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                CacheManager.Cache.Add(tableName + "-Columns", tableColumns);
            }

            if (!columns.ToLower().Equals(tableColumns.ToString().ToLower()))
                result = false;

            return result;
        }

        //P-Change
        public void OracleBulkInsert(DataTable dt, OracleConnection connection)
        {
            string tbleName = dt.TableName;

            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                OracleCommand cmd = connection.CreateCommand();
                string commandText = "INSERT INTO " + tbleName + "(";
                // cmd.CommandText = "INSERT INTO EBPRODUCT (ACID,DATE_,CLRBAL,GRP,PRODAMT,RTOFINT,INTAMT,POSTDT,AUTHUSER,BRNCD,GLCD,ACNO,DELETED) VALUES (:1, :2, :3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13)";
                cmd.ArrayBindCount = dt.Rows.Count;

                DataView dv = new DataView(dt);

                OracleParameter oraPara = null;
                int colCount = 1;
                string colNumber = "";

                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == "Id") continue;

                    DataTable newdt = dv.ToTable(false, col.ColumnName);
                    DataRow[] row = newdt.Select();

                    if (colCount < dt.Columns.Count - 1)
                    {
                        commandText += col.ColumnName + ",";
                        colNumber += ":" + colCount + ",";
                    }
                    else
                    {
                        commandText += col.ColumnName;
                        colNumber += ":" + colCount;
                    }

                    colCount++;

                    oraPara = new OracleParameter();
                    switch (col.DataType.Name)
                    {
                        case "Int32":
                            oraPara.OracleDbType = OracleDbType.Int32;
                            oraPara.Value = row.AsEnumerable().Select(r => Convert.ToInt32(r[0])).ToList().ToArray(); //row.Select(c =>Convert.ToInt16(c)).ToArray();//.Cast<Int32[]>().ToArray();
                            break;
                        case "String":
                            oraPara.OracleDbType = OracleDbType.Char;
                            oraPara.Value = row.AsEnumerable().Select(c => c[0].ToString()).ToList().ToArray();
                            break;
                        case "DateTime":
                            oraPara.OracleDbType = OracleDbType.Date;
                            oraPara.Value = row.AsEnumerable().Select(c =>
                            {
                                DateTime? dateL;
                                if (c[0] != DBNull.Value)
                                    dateL = Convert.ToDateTime(c[0]);
                                else dateL = null;
                                return dateL;
                            }).ToList().ToArray().Select(d => (DateTime?)d).ToArray();
                            //oraPara.Value = row.AsEnumerable().Select(c => (DateTime?)c[0]).ToList().ToArray();

                            break;
                        case "Boolean":
                            oraPara.OracleDbType = OracleDbType.Int16;
                            oraPara.Value = row.AsEnumerable().Select(c => Convert.ToBoolean(c[0]) == true ? 1 : 0).ToList().ToArray();
                            break;
                        case "Decimal":
                            oraPara.OracleDbType = OracleDbType.Decimal;
                            oraPara.Value = row.AsEnumerable().Select(c => Convert.ToDecimal(c[0])).ToList().ToArray();
                            break;
                    }

                    cmd.Parameters.Add(oraPara);

                }
                commandText += ")Values(" + colNumber + ")";
                cmd.CommandText = commandText;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
