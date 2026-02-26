using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.BusinessLayer.Base.Services;
using EasyBank.Framework.EasyAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EASYECS.BussinessLayer.ECSService.Object
{
    [Serializable]
    [DataContract]
    [Table("ebECSMandateHistory")]
    public class ECSHistory : ECS

    {
        #region Constants

        #endregion

        #region Constructors
        public ECSHistory() : base()
        {


        }

        public ECSHistory(DataRow row) : base(row)
        {

        }
        #endregion

        #region Public properties


        #endregion
    }
}
