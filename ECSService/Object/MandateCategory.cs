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
    [Table("ebMandateCategoryMaster")]
    public class MandateCategory : BaseObject
    {
        #region Constants
        public const string TABLE_MANDATE_CATEGORY_MASTER = "ebMandtCategoryMaster";
        public const string FIELD_ID = "Id";
        public const string FIELD_CAT_CODE = "CatCode";
        public const string FIELD_DESCRP = "Descrp";
        public const string FIELD_AUTH_USER = "AuthUser";
        public const string FIELD_DELETED = "Deleted";
        public const string FIELD_DTSTAMP = "DtStamp";
        #endregion

        #region Constructor
        public MandateCategory()
        {
            this.IsNew = true;
        }
        public MandateCategory(DataRow row)
        {
            this.IsNew = false;

            //if (row[FIELD_ID] != DBNull.Value)
            this.Id = Convert.ToInt32(row[FIELD_ID]);
            this.CatCode = row[FIELD_CAT_CODE].ToString();
            this.Descrp = row[FIELD_DESCRP].ToString();
            this.AuthUser = row[FIELD_AUTH_USER].ToString();
            if (row[FIELD_DTSTAMP] != DBNull.Value)
                this.DtStamp = Convert.ToDateTime(row[FIELD_DTSTAMP]);
            this.Deleted = row[FIELD_DELETED].ToString();
        }
        #endregion

        #region Properties
        [DataMember]
        [Column()]
        [PrimaryKey]
        public Int32 Id { get; set; }

        [DataMember]
        [Column()]
        public string CatCode { get; set; }

        [DataMember]
        [Column()]
        public string Descrp { get; set; }

        [DataMember]
        [Column()]
        public string AuthUser { get; set; }

        [DataMember]
        [Column()]
        public string Deleted { get; set; }

        [DataMember]
        [Column()]
        public DateTime DtStamp { get; set; }
        #endregion
    }
}
