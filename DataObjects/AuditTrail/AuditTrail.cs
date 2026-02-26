using System;
using System.Data;
using EasyBank.Framework.EasyAttributes;
using EasyBank.BusinessLayer.Base.Objects;

namespace EasyBank.DataLayer.DataObjects
{
    /// <summary>
    /// AuditLog business object.  Contains information about AuditLogs and
    /// its associated object creation behaviours.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Serializable]
    //  [Table("ebAuditLog")]
    [Table("ebAuditData")]
    public class AuditTrail : BaseObject
    {

        #region Constants

        public const string FIELD_ID = "Id";
        public const string FIELD_DATE = "Date_";
        public const string FIELD_KEY = "Key";
        public const string FIELD_HEADER = "Header";
        public const string FIELD_NEW_DATA = "OldData";
        public const string FIELD_OLD_DATA = "NewData";
        public const string FIELD_ENTITY = "Entity";
        public const string FIELD_KEYNAME = "KeyName";
        public const string FIELD_UPDATEDBY = "UpdatedBy";
        public const string FIELD_BRNCD = "BrnCd";
        //new col
        public const string FIELD_IP_ADDR = "IPAddr";


        #endregion

        # region Contructors

        public AuditTrail()
        {
            this.IsNew = true;
            //this.Id = Guid.NewGuid();
        }

        public AuditTrail(DataRow row)
        {
            this.IsNew = false;
            this.Id = Convert.ToInt32(row[FIELD_ID]);
            this.Date_ = Convert.ToDateTime(row[FIELD_DATE]);
            this.Key = row[FIELD_KEY].ToString();
            this.NewData = row[FIELD_NEW_DATA].ToString();
            this.OldData = row[FIELD_NEW_DATA].ToString();
            this.Entity = row[FIELD_ENTITY].ToString();
            this.KeyName = row[FIELD_KEYNAME].ToString();
            this.UpdatedBy = row[FIELD_UPDATEDBY].ToString();
            this.BrnCd = row[FIELD_BRNCD].ToString();
            this.Header = row[FIELD_HEADER].ToString();
            this.IPAddr = row[FIELD_IP_ADDR].ToString();//new col

        }

        # endregion

        # region Public properties

        [Column()]
        public Int32 Id { get; set; }

        [Column()]
        public string UpdatedBy { get; set; }


        [Column()]
        public DateTime? Date_ { get; set; }

        [Column()]
        public string Key { get; set; }

        [Column()]
        public string KeyName { get; set; }


        [Column()]
        public string Entity { get; set; }

        [Column()]
        public string NewData { get; set; }

        [Column()]
        public string OldData { get; set; }

        [Column()]
        public string Header { get; set; }

        [Column()]
        public string BrnCd { get; set; }

        [Column()]
        public string IPAddr { get; set; }

        # endregion

    }
}