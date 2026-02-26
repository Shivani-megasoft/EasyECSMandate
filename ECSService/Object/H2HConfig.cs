using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.Framework.EasyAttributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECSService.Object
{
    [Serializable]
    [DataContract]
    [Table("ehConfig")]
    public class H2HConfig : BaseObject
    {
        #region constant
        public const string TABLE_H2HCONFIG = "ehConfig";
        public const string FIELD_ID = "Id";
        public const string FIELD_CONFIG_KEY = "ConfigKey";
        public const string FIELD_CONFIG_VALUE = "ConfigValue";
        public const string FIELD_STATUS = "Status";

        #endregion

        #region Constructors
        public H2HConfig()
        {
           // this.Id = Guid.NewGuid();
            this.IsNew = true;
        }

        public H2HConfig(DataRow row)
        {
            this.IsNew = false;
            //if (row[FIELD_ID] != DBNull.Value)
            this.Id = Convert.ToInt32(row[FIELD_ID]);

            this.ConfigKey = row[FIELD_CONFIG_KEY].ToString();
            this.ConfigValue = row[FIELD_CONFIG_VALUE].ToString();
            this.Status = row[FIELD_STATUS].ToString();

        }
        #endregion

        #region Public properties

        [DataMember]
        [Column()]
        [PrimaryKey]
        public Int32 Id { get; set; }

        [DataMember]
        [Column()]
        public string ConfigKey { get; set; }

        [DataMember]
        [Column()]
        public string ConfigValue { get; set; }

        [DataMember]
        [Column()]
        public string Status { get; set; }
        

        #endregion
    }
}
