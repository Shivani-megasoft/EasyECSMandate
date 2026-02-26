using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.Framework.EasyAttributes;
using System;
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EASYECS.BussinessLayer.ECSService.Object
{
    [Serializable]
    [DataContract]
    [Table("ebMandateFileHistory")]
    public class MandateFileHistory : BaseObject
    {
        #region Constant
        public const string TABLE_MANDATEFILEHISTORY = "ebMandateFileHistory";
        public const string FIELD_ID = "Id";
        public const string FIELD_MNDT_ID = "MndtId";
        public const string FIELD_MNDT_REQ_TYP = "MndtReqTyp";
        public const string FIELD_XML_FILE = "XMLFile";
        public const string FIELD_FILE_PATH = "FilePath";
        public const string FIELD_DT_STAMP = "DtStamp";
        public const string FIELD_AUTH_USER = "AuthUser";
        public const string FIELD_IS_ACCEPT = "IsAccept";
        public const string FIELD_REASON = "Reason";

        #endregion
        #region Constructor
        public MandateFileHistory()
        {
            this.IsNew = true;
        }
        public MandateFileHistory(DataRow row)
        {
            this.IsNew = false;
            this.Id = Convert.ToInt32(row[FIELD_ID]);
            this.MndtId = Convert.ToInt32(row[FIELD_MNDT_ID]);
            this.MndtReqTyp = row[FIELD_MNDT_REQ_TYP].ToString();
            this.XMLFile = row[FIELD_XML_FILE].ToString();
            this.FilePath = row[FIELD_FILE_PATH].ToString();
            if (row[FIELD_DT_STAMP] != DBNull.Value)
                this.DtStamp = Convert.ToDateTime(row[FIELD_DT_STAMP]);
            this.AuthUser = row[FIELD_AUTH_USER].ToString();
            if (row[FIELD_IS_ACCEPT] != DBNull.Value)
                this.IsAccept = Convert.ToBoolean(row[FIELD_IS_ACCEPT]);
            this.Reason = row[FIELD_REASON].ToString();
        }
        #endregion

        #region Public properties

        [DataMember]
        [Column()]
        [PrimaryKey]
        public Int32 Id { get; set; }

        [DataMember]
        [Column()]
        public Int32 MndtId { get; set; }

        [DataMember]
        [Column()]
        public string MndtReqTyp { get; set; }

        [DataMember]
        [Column()]
        public string XMLFile { get; set; }

        [DataMember]
        [Column]
        public string FilePath { get; set; }

        [DataMember]
        [Column()]
        public DateTime DtStamp { get; set; }

        [DataMember]
        [Column()]
        public string AuthUser { get; set; }

        [DataMember]
        [Column]
        public bool IsAccept { get; set; }

        [DataMember]
        [Column]
        public string Reason { get; set; }
        #endregion
    }
}
