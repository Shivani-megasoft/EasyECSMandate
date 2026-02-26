using EasyBank.DataLayer.DataObjects;
using EASYECS.BussinessLayer.ECSService.Object;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using EasyBank.Framework.EasyExtensions;
using System.Linq;
using ECSService.Object;
using ECSService.BussinessLayer.ECSService.Object;
using System.Data;
using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.BusinessLayer.Transactions.Objects;
using EasyBank.BusinessLayer.Accounts.Objects;

namespace EASYECS.BussinessLayer.ECSService.Service
{
    public class MandateService
    {
        public void Update(ECS ecs)
        {
            if (ecs.IsNew)
                new SmartData<ECS>().Insert(ecs, "InserECSMandate");
            else
                new SmartData<ECS>().Update(ecs, "UpdateECSMandate");
        }
        public void Update(List<ECS> ecsList)
        {
            foreach (ECS ecs in ecsList)
            {
                new SmartData<ECS>().Update(ecs, "UpdateECSMandate");
            }
        }

        public void BulkUpdate(List<ECS> ecsList, List<MandateFileHistory> mandateHisList)
        {
            using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
            {
                OracleBulkDBEngine bulk = new OracleBulkDBEngine();

                var newECSList = ecsList.Where(p => p.IsNew).ToList();
                var updateECSList = ecsList.Where(p => !p.IsNew).ToList();
                if (null != newECSList && newECSList.Count > 0)
                {
                    var dataTableNew = newECSList.ToDataTable<ECS>();
                    bulk.Insert(dataTableNew, connection);
                }
                if (null != updateECSList && updateECSList.Count > 0)
                {
                    var dataTableUpdate = updateECSList.ToDataTable<ECS>();
                    bulk.Update(dataTableUpdate, "UpdateECSBatch", connection);
                }
                if (null != mandateHisList && mandateHisList.Count > 0)
                {
                    var dataTableNew = mandateHisList.ToDataTable<MandateFileHistory>();
                    bulk.Insert(dataTableNew, connection);
                }       
                connection.Close();
            }
        }
       
        public List<ECS> Reserve(string batchSize, string UserId)
        {
            List<string> stringList = new List<string>();

            stringList.Add(batchSize);
            stringList.Add(UserId);

            return new SmartData<ECS>().GetAllByCriteria(stringList, null, "ReserveECSMandate");
        }
        public List<ECS> GetReservedMandate(string UserId)
        {
            return new SmartData<ECS>().GetAllByCriteria(UserId, "GetReservedMandate");
        }

        public ECS GetById(Int32 Id)
        {
            return new SmartData<ECS>().GetById(Id.ToString(), "GetMandateById");
        }

        public List<ECS> GetMandateForVerify(string LoginId)
        {
            return new SmartData<ECS>().GetAllByCriteria(LoginId, "GetMandateForVerify");
        }
        public List<MandateFileHistory> GetAllReturnMandateHistory()
        {           
            return new SmartData<MandateFileHistory>().GetAllByCriteria("", "GetAllReturnMandateHistory");
        }

        public ECS AuthReserveECSMandate(string batchSize, string LoginId)
        {
            List<string> stringList = new List<string>();

            stringList.Add(batchSize);
            stringList.Add(LoginId);
            return new SmartData<ECS>().GetByCriteria(stringList, null, "AuthReserveECSMandate");
        }

        public List<ECS> GetUnAuthMandate(string MandateTyp)
        {
            return new SmartData<ECS>().GetAllByCriteria(MandateTyp, "GetUnAuthMandate");
        }
        public List<MandateReturnReason> GetAllReason(string RetOn, string Varient)
        {
            List<string> stringList = new List<string>();
            stringList.Add(RetOn);
            stringList.Add(Varient);
            return new SmartData<MandateReturnReason>().GetAllByCriteria(stringList, null, "GetAllReason");
        }      

        public List<H2HConfig> GetH2HConfig()
        {
            return new SmartData<H2HConfig>().GetAll("GetH2HConfig");
        }

