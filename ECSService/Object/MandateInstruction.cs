using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.BusinessLayer.Base.Services;
using EasyBank.Framework.EasyAttributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EASYECS.BussinessLayer.ECSService.Object

{
    [Serializable]
    [DataContract]
    [Table("EBMANDATEINSTRUCTION")]
    public class MandateInstruction : BaseObject
    {
        private GlobalIdGenerator globalIdGenerator = null;
        [NonSerialized]
        protected GlobalIdGeneratorService globalIdGeneratorService;

        #region Constant
        public const string TABLE_EBMANDATEINSTRUCTION = "EBMANDATEINSTRUCTION";
        public const string FIELD_ID = "ID";
        public const string FIELD_MNDTID = "MNDTID";
        public const string FIELD_DMDDT = "DMDDT";
        public const string FIELD_EXECDT = "EXECDT";
        public const string FIELD_AMT = "AMT";
        public const string FIELD_INPFILENAME = "INPFILENAME";
        public const string FIELD_ACKSTAT = "ACKSTAT";
        public const string FIELD_ACKDT = "ACKDT";
        public const string FIELD_ACKRESP = "ACKRESP";
        public const string FIELD_RESPSTAT = "RESPSTAT";
        public const string FIELD_RESPDT = "RESPDT";
        public const string FIELD_RESPFILENAME = "RESPFILENAME";
        public const string FIELD_REFNO = "REFNO";
        public const string FIELD_RCODE = "RCODE";
        public const string FIELD_REM = "REM";
        public const string FIELD_STAT = "STAT";
        public const string FIELD_DTSTAMP = "DTSTAMP";
        public const string FIELD_AUTHUSER = "AUTHUSER";
        #endregion
        #region Constructor
        public MandateInstruction()
        {
            this.IsNew = true;
        }
        public MandateInstruction(DataRow row)
        {
            this.IsNew = false;
            this.Id = Convert.ToInt32(row[FIELD_ID]);
            this.MNDTID = Convert.ToInt32(row[FIELD_MNDTID]);
            if (row[FIELD_DMDDT] != DBNull.Value)
                this.DMDDT = Convert.ToDateTime(row[FIELD_DMDDT]);
            if (row[FIELD_EXECDT] != DBNull.Value)
                this.EXECDT = Convert.ToDateTime(row[FIELD_EXECDT]);
           this.AMT = Convert.ToDecimal(row[FIELD_AMT]);
            this.INPFILENAME = row[FIELD_INPFILENAME].ToString();
            this.ACKSTAT = row[FIELD_ACKSTAT].ToString();
            if (row[FIELD_ACKDT] != DBNull.Value)
                this.ACKDT = Convert.ToDateTime(row[FIELD_ACKDT]);
            this.ACKRESP = row[FIELD_ACKRESP].ToString();
            this.RESPSTAT = row[FIELD_RESPSTAT].ToString();
            if (row[FIELD_RESPDT] != DBNull.Value)
                this.RESPDT = Convert.ToDateTime(row[FIELD_RESPDT]);
             this.RESPFILENAME = row[FIELD_RESPFILENAME].ToString();
            this.REFNO = row[FIELD_REFNO].ToString();
            this.RCODE = row[FIELD_RCODE].ToString();
            this.REM = row[FIELD_REM].ToString();
            this.STAT = row[FIELD_STAT].ToString();
            this.DTSTAMP = Convert.ToDateTime(row[FIELD_DTSTAMP]);
            this.AUTHUSER = row[FIELD_AUTHUSER].ToString();

        }
        #endregion
        #region Public properties

        [DataMember]
        [Column()]
        [PrimaryKey]
        public Int32 Id { get; set; }

        [DataMember]
        [Column()]
        public Int32 MNDTID { get; set; }


        [DataMember]
        [Column()]
        public DateTime DMDDT { get; set; }

        [DataMember]
        [Column()]
        public DateTime? EXECDT { get; set; }

        [DataMember]
        [Column()]
        public decimal AMT { get; set; }
        [DataMember]
        [Column()]
        public string INPFILENAME { get; set; }

        [DataMember]
        [Column()]
        public string ACKSTAT { get; set; }

        [DataMember]
        [Column()]
        public DateTime? ACKDT { get; set; }

        [DataMember]
        [Column()]
        public string ACKRESP { get; set; }

        [DataMember]
        [Column]
        public string RESPSTAT { get; set; }

        [DataMember]
        [Column]
        public DateTime? RESPDT { get; set; }

        [DataMember]
        [Column]
        public string RESPFILENAME { get; set; }
        [DataMember]
        [Column]
        public string REFNO { get; set; }
        [DataMember]
        [Column]
        public string RCODE { get; set; }
        [DataMember]
        [Column]
        public string REM { get; set; }
        [DataMember]
        [Column]
        public string STAT { get; set; }
        [DataMember]
        [Column]
        public DateTime DTSTAMP { get; set; }
        [DataMember]
        [Column]
        public string AUTHUSER { get; set; }


        public GlobalIdGenerator GlobalIdGenerator
        {
            get
            {
                if (null == globalIdGeneratorService)
                {
                    globalIdGeneratorService = new GlobalIdGeneratorService();
                }
                if (globalIdGenerator == null)
                    globalIdGenerator = globalIdGeneratorService.GetMaxId(TABLE_EBMANDATEINSTRUCTION);

                return globalIdGenerator;
            }
        }

        #endregion
    }
}