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
    [Table("ebECSMandate")]
    public class ECS : BaseObject
    {
        private GlobalIdGenerator globalIdGenerator = null;
        [NonSerialized]
        protected GlobalIdGeneratorService globalIdGeneratorService;

        #region Constants
        public const string TABLE_ECSMANDATE = "ebECSMandate";
        public const string FIELD_ID = "Id";
        public const string FIELD_MANDATE_FILE = "MandateFile";
        public const string FIELD_MSG_ID = "MsgId"; //need to add in table
        public const string FIELD_UMRNO = "UMRNo";
        public const string FIELD_UMRDT = "UMRDt";
        public const string FIELD_SPNSR_BNKCD = "SpnsrBnkCd";
        public const string FIELD_UTILITYCD = "UtilityCd";
        public const string FIELD_UTILITYNAME = "UtilityName";
        public const string FIELD_ACTION = "Action";
        public const string FIELD_ACTYP = "AcTyp";
        public const string FIELD_ACNO = "AcNo";
        public const string FIELD_BANKNAME = "BankName";
        public const string FIELD_BRNNAME = "BrnName";
        public const string FIELD_IFSC = "IFSC";
        public const string FIELD_BNK_CD = "BnkCd";
        public const string FIELD_AMT = "Amt";
        public const string FIELD_REF_NO1 = "RefNo1";
        public const string FIELD_REF_NO2 = "RefNo2";
        public const string FIELD_FREQ = "Freq";
        public const string FIELD_DEBIT_TYP = "DebitTyp";
        public const string FIELD_FRM_PRD = "FrmPrd";
        public const string FIELD_TO_PRD = "ToPrd";
        public const string FIELD_IS_UNIT_CANCEL = "IsUnitCancel";
        public const string FIELD_PHONE_NO = "PhoneNo";
        public const string FIELD_EMAIL = "EMail";
        public const string FIELD_DETAIL_FRNT_IMG = "DetailFrntImg";
        public const string FIELD_FRNT_IMG = "FrntImg";
        public const string FIELD_MANDATE_STATUS = "MandateStatus";
        public const string FIELD_DT_STAMP = "DtStamp";
        public const string FIELD_CLERK = "Clerk";
        public const string FIELD_AUTH_USER = "AuthUser";
        public const string FIELD_RESERVE_BY = "ReserveBy";
        public const string FIELD_OPERATION = "Operation";
        public const string FIELD_DELETED = "Deleted";
        public const string FIELD_TRN_IND = "TrnInd";
        public const string FIELD_MANDATE_TYP = "MandateTyp";
        public const string FIELD_SPNSR_BANKNAME = "SpnsrBankName";
        public const string FIELD_CRDTR_AGENT_CODE = "CredtrAgentCode";
        public const string FIELD_CRDTR_AGENT_NAME = "CredtrAgentName";
        public const string FIELD_CRDTR_NAME = "CrdtrName";
        public const string FIELD_MOBILE = "Mobile";
        public const string FIELD_OTHR_DETAIL = "OthrDetail";
        public const string FIELD_DEBTR_CODE = "DebtrCode";
        public const string FIELD_DEBTR_BNK = "DebtrBnk";
        public const string FIELD_USER_NACH_ID = "UserNACHId";
        public const string FIELD_MANDATE_REQ_ID = "MndtReqId";
        public const string FIELD_CATEGORY = "Category";
        public const string FIELD_PROPRIETARY = "Proprietary";
        public const string FIELD_SEQ = "Seq";
        public const string FIELD_RTN_RSN = "RtnRsn";
        public const string FIELD_CR_BRNCD = "CrBrnCd";
        public const string FIELD_CR_GLCD = "CrGlCd";
        public const string FIELD_CR_ACNO = "CrAcNo";
        public const string FIELD_VARIANT = "Variant";
        public const string FIELD_NPCI_FILE = "NPCIFile";
        public const string FIELD_RESERVE_DT = "ReserveDt";
        public const string FIELD_EXEC_DT = "ExecDt";
        public const string FIELD_AUTH_DT = "AuthDt";
        public const string FIELD_DEBTR_NAME = "DebtrName";
        public const string CREATE = "Create";
        public const string CREATE_ACK = "CreateAck";
        public const string CREATE_INWARD = "CreateInw";
        public const string AMEND = "Amend";
        public const string AMEND_ACK = "AmendAck";
        public const string AMEND_INWARD = "AmendInw";
        public const string CANCEL = "Cancel";
        public const string CANCEL_ACK = "CancelAck";
        public const string CANCEL_INWRAD = "CancelInw";
        public const string ACCEPT = "Accept";
        public const string ACCEPT_ACK = "AcceptAck";
        public const string FIELD_REQDT = "ReqDt";
        public const string FIELD_RESDT = "ResDt";
        public const string FIELD_ISACCEPT = "IsAccept";
        public const string FIELD_STATUS = "Status";
        public const string FIELD_REASON = "Reason";
        public const string FIELD_RESFILE = "ResFile";
        public const string FIELD_DATEOFMANDATE = "DateOfMandate";
        #endregion
        #region Constructors
        public ECS()
        {
            //this.Id = ;
            this.IsNew = true;
        }
        public ECS(DataRow row)
        {
            this.IsNew = false;

            this.Id = Convert.ToInt32(row[FIELD_ID]);
            this.MandateFile = row[FIELD_MANDATE_FILE].ToString();
            if (row[FIELD_UMRNO] != DBNull.Value)
                this.UMRNo = row[FIELD_UMRNO].ToString();
            this.MsgId = row[FIELD_MSG_ID].ToString();
            this.UMRDt = Convert.ToDateTime(row[FIELD_UMRDT]);
            this.SpnsrBnkCd = row[FIELD_SPNSR_BNKCD].ToString();
            this.UtilityCd = row[FIELD_UTILITYCD].ToString();
            this.UtilityName = row[FIELD_UTILITYNAME].ToString();
            this.Action = row[FIELD_ACTION].ToString();
            this.AcTyp = row[FIELD_ACTYP].ToString();
            this.AcNo = row[FIELD_ACNO].ToString();
            this.BankName = row[FIELD_BANKNAME].ToString();
            this.BrnName = row[FIELD_BRNNAME].ToString();
            this.IFSC = row[FIELD_IFSC].ToString();
            this.BnkCd = row[FIELD_BNK_CD].ToString();
            this.Amt = Convert.ToDecimal(row[FIELD_AMT]);
            this.RefNo1 = row[FIELD_REF_NO1].ToString();
            this.RefNo2 = row[FIELD_REF_NO2].ToString();
            this.Freq = row[FIELD_FREQ].ToString();
            this.DebitTyp = row[FIELD_DEBIT_TYP].ToString();
            if (row[FIELD_FRM_PRD] != DBNull.Value)
                this.FrmPrd = Convert.ToDateTime(row[FIELD_FRM_PRD]);
            if (row[FIELD_TO_PRD] != DBNull.Value)
                this.ToPrd = Convert.ToDateTime(row[FIELD_TO_PRD]);
            this.DtStamp = Convert.ToDateTime(row[FIELD_DT_STAMP]);
            if (row[FIELD_IS_UNIT_CANCEL] != DBNull.Value)
                this.IsUnitCancel = Convert.ToBoolean(row[FIELD_IS_UNIT_CANCEL]);
            this.PhoneNo = row[FIELD_PHONE_NO].ToString();
            this.EMail = row[FIELD_EMAIL].ToString();
            this.ReserveBy = row[FIELD_RESERVE_BY].ToString();
            this.DetailFrntImg = row[FIELD_DETAIL_FRNT_IMG].ToString();
            this.MandateStatus = row[FIELD_MANDATE_STATUS].ToString();
            this.FrntImg = row[FIELD_FRNT_IMG].ToString();
            this.Deleted = row[FIELD_DELETED].ToString();
            this.Operation = row[FIELD_OPERATION].ToString();
            this.TrnInd = row[FIELD_TRN_IND].ToString();
            this.Clerk = row[FIELD_CLERK].ToString();
            this.MandateTyp = row[FIELD_MANDATE_TYP].ToString();
            //new column
            this.SpnsrBankName = row[FIELD_SPNSR_BANKNAME].ToString();
            this.CredtrAgentCode = row[FIELD_CRDTR_AGENT_CODE].ToString();
            this.CredtrAgentName = row[FIELD_CRDTR_AGENT_NAME].ToString();
            this.CrdtrName = row[FIELD_CRDTR_NAME].ToString();
            this.Mobile = row[FIELD_MOBILE].ToString();
            this.OthrDetail = row[FIELD_OTHR_DETAIL].ToString();
            this.DebtrCode = row[FIELD_DEBTR_CODE].ToString();
            this.DebtrBnk = row[FIELD_DEBTR_BNK].ToString();
            //Need to add in DB
            this.UserNACHId = row[FIELD_USER_NACH_ID].ToString();
            this.MndtReqId = row[FIELD_MANDATE_REQ_ID].ToString();
            this.Category = row[FIELD_CATEGORY].ToString();
            this.Proprietary = row[FIELD_PROPRIETARY].ToString();
            this.Seq = row[FIELD_SEQ].ToString();
            this.RtnRsn = row[FIELD_RTN_RSN].ToString();
            this.Variant = row[FIELD_VARIANT].ToString();
            this.NPCIFile = row[FIELD_NPCI_FILE].ToString();
            if (row[FIELD_RESERVE_DT] != DBNull.Value)
                this.ReserveDt = Convert.ToDateTime(row[FIELD_RESERVE_DT]);
            if (row[FIELD_AUTH_DT] != DBNull.Value)
                this.AuthDt = Convert.ToDateTime(row[FIELD_AUTH_DT]);
            this.CrBrnCd = row[FIELD_CR_BRNCD].ToString();
            if (row[FIELD_EXEC_DT] != DBNull.Value)
                this.ExecDt = Convert.ToDateTime(row[FIELD_EXEC_DT]);
            this.CrGlCd = row[FIELD_CR_GLCD].ToString();
            this.CrAcNo = row[FIELD_CR_ACNO].ToString();
            this.AuthUser = row[FIELD_AUTH_USER].ToString();
            this.DebtrName = row[FIELD_DEBTR_NAME].ToString();
            if (row[FIELD_REQDT] != DBNull.Value)
                this.ReqDt = Convert.ToDateTime(row[FIELD_REQDT]);
            if (row[FIELD_RESDT] != DBNull.Value)
                this.ResDt = Convert.ToDateTime(row[FIELD_RESDT]);
            if (row[FIELD_ISACCEPT] != DBNull.Value)
                this.IsAccept = Convert.ToBoolean(row[FIELD_ISACCEPT]);
            this.Status = row[FIELD_STATUS].ToString();
            this.Reason = row[FIELD_REASON].ToString();
            this.ResFile = row[FIELD_RESFILE].ToString();
            if (row[FIELD_DATEOFMANDATE] != DBNull.Value)
                this.DateOfMandate = Convert.ToDateTime(row[FIELD_DATEOFMANDATE]);

            this.Snapshot = ObjectCloner.Clone(this);
        }
        #endregion

        #region Public properties

        [DataMember]
        [Column()]
        [Required(ErrorMessage = "UMRN is required")]
        public string UMRNo { get; set; }

        [DataMember]
        [Column()]
        public string MandateFile { get; set; }

        [DataMember]
        [Column()]
        public string MsgId { get; set; }
        [DataMember]
        [Column()]
        public DateTime UMRDt { get; set; }
        [DataMember]
        [Column()]
        public string SpnsrBnkCd { get; set; }
        [DataMember]
        [Column()]
        public string UtilityCd { get; set; }
        [DataMember]
        [Column()]
        public string UtilityName { get; set; }
        [DataMember]
        [Column()]
        public string Action { get; set; }
        [DataMember]
        [Column()]
        public string AcTyp { get; set; }
        [DataMember]
        [Column()]
        public string AcNo { get; set; }
        [DataMember]
        [Column()]
        public string BankName { get; set; }
        [DataMember]
        [Column()]
        public string BrnName { get; set; }
        [DataMember]
        [Column()]
        public string IFSC { get; set; }

        [DataMember]
        [Column()]
        public string BnkCd { get; set; }

        //[DataMember]
        //[Column()]
        //public string AmountInWord { get; set; }
        [DataMember]
        [Column()]
        public decimal Amt { get; set; }
        [DataMember]
        [Column()]
        public string RefNo1 { get; set; }
        [DataMember]
        [Column()]
        public string RefNo2 { get; set; }
        [DataMember]
        [Column()]
        public string Freq { get; set; }

        [DataMember]
        [Column()]
        public DateTime? FrmPrd { get; set; }
        [DataMember]
        [Column()]
        public DateTime? ToPrd { get; set; }
        [DataMember]
        [Column()]
        public bool IsUnitCancel { get; set; }

        [DataMember]
        [Column()]
        public string PhoneNo { get; set; }
        [DataMember]
        [Column()]
        public string EMail { get; set; }

        [DataMember]
        [Column()]
        public string DetailFrntImg { get; set; }

        [DataMember]
        [Column()]
        public string FrntImg { get; set; }
        [DataMember]
        [Column()]
        public string TrnInd { get; set; }

        [DataMember]
        [Column()]
        public string MandateStatus { get; set; }

        [DataMember]
        [Column()]
        public DateTime DtStamp { get; set; }

        [DataMember]
        [Column()]
        public string Clerk { get; set; }
        [DataMember]
        [Column()]
        public string AuthUser { get; set; }
        [DataMember]
        [Column()]
        public string ReserveBy { get; set; }
        [DataMember]
        [Column()]
        public string Operation { get; set; }

        [DataMember]
        [Column()]
        public string Deleted { get; set; }


        [DataMember]
        [Column()]
        public string DebitTyp { get; set; }

        [DataMember]
        [Column()]
        public string MandateTyp { get; set; }

        [DataMember]
        [Column()]
        public string SpnsrBankName { get; set; }

        [DataMember]
        [Column()]
        public string CredtrAgentCode { get; set; }

        [DataMember]
        [Column()]
        public string CredtrAgentName { get; set; }

        [DataMember]
        [Column()]
        public string CrdtrName { get; set; }

        [DataMember]
        [Column()]
        public string Mobile { get; set; }

        [DataMember]
        [Column()]
        public string OthrDetail { get; set; }

        [DataMember]
        [Column()]
        public string DebtrCode { get; set; }

        [DataMember]
        [Column()]
        public string DebtrBnk { get; set; }

        [DataMember]
        [Column()]
        public string UserNACHId { get; set; }

        [DataMember]
        [Column]
        public string MndtReqId { get; set; }

        [DataMember]
        [Column()]
        public string Category { get; set; }

        [DataMember]
        [Column()]
        public string Proprietary { get; set; }

        [DataMember]
        [Column()]
        public string Seq { get; set; }

        [DataMember]
        [Column()]
        public string RtnRsn { get; set; }

        [DataMember]
        [Column()]
        public string Variant { get; set; }

        [DataMember]
        [Column()]
        public string NPCIFile { get; set; }

        [DataMember]
        [Column()]
        public DateTime? ReserveDt { get; set; }

        [DataMember]
        [Column()]
        public DateTime? AuthDt { get; set; }

        [DataMember]
        [Column()]
        [PrimaryKey]
        public Int32 Id { get; set; }

        [DataMember]
        [Column()]
        public string CrBrnCd { get; set; }

        [DataMember]
        [Column()]
        public string CrGlCd { get; set; }

        [DataMember]
        [Column()]
        public string CrAcNo { get; set; }

        [DataMember]
        [Column()]
        public DateTime? ExecDt { get; set; }

        [DataMember]
        [Column()]
        public string DebtrName { get; set; }

        [DataMember]
        [Column]
        public string Status { get; set; }

        [DataMember]
        [Column]
        public string Reason { get; set; }

        [DataMember]
        [Column]
        public bool IsAccept { get; set; }

        [DataMember]
        [Column()]
        public DateTime? ResDt { get; set; }

        [DataMember]
        [Column()]
        public DateTime? ReqDt { get; set; }

        [DataMember]
        [Column]
        public string ResFile { get; set; }

        [DataMember]
        [Column]
        public DateTime? DateOfMandate { get; set; }


        public GlobalIdGenerator GlobalIdGenerator
        {
            get
            {
                if (null == globalIdGeneratorService)
                {
                    globalIdGeneratorService = new GlobalIdGeneratorService();
                }
                if (globalIdGenerator == null)
                    globalIdGenerator = globalIdGeneratorService.GetMaxId(TABLE_ECSMANDATE);

                return globalIdGenerator;
            }
        }
        public GlobalIdGenerator GlobalIdGeneratorOWMandate
        {
            get
            {
                if (null == globalIdGeneratorService)
                {
                    globalIdGeneratorService = new GlobalIdGeneratorService();
                }
                if (globalIdGenerator == null)
                    globalIdGenerator = globalIdGeneratorService.GetMaxId("OWMandate");

                return globalIdGenerator;
            }
        }
        #endregion
    }
}
