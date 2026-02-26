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
using EasyBank.BusinessLayer.Accounts.Services;

using System.Text.RegularExpressions;
using EasyBank.SystemOperations.Objects;

namespace EASYECS.BussinessLayer.ECSService.Service
{
    public class SMSService
    {
        private void InsertSms(string mobileNo, string message, Int32 acid)
        {
            SmsMessage smsMessage = new SmsMessage();
          
            smsMessage.Message = message;
            smsMessage.MobileNo = "91" + mobileNo;
            smsMessage.Status = "Failed";
            smsMessage.Date_ = DateTime.Now;
            smsMessage.TrnId = 0;
            smsMessage.AcId = acid;
            smsMessage.AppId = "ECS";


            new SmartData<SmsMessage>().Insert(smsMessage, "InsertSms");
        }

        public void SendSms(SMS sms,string status)
        {
            string templateFile = string.Empty;
            const string token = @"{([^}]+)}";


            try
            {
                templateFile = new AccountSMSConfigService().GetSmsTemplateByType(status).Template;
                var text = templateFile;
                var message = Regex.Replace(text, token, match => ProcessTokens(match.Groups[1].Value, sms));
                InsertSms(sms.MobNo, message, sms.AcId);
            }
            catch (Exception ex)
            {
                //EasyLogger.Log(ex.GetDetails(), "General", 1, System.Diagnostics.TraceEventType.Error);
            }
        }


        private string ProcessTokens(string token, SMS sms)
        {
            string result = "";
            switch (token)
            {

                case "Amt":
                    result = Convert.ToDecimal(sms.Amt).ToString("0.00");
                    break;

                case "ChqNo":
                    result = sms.ChqNo;
                    break;
                case "AcName":
                    result = sms.Name;
                    break;
                case "AcNo":
                    result = sms.AcNo.Substring(sms.AcNo.Length - 6);
                    break;
                case "Reason":
                    result = sms.reason;
                    break;
            }
            return result;
        }
    }
}