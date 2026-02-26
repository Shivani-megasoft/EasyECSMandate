using EasyBank.BusinessLayer.Base.Objects;
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
    [Table("ebMandateRtnReasonMaster")]
    public class MandateReturnReason : BaseObject
    {
        #region Constants
        public const string FIELD_ID = "Id";
        public const string FIELD_REASON_CODE = "RsnCd";
        public const string FIELD_REASON = "Reason";
        public const string FIELD_RETURN_ON = "ReturnOn";
        public const string FIELS_DTSTAMP = "DtStamp";
        public const string FIELD_OPERATION = "Operation";
        public const string FIELD_DELETED = "Deleted";
        public const string FIELD_AUTH_USER = "AuthUser";
        public const string FIELD_NORMAL_APPL = "NormalAppl";
        public const string FIELD_DEST_APPL = "DestAppl";
        public const string FIELD_API_APPL = "ApiAppl";
        public const string FIELD_MWI_APPL = "MwiAppl";
        public const string FIELD_CHG_APPL = "ChgApl";
        public const string FIELD_ESIGN_APPL = "ESign";

        #endregion
        #region Constructor
        public MandateReturnReason()
        {
            this.IsNew = true;
        }

        public MandateReturnReason(DataRow row)
        {
            this.IsNew = false;
            //if (row[FIELD_ID] != DBNull.Value)
            this.Id = Convert.ToInt32(row[FIELD_ID]);
            this.RsnCd = row[FIELD_REASON_CODE].ToString();
            this.Reason = row[FIELD_REASON].ToString();
            this.ReturnOn = row[FIELD_RETURN_ON].ToString();
            this.DtStamp = Convert.ToDateTime(row[FIELS_DTSTAMP]);
            this.Operation = row[FIELD_OPERATION].ToString();
            this.Deleted = row[FIELD_DELETED].ToString();
            this.AuthUser = row[FIELD_AUTH_USER].ToString();
            if (row[FIELD_NORMAL_APPL] != DBNull.Value)
                this.NormalAppl = Convert.ToBoolean(row[FIELD_NORMAL_APPL]);
            if (row[FIELD_DEST_APPL] != DBNull.Value)
                this.DestAppl = Convert.ToBoolean(row[FIELD_DEST_APPL]);
            if (row[FIELD_API_APPL] != DBNull.Value)
                this.ApiAppl = Convert.ToBoolean(row[FIELD_API_APPL]);
            if (row[FIELD_MWI_APPL] != DBNull.Value)
                this.MwiAppl = Convert.ToBoolean(row[FIELD_MWI_APPL]);
            if (row[FIELD_CHG_APPL] != DBNull.Value)
                this.ChgApl = Convert.ToBoolean(row[FIELD_CHG_APPL]);
            if (row[FIELD_ESIGN_APPL] != DBNull.Value)
                this.ESign = Convert.ToBoolean(row[FIELD_ESIGN_APPL]);
        }

        #endregion

        #region Properties
        [DataMember]
        [Column()]
        [PrimaryKey]
        public int Id { get; set; }

        [DataMember]
        [Column()]
        public string RsnCd { get; set; }

        [DataMember]
        [Column()]
        public string Reason { get; set; }

        [DataMember]
        [Column()]
        public string ReturnOn { get; set; }

        [DataMember]
        [Column()]
        public bool NormalAppl { get; set; }

        [DataMember]
        [Column()]
        public bool DestAppl { get; set; }

        [DataMember]
        [Column()]
        public bool ApiAppl { get; set; }

        [DataMember]
        [Column()]
        public bool MwiAppl { get; set; }

        [DataMember]
        [Column()]
        public DateTime DtStamp { get; set; }

        [DataMember]
        [Column()]
        public string Operation { get; set; }

        [DataMember]
        [Column()]
        public string Deleted { get; set; }

        [DataMember]
        [Column()]
        public string AuthUser { get; set; }
        
        [DataMember]
        [Column()]
        public bool ESign { get; set; }

        [DataMember]
        [Column()]
        public bool ChgApl { get; set; }



        #endregion
    }
}
