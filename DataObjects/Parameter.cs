using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace EasyBank.DataLayer.DataObjects
{
    public class Parameter : DbParameter
    {
        private string parameterName;
        private object parameterValue;
        private string parameterDataType;

        public Parameter(string name, object value)
        {
            this.parameterName = name;
            this.parameterValue = value;
        }

        public Parameter(string name, object value, string dataType)
        {
            this.parameterName = name;
            this.parameterValue = value;
            this.parameterDataType = dataType;
        }

        public override string ParameterName
        {
            get { return this.parameterName; }
            set { this.parameterValue = value; }
        }

        public override object Value
        {
            get { return this.parameterValue; }
            set { this.parameterValue = value; }
        }

        public string DataType
        {
            get { return this.parameterDataType; }
            set { this.parameterDataType = value; }
        }

        public override System.Data.DbType DbType
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override System.Data.ParameterDirection Direction
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool IsNullable
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override void ResetDbType()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override int Size
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override string SourceColumn
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool SourceColumnNullMapping
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override System.Data.DataRowVersion SourceVersion
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }
    }
}