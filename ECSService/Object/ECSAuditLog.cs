using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.Framework.EasyAttributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECSService.BussinessLayer.ECSService.Object
{
    [Serializable]
    [DataContract]
    [Table("ECSAUDITLOG")]
    public class ECSAuditLog : BaseObject
    {
        #region Constants
        public const string TABLE_MANDATE_CATEGORY_MASTER = "ebMandtCategoryMaster";
        public const string FIELD_ID = "Id";
        public const string FIELD_BRNCD = "BRNCD";
        public const string FIELD_UMRNO = "UMRNO";
        public const string FIELD_DATE_ = "DATE_";
        public const string FIELD_KEY = "KEY";
        public const string FIELD_ENTITY = "ENTITY";
        public const string FIELD_DATA = "DATA";
        public const string FIELD_AUTHUSER = "AUTHUSER";
        #endregion

        #region Constructor
        public ECSAuditLog()
        {
            this.IsNew = true;
        }
        public ECSAuditLog(DataRow row)
        {
            this.IsNew = false;

            //if (row[FIELD_ID] != DBNull.Value)
            this.Id = Convert.ToInt32(row[FIELD_ID]);
            this.BRNCD = row[FIELD_BRNCD].ToString();
            this.UMRNO = row[FIELD_UMRNO].ToString();
            this.DATE_ = Convert.ToDateTime(row[FIELD_DATE_]);
            this.KEY = Convert.ToInt32(row[FIELD_KEY]);
            this.ENTITY = row[FIELD_ENTITY].ToString();
            this.DATA = row[FIELD_DATA].ToString();
            this.AUTHUSER = row[FIELD_AUTHUSER].ToString();
        }
        #endregion

        #region Properties
        [DataMember]
        [Column()]
        [PrimaryKey]
        public Int32 Id { get; set; }

        [DataMember]
        [Column()]
        public string BRNCD { get; set; }

        [DataMember]
        [Column()]
        public string UMRNO { get; set; }

        [DataMember]
        [Column()]
        public DateTime  DATE_ { get; set; }

        [DataMember]
        [Column()]
        public int KEY { get; set; }

        [DataMember]
        [Column()]
        public string ENTITY { get; set; }

        [DataMember]
        [Column()]
        public string DATA { get; set; }

        [DataMember]
        [Column()]
        public string AUTHUSER { get; set; }
        #endregion
    }
}
