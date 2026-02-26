using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Oracle.DataAccess.Client;

namespace EasyBank.DataLayer.DataObjects
{
    public class Request
    {
        private string commandText = string.Empty;
        private CommandType commandType = CommandType.Text;
        private OracleTransaction transaction;
        List<OracleParameter> collection = new List<OracleParameter>();

        public string CommandText
        {
            get { return this.commandText; }
            set { this.commandText = value; }
        }

        public CommandType CommandType
        {
            get { return this.commandType; }
            set { this.commandType = value; }
        }

        public OracleTransaction Transaction
        {
            get { return this.transaction; }
            set { this.transaction = value; }
        }

        public List<OracleParameter> Parameters
        {
            get { return this.collection; }
        }
    }
}