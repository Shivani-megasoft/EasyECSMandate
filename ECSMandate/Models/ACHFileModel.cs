using EasyBank.BusinessLayer.Accounts.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSMandate.Models
{
    public class ACHFileModel
    {

        public ACHFileModel()
        {

        }

        #region

        public Int32 Id { get; set; }
        public string TrnCode { get; set; }
        public string UserName { get; set; }
        public string FileNumber { get; set; }
        public string FolioNumber { get; set; }
        public string SettlementDate { get; set; }
        public string FileItemSeqNo { get; set; }
        public string UserNumber { get; set; }
        public string SponsorBnkCd { get; set; }
        public string UserBnkAcNo { get; set; }
        public string TotalCount { get; set; }
        public string Cycle { get; set; }
        public string DestAcType { get; set; }
        public string LedFolio { get; set; }
        public string BenficiaryAcName { get; set; }
        public decimal Amt { get; set; }
        public string ItemSeqNo { get; set; }
        public string StatusFlag { get; set; }
        public string ResponseCd { get; set; }
        public string DestBnkCd { get; set; }
        public string BenficiaryBnkAcNo { get; set; }
        public string TranRefNo { get; set; }
        public string ProductType { get; set; }
        public string BenficiaryAadhaar { get; set; }
        public string UMRNNo { get; set; }

        public string TrnInd { get; set; }
        public Account Account { get; set; }

        public string Stat { get; set; }
        public Int32 AcId { get; set; }
        public string RCode { get; set; }
        public string RecdDt { get; set; }


        #endregion


    }
}