        public ECS GetECSByFileName(string fileName)
        {
            return new SmartData<ECS>().GetByCode(fileName, "GetECSByFileName");
        }
        public MandateCategory GetCategoryByCd(string CatCode)
        {
            return new SmartData<MandateCategory>().GetByCode(CatCode, "GetCategoryByCd");
        }

        public ECS GetByUmrn(string UMRN)
        {
            List<string> stringList = new List<string>();
            stringList.Add(UMRN);
            return new SmartData<ECS>().GetByCriteria(stringList, null, "GetByUmrn");
        }
        public void UpdateHistory(MandateFileHistory manFileHis)
        {
            new SmartData<MandateFileHistory>().Insert(manFileHis, "InsertHistory");
        }

        public List<ECS> GetAllECSByStatus(string mandateStatus, DateTime FrmDate, DateTime ToDate)
        {
            List<string> stringList = new List<string>();
            stringList.Add(mandateStatus);
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate); dateList.Add(ToDate);

            return new SmartData<ECS>().GetAllByCriteria1(stringList, dateList, "GetAllECSByStatus");
        }

        public List<ECS> GetAllMandateByNPCI(string NPCIFile)
        {
            return new SmartData<ECS>().GetAllByCriteria(NPCIFile, "GetAllMandateByNPCI");
        }
        public void UpdateMandateStatusByNPCIFile(string NPCIFile, string Status)
        {
            List<string> stringList = new List<string>();
            stringList.Add(NPCIFile); stringList.Add(Status);

            new SmartData<ECS>().Update(stringList, null, "UpdateMandateStatusByNPCIFile");
        }
        public List<MandateCategory> GetAllCategory()
        {
            return new SmartData<MandateCategory>().GetAll("GetAllCategory");
        }
        public List<ECS> GetAllReturnMandate()
        {
            return new SmartData<ECS>().GetAll("GetAllReturnMandate");
        }

        public void Update(ECS ecs, MandateFileHistory manFileHis)
        {
            using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
            {
                if (ecs.IsNew)
                    new SmartData<ECS>().Insert(ecs, "InserECSMandate", connection);
                else
                    new SmartData<ECS>().Update(ecs, "UpdateECSMandate", connection);

                if (manFileHis != null)
                {
                    if (manFileHis.IsNew)
                        new SmartData<MandateFileHistory>().Insert(manFileHis, "InsertHistory");
                    else
                        new SmartData<MandateFileHistory>().Insert(manFileHis, "UpdateHistory");
                }
            }
        }        

        public ECS GetMandateByMsgId(string MsgId)
        {
            List<string> stringList = new List<string>();
            stringList.Add(MsgId);

            return new SmartData<ECS>().GetByCriteria(stringList, null, "GetMandateByMsgId");
        }

        public List<ECS> GetAllReqMandte(string Type, DateTime FrmDate, DateTime ToDate)
        {
            List<String> stringList = new List<String>();
            stringList.Add(Type);
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate);
            dateList.Add(ToDate);
            return new SmartData<ECS>().GetAllByCriteria(stringList, dateList, "GetAllReqMandte");
        } 
        
             

        public string GetEhConfigByField(string field)
        {
            var dtConfig = new SmartData<DataTable>().GetTable(field, null, "GetEhConfigByField");

            return dtConfig.Rows[0]["ConfigValue"].ToString();
        }
        public ECS GetAckMandte(int MandateId, string ReqTyp, string xmlName)
        {
            List<string> stringList = new List<string>();
            stringList.Add(MandateId.ToString()); stringList.Add(ReqTyp); stringList.Add(xmlName);

            return new SmartData<ECS>().GetByCriteria(stringList, null, "GetAckMandte");
        }

        public MandateFileHistory GetAckByMandateId(int MandateId)
        {
            List<string> stringList = new List<string>();
            stringList.Add(MandateId.ToString()); 

            return new SmartData<MandateFileHistory>().GetByCriteria(stringList, null, "GetAckByMandateId");
        }
        public List<ECS> GetAllECSByStartDay(int frmDay, int toDay)
        {
            List<string> stringList = new List<string>();

            stringList.Add(frmDay.ToString());
            stringList.Add(toDay.ToString());
            return new SmartData<ECS>().GetAllByCriteria(stringList, null, "GetAllECSByStartDay");
        }

        public List<ECS> GetAllNewECSByStartDay(int frmDay, int toDay, DateTime FrmDate, DateTime ToDate)
        {
            List<string> stringList = new List<string>();

            stringList.Add(frmDay.ToString());
            stringList.Add(toDay.ToString());
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate);
            dateList.Add(ToDate);
            return new SmartData<ECS>().GetAllByCriteria(stringList, dateList, "GetAllNewECSByStartDay");
        }
        public void UpdateIdGenerator(IdGenerator idGenerator)
        {
            //[CR74]: Changes  for CBS Merging

            new SmartData<IdGenerator>().Update(idGenerator, "UpdateIdGenerator");
        }
        public DataTable GetAllMandateCount(string criteria, DateTime AsOnDt)
        {
            return new SmartData<DataTable>().GetTable(criteria, AsOnDt, "GetAllMandateCount");
        }
        public List<ECS> GetMandateByStatus(string status, string loginId)
        {
            List<string> stringList = new List<string>();
            stringList.Add(status);
            stringList.Add(loginId);

            return new SmartData<ECS>().GetAllByCriteria(stringList, null, "GETMANDATEBYSTATUS");
        }
        public DataTable GetACHFileNames(DateTime settlementDt)
        {
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(settlementDt);
            return new SmartData<DataTable>().GetTable(null, dateList, "GetACHFileNames");
        }

        public DataTable GetACHByFileName(string fileName)
        {
            List<string> stringList = new List<string>();
            stringList.Add(fileName);
            return new SmartData<DataTable>().GetTable(stringList, null, "GetACHByFileName");
        }
        public List<EBTCredit> GetAllACHByByStatus(string trnCode, string fileName, string status,  DateTime processDt)
        {
            List<string> stringList = new List<string>();
            List<DateTime> dateList = new List<DateTime>();
            stringList.Add(trnCode); stringList.Add(fileName);
            stringList.Add(status); dateList.Add(processDt);

            return new SmartData<EBTCredit>().GetAllByCriteria(stringList, dateList, "GetAllACHByByStatus");
        }
        public List<EBTCredit> RESERVEACHCREDIT(string trnCode, DateTime trnDt, string fileName, string clerk, int noOfRecords)
        {
            List<string> stringlist = new List<string>();
            stringlist.Add(trnCode); stringlist.Add(fileName);
            stringlist.Add(clerk); stringlist.Add("100");
            List<DateTime> datelist = new List<DateTime>();
            datelist.Add(trnDt);
            return new SmartData<EBTCredit>().GetAllByCriteria(stringlist, datelist, "RESERVEACHCREDIT");
        }
        public EBTCredit GetByItemSeqNo(DateTime trnDate, string ItemSeqNo)
        {
            return new SmartData<EBTCredit>().GetByCriteria(ItemSeqNo, trnDate, "GetACHItemSeqNo");
        }

        public List<EBTCredit> GetAllACHCreditByType(string type, DateTime asOnDt)
        {
            List<DateTime> dateList = new List<DateTime>();
            List<string> stringList = new List<string>();
            dateList.Add(asOnDt); stringList.Add(type);
            return new SmartData<EBTCredit>().GetAllByCriteria(stringList, dateList, "GetAllACHCreditByType");
        }

        //public void UpdateInpHistory(List<InputFileHistory> inputFile)
        //{
        //    using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
        //    {
        //        foreach (InputFileHistory file in inputFile)    
        //        {
        //            if (file.IsNew)
        //                new SmartData<InputFileHistory>().Insert(file, "InsertInpHistory", connection);
        //            else
        //                new SmartData<InputFileHistory>().Update(file, "UpdateInpHistory", connection);
        //        }
        //    }
        //}
        public void BulkUpdate(List<DailyTransaction> dailyTransactionList, List<MainBalance> mainBalanceList,
             List<EBTCredit> ebtCreditList)
        {
            using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
            {
                //using (var tx = connection.BeginTransaction())
                //{
                OracleBulkDBEngine bulk = new OracleBulkDBEngine();
                if (mainBalanceList != null && mainBalanceList.Count > 0)
                {
                    var dataTableUpdate = mainBalanceList.ToDataTable<MainBalance>();
                    bulk.Update(dataTableUpdate, "UpdateMainBalanceBatch", connection);
                }
                if (dailyTransactionList != null)
                {
                    var newDailyTransactionList = dailyTransactionList.Where(p => p.IsNew).ToList();
                    var updateDailyTransactionList = dailyTransactionList.Where(p => !p.IsNew).ToList();

                    if (null != newDailyTransactionList && newDailyTransactionList.Count > 0)
                    {
                        var dataTableNew = newDailyTransactionList.ToDataTable<DailyTransaction>();
                        bulk.Insert(dataTableNew, connection);
                    }

                    if (null != updateDailyTransactionList && updateDailyTransactionList.Count > 0)
                    {
                        var dataTableUpdate = updateDailyTransactionList.ToDataTable<DailyTransaction>();
                        bulk.Update(dataTableUpdate, "UpdateDailyTransactionBatch", connection);
                    }
                }

                if (ebtCreditList != null && ebtCreditList.Count > 0)
                {
                    var newebtCreditList = ebtCreditList.Where(p => p.IsNew).ToList();
                    var updateebtCreditList = ebtCreditList.Where(p => !p.IsNew).ToList();

                    if (null != newebtCreditList && newebtCreditList.Count > 0)
                    {
                        var dataTableNew = newebtCreditList.ToDataTable<EBTCredit>();
                        bulk.Insert(dataTableNew, connection);
                    }

                    if (null != updateebtCreditList && updateebtCreditList.Count > 0)
                    {
                        var dataTableUpdate = updateebtCreditList.ToDataTable<EBTCredit>();
                        bulk.Update(dataTableUpdate, "UpdateEBTCreditBatch", connection);
                    }
                }
            }
        }
        //public void UpdateInpHistory(List<MandateInstruction> inputFile)
        //{
        //    foreach (MandateInstruction file in inputFile)
        //    {
        //        if (file.IsNew)
        //            new SmartData<MandateInstruction>().Insert(file, "InsertACHInputFile");
        //        else
        //            new SmartData<MandateInstruction>().Update(file, "UpdateACHInputFile");
        //    }
        //}

        //public void Update(MandateInstruction inputFile)
        //{
        //    if (inputFile.IsNew)
        //        new SmartData<MandateInstruction>().Insert(inputFile, "InsertACHInputFile");
        //    else
        //        new SmartData<MandateInstruction>().Update(inputFile, "UpdateACHInputFile");
        //}


        public List<ECS> GetAllUnExecutedECS(DateTime dueDate)
        {
            List<string> stringList = new List<string>();
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(dueDate);

            return new SmartData<ECS>().GetAllByCriteria(null, dateList, "GetAllUnExecutedECS");
        }

        public MandateInstruction GetByMndtId(int MandateId)
        {
            List<string> stringList = new List<string>();
            stringList.Add(MandateId.ToString());

            return new SmartData<MandateInstruction>().GetByCriteria(stringList, null, "GetAchInputDataByMndtId");
        }
        public List<MandateReturnReason> GetReasonByReasonCode(string RetCode)
        {       
           return new SmartData<MandateReturnReason>().GetAllByCriteria(RetCode, "GetReasonByReasonCode");
        }

        public List<MandateInstruction> GetAllECSByFrmDtAndToDt(int frmDay, int toDay, DateTime FrmDate, DateTime ToDate)
        {
            List<string> stringList = new List<string>();

            stringList.Add(frmDay.ToString());
            stringList.Add(toDay.ToString());
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate);
            dateList.Add(ToDate);
            return new SmartData<MandateInstruction>().GetAllByCriteria(stringList, dateList, "GetAllECSByFrmDtAndToDt");
        }


        public List<MandateInstruction> GetAllECSByINPFILENAME(int frmDay, int toDay, DateTime FrmDate, DateTime ToDate)
        {
            List<string> stringList = new List<string>();

            stringList.Add(frmDay.ToString());
            stringList.Add(toDay.ToString());
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate);
            dateList.Add(ToDate);
            return new SmartData<MandateInstruction>().GetAllByCriteria(stringList, dateList, "GetAllECSByINPFILENAME");
        }


        public MandateInstruction GetInstructionByMndtIdandDmdDt(int MndtId, DateTime DmdDt)
        {
            List<string> stringList = new List<string>();
            stringList.Add(MndtId.ToString());
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(DmdDt);
            return new SmartData<MandateInstruction>().GetByCriteria(stringList, dateList, "GetInstructionByMndtIdandDmdDt");
        }

        public void Update(MandateInstruction mandateinstruction)
        {
            if (mandateinstruction.IsNew)
                new SmartData<MandateInstruction>().Insert(mandateinstruction, "InsertMandateInstruction");
            else
                new SmartData<MandateInstruction>().Update(mandateinstruction, "UpdateMandateInstruction");
        }

        public List<ECSHistory> GetECSHistoryByTrnInd(string TrnInd, string MandateTyp)
        {
            List<DateTime> dateList = new List<DateTime>();
            List<string> stringList = new List<string>();
            stringList.Add(TrnInd);
            stringList.Add(MandateTyp);
            return new SmartData<ECSHistory>().GetAllByCriteria(stringList, null, "GetECSHistoryByTrnInd");
        }

        public MandateInstruction GetMandateInstructionByRefNo(string RefNo)
        {
            List<string> stringList = new List<string>();
            stringList.Add(RefNo);
            return new SmartData<MandateInstruction>().GetByCriteria(stringList, null, "GetMandateInstructionByRefNo");
        }

        public List<EBTCredit> GetAllMandteResponse( DateTime FrmDate, DateTime ToDate)
        {
            List<String> stringList = new List<String>();
        
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate);
            dateList.Add(ToDate);
            return new SmartData<EBTCredit>().GetAllByCriteria(null, dateList, "GetAllMandteResponse");
        }
        public void Update(ECSHistory ecsHistory)
        {
            if (ecsHistory.IsNew)
                new SmartData<ECSHistory>().Insert(ecsHistory, "InsertECSMandateHistory");
            else
                new SmartData<ECSHistory>().Update(ecsHistory, "UpdateECSMandateHistory");
        }

        public ECSHistory GetMandateHistoryById(Int32 Id)
        {
            return new SmartData<ECSHistory>().GetById(Id.ToString(), "GetMandateHistoryById");
        }

        public void UpdateEcsHistory(ECS ecs, ECSHistory ecsHistory)
        {
            using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
            {
                if (ecs != null)
                {
                    if (ecs.IsNew)
                        new SmartData<ECS>().Insert(ecs, "InserECSMandate", connection);
                    else
                        new SmartData<ECS>().Update(ecs, "UpdateECSMandate", connection);
                }
                if (ecsHistory != null)
                {
                    if (ecsHistory.IsNew)
                        new SmartData<ECSHistory>().Insert(ecsHistory, "InsertECSMandateHistory", connection);
                    else
                        new SmartData<ECSHistory>().Insert(ecsHistory, "UpdateECSMandateHistory", connection);
                }
            }
        }

        public void Update(ECS ecs, ECSHistory ecsHistory, ECSAuditLog eCSAuditLog)
        {
            using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
            {
                if (ecs != null)
                {
                    if (ecs.IsNew)
                        new SmartData<ECS>().Insert(ecs, "InserECSMandate", connection);
                    else
                        new SmartData<ECS>().Update(ecs, "UpdateECSMandate", connection);
                }
                if (ecsHistory != null)
                {
                    if (ecsHistory.IsNew)
                        new SmartData<ECSHistory>().Insert(ecsHistory, "InsertECSMandateHistory", connection);
                    else
                        new SmartData<ECSHistory>().Insert(ecsHistory, "UpdateECSMandateHistory", connection);
                }
                if (eCSAuditLog != null)
                {
                    if (eCSAuditLog.IsNew)
                        new SmartData<ECSAuditLog>().Insert(eCSAuditLog, "InsertECSAuditLog", connection);
                    else
                        new SmartData<ECSAuditLog>().Insert(eCSAuditLog, "UpdateECSAuditLog", connection);
                }
            }
        }

        public void Update(ECSAuditLog eCSAuditLog)
        {
            if (eCSAuditLog.IsNew)
                new SmartData<ECSAuditLog>().Insert(eCSAuditLog, "InsertECSAuditLog");
            else
                new SmartData<ECSAuditLog>().Update(eCSAuditLog, "UpdateECSAuditLog");
        }
        public ECS GetMandateByMndtId(Int32 Id)
        {
            return new SmartData<ECS>().GetById(Id.ToString(), "GetMandateByMndtId");
        }


        public void Update(ECS ecs, MandateInstruction mandateInstruction)
        {
            using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
            {
                if (ecs != null)
                {
                    if (ecs.IsNew)
                        new SmartData<ECS>().Insert(ecs, "InserECSMandate", connection);
                    else
                        new SmartData<ECS>().Update(ecs, "UpdateECSMandate", connection);
                }
                if (mandateInstruction != null)
                {
                    if (mandateInstruction.IsNew)
                        new SmartData<MandateInstruction>().Insert(mandateInstruction, "InsertMandateInstruction", connection);
                    else
                        new SmartData<MandateInstruction>().Insert(mandateInstruction, "UpdateMandateInstruction", connection);
                }
            }
        }

        public void BulkUpdate(List<ECS> ecsList, List<MandateFileHistory> mandateHisList, List<ECSHistory> ecsHistoryList)
        {
            using (OracleConnection connection = DBEngine.CreateAndOpenConnection())
            {
                OracleBulkDBEngine bulk = new OracleBulkDBEngine();
                var newECSList = ecsList.Where(p => p.IsNew).ToList();
                var updateECSList = ecsList.Where(p => !p.IsNew).ToList();
                if (null != newECSList && newECSList.Count > 0)
                {
                    var dataTableNew = newECSList.ToDataTable<ECS>();
                    bulk.Insert(dataTableNew, connection);
                }
                if (null != updateECSList && updateECSList.Count > 0)
                {
                    var dataTableUpdate = updateECSList.ToDataTable<ECS>();
                    bulk.Update(dataTableUpdate, "UpdateECSBatch", connection);
                }
                if (null != mandateHisList && mandateHisList.Count > 0)
                {
                    var dataTableNew = mandateHisList.ToDataTable<MandateFileHistory>();
                    bulk.Insert(dataTableNew, connection);
                }
                if (null != ecsHistoryList && ecsHistoryList.Count > 0)
                {
                    var dataTableNew = ecsHistoryList.ToDataTable<ECSHistory>();
                    bulk.Insert(dataTableNew, connection);
                }

                connection.Close();
            }
        }

        public List<ECS> GetAllReqMandtebystatus(string Type, DateTime FrmDate, DateTime ToDate,string Status)
        {
            List<String> stringList = new List<String>();
            stringList.Add(Type);
            stringList.Add(Status);
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate);
            dateList.Add(ToDate);
            return new SmartData<ECS>().GetAllByCriteria(stringList, dateList, "GetAllReqMandtebystatus");
        }


        public List<ECS> GetAllECSByStatusandDT(string mandatetyp, string MandateStatus, DateTime FrmDate, DateTime ToDate)
        {
            List<string> stringList = new List<string>();
            stringList.Add(mandatetyp);
            stringList.Add(MandateStatus);
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(FrmDate);
            dateList.Add(ToDate);

            return new SmartData<ECS>().GetAllByCriteria(stringList, dateList, "GetAllECSByStatusandDT");
        }




    }
}