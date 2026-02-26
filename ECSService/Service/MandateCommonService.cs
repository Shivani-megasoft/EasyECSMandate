using EasyBank.BusinessLayer.Accounts.Objects;
using EasyBank.BusinessLayer.Accounts.Services;
using EasyBank.BusinessLayer.Base.Contracts;
using EasyBank.BusinessLayer.Base.Objects;
using EasyBank.BusinessLayer.Base.Services;
using EasyBank.BusinessLayer.HOAdmin.Objects;
using EasyBank.BusinessLayer.HOAdmin.Services;
using EasyBank.Infrastructure.SystemParameters;
using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ECSService.Service
{
    public class MandateCommonService
    {
        static IStartUp StartUp = StartupManager.StartUp;

        MandateService mandateService = new MandateService();
        public XmlDocument CreateXML(ECS mandate)
        {
            string mndtReqId = mandate.MsgId;

            XmlDocument doc = new XmlDocument();

            XmlDeclaration docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlNode documentNode = doc.CreateElement("Document");
            documentNode.Attributes.Append(documentNode.OwnerDocument.CreateAttribute("xmlns"));
            documentNode.Attributes["xmlns"].Value = "urn:iso:std:iso:20022:tech:xsd:pain.009.001.01";

            XmlNode productsNode = doc.CreateElement("MndtInitnReq");

            XmlNode grpNode = doc.CreateElement("GrpHdr");

            XmlNode msgNode = doc.CreateElement("MsgId");
            msgNode.InnerText = mndtReqId;

            grpNode.AppendChild(msgNode);

            XmlNode creNode = doc.CreateElement("CreDtTm");
            creNode.InnerText = DateTime.Now.ToString("s");
            grpNode.AppendChild(creNode);


            //Instructing Agent
            XmlNode instNode = doc.CreateElement("InstgAgt");
            XmlNode finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysNode = doc.CreateElement("ClrSysMmbId");
            XmlNode memIdNode = doc.CreateElement("MmbId");
            memIdNode.InnerText = new MandateService().GetEhConfigByField("IFSC_CD");

            clrSysNode.AppendChild(memIdNode);
            finInstNode.AppendChild(clrSysNode);
            XmlNode nmNode = doc.CreateElement("Nm");
            nmNode.InnerText = StartUp.BNK_NAME;
            finInstNode.AppendChild(nmNode);

            instNode.AppendChild(finInstNode);
            grpNode.AppendChild(instNode);

            //Instructed Agent
            XmlNode instdNode = doc.CreateElement("InstdAgt");
            XmlNode instdFinNode = doc.CreateElement("FinInstnId");
            XmlNode instdNodeclrSysNode = doc.CreateElement("ClrSysMmbId");
            XmlNode instdNodeMemIdNode = doc.CreateElement("MmbId");
            instdNodeMemIdNode.InnerText = mandate.BnkCd;

            instdNodeclrSysNode.AppendChild(instdNodeMemIdNode);
            instdFinNode.AppendChild(instdNodeclrSysNode);

            XmlNode instdNmNode = doc.CreateElement("Nm");
            instdNmNode.InnerText = mandate.BankName;
            instdFinNode.AppendChild(instdNmNode);
            instdNode.AppendChild(instdFinNode);

            grpNode.AppendChild(instdNode);
            productsNode.AppendChild(grpNode);

            //Start Mndt tag
            XmlNode mndtNode = doc.CreateElement("Mndt");

            XmlNode mndtReqIdNode = doc.CreateElement("MndtReqId");
            mndtReqIdNode.InnerText = mndtReqId;
            mndtNode.AppendChild(mndtReqIdNode);

            XmlNode tpNode = doc.CreateElement("Tp");
            XmlNode svcNode = doc.CreateElement("SvcLvl");
            XmlNode prtryNode = doc.CreateElement("Prtry");
            prtryNode.InnerText = mandate.Category;
            svcNode.AppendChild(prtryNode);
            tpNode.AppendChild(svcNode);

            XmlNode lclInstrmNode = doc.CreateElement("LclInstrm");
            XmlNode lclInstrmPrtNode = doc.CreateElement("Prtry");
            lclInstrmPrtNode.InnerText = "DEBIT";

            lclInstrmNode.AppendChild(lclInstrmPrtNode);
            tpNode.AppendChild(lclInstrmNode);
            mndtNode.AppendChild(tpNode);

            XmlNode ocrnNode = doc.CreateElement("Ocrncs");
            XmlNode seqTpNode = doc.CreateElement("SeqTp");
            seqTpNode.InnerText = mandate.Seq;
            ocrnNode.AppendChild(seqTpNode);

            XmlNode frqNode = doc.CreateElement("Frqcy");
            frqNode.InnerText = mandate.Freq;
            ocrnNode.AppendChild(frqNode);

            XmlNode DrtnNode = doc.CreateElement("Drtn");
            XmlNode FrDtNode = doc.CreateElement("FrDt");
            FrDtNode.InnerText = mandate.DateOfMandate == null ? "" : Convert.ToDateTime(mandate.DateOfMandate).ToString("yyyy-MM-dd");
            DrtnNode.AppendChild(FrDtNode);
            ocrnNode.AppendChild(DrtnNode);

            XmlNode frstCollNode = doc.CreateElement("FrstColltnDt");
            frstCollNode.InnerText = Convert.ToDateTime(mandate.FrmPrd).ToString("yyyy-MM-dd");
            ocrnNode.AppendChild(frstCollNode);

            XmlNode fnlCollNode = doc.CreateElement("FnlColltnDt");
            fnlCollNode.InnerText = mandate.ToPrd == null ? "" : Convert.ToDateTime(mandate.ToPrd).ToString("yyyy-MM-dd");
            ocrnNode.AppendChild(fnlCollNode);
            mndtNode.AppendChild(ocrnNode);

            //ColltnAmt also may be present
            XmlNode maxAmtNode = doc.CreateElement("ColltnAmt");
            XmlAttribute amtAttr = doc.CreateAttribute("Ccy");
            amtAttr.Value = "INR";
            maxAmtNode.Attributes.Append(amtAttr);
            maxAmtNode.InnerText = mandate.Amt.ToString();
            mndtNode.AppendChild(maxAmtNode);

           // Account account = AccountService.GetByNo(mandate.CrBrnCd, mandate.CrGlCd, mandate.CrAcNo);
            Account account = AccountService.GetByNo( mandate.CrAcNo);

            //string customerName = account.Name;
            //customerName = customerName.Length > 39 ? customerName.Substring(0, 39) : customerName;
            XmlNode cdtrNode = doc.CreateElement("Cdtr");
            XmlNode cdNmNode = doc.CreateElement("Nm");
            //cdNmNode.InnerText = customerName;// mandate.CrdtrName;
            cdNmNode.InnerText = StartUp.BNK_NAME;
            cdtrNode.AppendChild(cdNmNode);
            mndtNode.AppendChild(cdtrNode);

            XmlNode cdtrAccNode = doc.CreateElement("CdtrAcct");
            XmlNode idNode = doc.CreateElement("Id");
            XmlNode OthrNode = doc.CreateElement("Othr");
            XmlNode OthrIdNode = doc.CreateElement("Id");
            OthrIdNode.InnerText = mandate.UtilityCd;
            OthrNode.AppendChild(OthrIdNode);
            idNode.AppendChild(OthrNode);
            cdtrAccNode.AppendChild(idNode);
            mndtNode.AppendChild(cdtrAccNode);

            XmlNode cdtrAgtNode = doc.CreateElement("CdtrAgt");
            XmlNode finInstIdNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysMmbIdNode = doc.CreateElement("ClrSysMmbId");
            XmlNode mmbIdNode = doc.CreateElement("MmbId");
            mmbIdNode.InnerText = new MandateService().GetEhConfigByField("IFSC_CD");// mandate.CredtrAgentCode; new change
            clrSysMmbIdNode.AppendChild(mmbIdNode);
            finInstIdNode.AppendChild(clrSysMmbIdNode);
            XmlNode nm = doc.CreateElement("Nm");
            nm.InnerText = mandate.CredtrAgentName;
            finInstIdNode.AppendChild(nm);
            cdtrAgtNode.AppendChild(finInstIdNode);
            mndtNode.AppendChild(cdtrAgtNode);

            string dbtrName = mandate.DebtrName.Length > 39 ? mandate.DebtrName.Substring(0, 39) : mandate.DebtrName;
            XmlNode dbtrNode = doc.CreateElement("Dbtr");
            XmlNode dbtrNmNode = doc.CreateElement("Nm");
            dbtrNmNode.InnerText = dbtrName;
            dbtrNode.AppendChild(dbtrNmNode);

            XmlNode dbtrIdNode = doc.CreateElement("Id");
            XmlNode prvtIdNode = doc.CreateElement("PrvtId");
            XmlNode othrNode = doc.CreateElement("Othr");
            XmlNode othrIdNode = doc.CreateElement("Id");
            othrIdNode.InnerText = mandate.Mobile;
            othrNode.AppendChild(othrIdNode);

            XmlNode schmeNode = doc.CreateElement("SchmeNm");
            XmlNode schPrtryNode = doc.CreateElement("Prtry");
            schPrtryNode.InnerText = mandateService.GetAllCategory().Where(a => a.CatCode == mandate.Category).FirstOrDefault().CatCode;
            schmeNode.AppendChild(schPrtryNode);

            othrNode.AppendChild(schmeNode);
            prvtIdNode.AppendChild(othrNode);
            dbtrIdNode.AppendChild(prvtIdNode);
            dbtrNode.AppendChild(dbtrIdNode);

            XmlNode ctctDtNode = doc.CreateElement("CtctDtls");
            if (!string.IsNullOrEmpty(mandate.Mobile))
            {
                XmlNode phoneNode = doc.CreateElement("PhneNb");
                phoneNode.InnerText = "+91-" + mandate.Mobile; //mandate.PhoneNo;
                ctctDtNode.AppendChild(phoneNode);

                XmlNode mobNode = doc.CreateElement("MobNb");
                mobNode.InnerText = "+91-" + mandate.Mobile;
                ctctDtNode.AppendChild(mobNode);
            }

            if (!string.IsNullOrEmpty(mandate.EMail.Trim()))
            {
                XmlNode emailNode = doc.CreateElement("EmailAdr");
                emailNode.InnerText = mandate.EMail;
                ctctDtNode.AppendChild(emailNode);
            }

            XmlNode ctctOthrNode = doc.CreateElement("Othr");
            ctctOthrNode.InnerText = mandate.OthrDetail;
            ctctDtNode.AppendChild(ctctOthrNode);
            dbtrNode.AppendChild(ctctDtNode);
            mndtNode.AppendChild(dbtrNode);

            XmlNode dbtrAcctNode = doc.CreateElement("DbtrAcct");//DbtrAcct start
            XmlNode dbtrAcctIdNode = doc.CreateElement("Id");
            XmlNode dbtrAcctOthrNode = doc.CreateElement("Othr");
            OthrIdNode = doc.CreateElement("Id");
            OthrIdNode.InnerText = mandate.AcNo;
            dbtrAcctOthrNode.AppendChild(OthrIdNode);
            dbtrAcctIdNode.AppendChild(dbtrAcctOthrNode);
            dbtrAcctNode.AppendChild(dbtrAcctIdNode);

            XmlNode dbtrTpNode = doc.CreateElement("Tp");
            XmlNode dbtrPrtry = doc.CreateElement("Prtry");
            dbtrPrtry.InnerText = mandate.AcTyp;
            dbtrTpNode.AppendChild(dbtrPrtry);
            dbtrAcctNode.AppendChild(dbtrTpNode);
            mndtNode.AppendChild(dbtrAcctNode); //DbtrAcct END

            XmlNode dbtrAgtNode = doc.CreateElement("DbtrAgt");//DbtrAgt start
            finInstIdNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysMemNode = doc.CreateElement("ClrSysMmbId");
            mmbIdNode = doc.CreateElement("MmbId");
            mmbIdNode.InnerText = mandate.BnkCd.ToUpper();// mandate.DebtrCode; change
            clrSysMemNode.AppendChild(mmbIdNode);
            finInstIdNode.AppendChild(clrSysMemNode);
            dbtrAgtNode.AppendChild(finInstIdNode);
            mndtNode.AppendChild(dbtrAgtNode); //DbtrAgt END           

            productsNode.AppendChild(mndtNode);
            documentNode.AppendChild(productsNode);
            doc.AppendChild(documentNode);
            return doc;
        }

        public static XmlDocument CreateAccept(ECS mandate, bool isAccept)
        {
            //Create XML file

            XmlDocument doc = new XmlDocument();
            //XmlDeclaration docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", "");         
            //doc.AppendChild(docNode);

            XmlNode documentNode = doc.CreateElement("Document");

            documentNode.Attributes.Append(documentNode.OwnerDocument.CreateAttribute("xmlns"));
            documentNode.Attributes["xmlns"].Value = "urn:iso:std:iso:20022:tech:xsd:pain.012.001.01";

            XmlNode productsNode = doc.CreateElement("MndtAccptncRpt");
            //doc.AppendChild(productsNode);

            XmlNode grpNode = doc.CreateElement("GrpHdr");
            //productsNode.AppendChild(productNode);

            XmlNode msgNode = doc.CreateElement("MsgId");//
            msgNode.InnerText = mandate.MsgId;

            grpNode.AppendChild(msgNode);

            XmlNode creNode = doc.CreateElement("CreDtTm");
            creNode.InnerText = DateTime.Now.ToString("s");
            grpNode.AppendChild(creNode);

            //Commnet as discusstion with manish
            //if (!string.IsNullOrEmpty(mandate.UserNACHId) && isAccept)
            //{
            //    XmlNode initNode = doc.CreateElement("InitgPty");
            //    XmlNode initId = doc.CreateElement("Id");
            //    XmlNode initOrg = doc.CreateElement("OrgId");
            //    XmlNode initOthr = doc.CreateElement("Othr");
            //    XmlNode othrId = doc.CreateElement("Id");
            //    othrId.InnerText = mandate.UserNACHId;

            //    initOthr.AppendChild(othrId);
            //    initOrg.AppendChild(initOthr);
            //    initId.AppendChild(initOrg);
            //    initNode.AppendChild(initId);
            //    grpNode.AppendChild(initNode);
            //}

            //Instructing Agent
            XmlNode instNode = doc.CreateElement("InstgAgt");
            XmlNode finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysNode = doc.CreateElement("ClrSysMmbId");
            XmlNode memIdNode = doc.CreateElement("MmbId");
            memIdNode.InnerText = mandate.BnkCd;

            clrSysNode.AppendChild(memIdNode);
            XmlNode name = doc.CreateElement("Nm");
            name.InnerText = mandate.BankName;

            finInstNode.AppendChild(clrSysNode);
            finInstNode.AppendChild(name);
            instNode.AppendChild(finInstNode);
            grpNode.AppendChild(instNode);

            //Instructed Agent
            XmlNode instdAgtNode = doc.CreateElement("InstdAgt");
            finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysMmbNode = doc.CreateElement("ClrSysMmbId");
            XmlNode mmbIdNode = doc.CreateElement("MmbId");
            mmbIdNode.InnerText = mandate.SpnsrBnkCd;

            clrSysMmbNode.AppendChild(mmbIdNode);
            XmlNode spnrName = doc.CreateElement("Nm");
            spnrName.InnerText = mandate.SpnsrBankName;

            finInstNode.AppendChild(clrSysMmbNode);
            finInstNode.AppendChild(spnrName);

            instdAgtNode.AppendChild(finInstNode);
            grpNode.AppendChild(instdAgtNode);
            productsNode.AppendChild(grpNode);
            //END GrpHdr tag

            //Underlying Amendment Details
            XmlNode underAccp = doc.CreateElement("UndrlygAccptncDtls");
            XmlNode msgInfo = doc.CreateElement("OrgnlMsgInf");
            XmlNode msgId = doc.CreateElement("MsgId");
            msgId.InnerText = mandate.MndtReqId;
            msgInfo.AppendChild(msgId);

            XmlNode msgNmId = doc.CreateElement("MsgNmId");
            msgNmId.InnerText = "pain.009.001.01"; //Dont know...need to discuss
            msgInfo.AppendChild(msgNmId);

            XmlNode creDt = doc.CreateElement("CreDtTm");
            creDt.InnerText = DateTime.Now.ToString("s");
            msgInfo.AppendChild(creDt);
            underAccp.AppendChild(msgInfo);

            //Acceptance Result
            XmlNode accResult = doc.CreateElement("AccptncRslt");
            XmlNode accpt = doc.CreateElement("Accptd");
            accpt.InnerText = isAccept.ToString().ToLower(); //need to chk
            accResult.AppendChild(accpt);

            XmlNode rejRsn = doc.CreateElement("RjctRsn");
            XmlNode code = doc.CreateElement("Prtry");
            code.InnerText = mandate.RtnRsn;//rejection/Acceptance code
            rejRsn.AppendChild(code);
            accResult.AppendChild(rejRsn);
            underAccp.AppendChild(accResult);

            XmlNode orgnMndt = doc.CreateElement("OrgnlMndt");
            XmlNode orgnMndtId = doc.CreateElement("OrgnlMndtId");
            orgnMndtId.InnerText = mandate.UMRNo;
            orgnMndt.AppendChild(orgnMndtId);
            underAccp.AppendChild(orgnMndt);
            productsNode.AppendChild(underAccp);
            documentNode.AppendChild(productsNode);
            //doc.AppendChild(productsNode);

            doc.AppendChild(documentNode);
            //doc.InsertBefore(docNode, documentNode);
            return doc;
        }

        public XmlDocument MandateCancelXML(ECS mandate)
        {
            //pending XML document           
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode documentNode = doc.CreateElement("Document");
            documentNode.Attributes.Append(documentNode.OwnerDocument.CreateAttribute("xmlns"));
            documentNode.Attributes["xmlns"].Value = "urn:iso:std:iso:20022:tech:xsd:pain.012.001.01";

            XmlNode mndtCxlReq = doc.CreateElement("MndtCxlReq");
            XmlNode grpNode = doc.CreateElement("GrpHdr");

            XmlNode msgNode = doc.CreateElement("MsgId");//
            msgNode.InnerText = MandateCommonService.CreateMsgId();
            grpNode.AppendChild(msgNode);

            XmlNode creNode = doc.CreateElement("CreDtTm");
            creNode.InnerText = DateTime.Now.ToString("s");
            grpNode.AppendChild(creNode);

            //if (!string.IsNullOrEmpty(mandate.UserNACHId))
            //{
            //    XmlNode initNode = doc.CreateElement("InitgPty");
            //    XmlNode initId = doc.CreateElement("Id");
            //    XmlNode initOrg = doc.CreateElement("OrgId");
            //    XmlNode initOthr = doc.CreateElement("Othr");
            //    XmlNode othrId = doc.CreateElement("Id");
            //    othrId.InnerText = mandate.UserNACHId;

            //    initOthr.AppendChild(othrId);
            //    initOrg.AppendChild(initOthr);
            //    initId.AppendChild(initOrg);
            //    initNode.AppendChild(initId);
            //    grpNode.AppendChild(initNode);
            //}
            //Instructing Agent
            XmlNode instNode = doc.CreateElement("InstgAgt");
            XmlNode finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysNode = doc.CreateElement("ClrSysMmbId");
            XmlNode memIdNode = doc.CreateElement("MmbId");
            memIdNode.InnerText = mandate.SpnsrBnkCd;

            clrSysNode.AppendChild(memIdNode);
            XmlNode name = doc.CreateElement("Nm");
            name.InnerText = mandate.SpnsrBankName;

            finInstNode.AppendChild(clrSysNode);
            finInstNode.AppendChild(name);
            instNode.AppendChild(finInstNode);
            grpNode.AppendChild(instNode);

            //Instructed Agent
            XmlNode instdAgtNode = doc.CreateElement("InstdAgt");
            finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysMmbNode = doc.CreateElement("ClrSysMmbId");
            XmlNode mmbIdNode = doc.CreateElement("MmbId");
            mmbIdNode.InnerText = mandate.BnkCd;

            clrSysMmbNode.AppendChild(mmbIdNode);
            XmlNode spnrName = doc.CreateElement("Nm");
            spnrName.InnerText = mandate.BankName;

            finInstNode.AppendChild(clrSysMmbNode);
            finInstNode.AppendChild(spnrName);

            instdAgtNode.AppendChild(finInstNode);
            grpNode.AppendChild(instdAgtNode);
            mndtCxlReq.AppendChild(grpNode);
            //END GrpHdr tag

            //Underlying Amendment Details
            XmlNode underCancel = doc.CreateElement("UndrlygCxlDtls");
            XmlNode cxlRsn = doc.CreateElement("CxlRsn");
            XmlNode rsn = doc.CreateElement("Rsn");
            XmlNode prtNode = doc.CreateElement("Prtry");

            prtNode.InnerText = mandate.RtnRsn;//mandate.MndtReqId;  mandate return reason should provided
            rsn.AppendChild(prtNode);
            cxlRsn.AppendChild(rsn);
            underCancel.AppendChild(cxlRsn);

            XmlNode orgnMndt = doc.CreateElement("OrgnlMndt");

            XmlNode orgnMndtId = doc.CreateElement("OrgnlMndtId");
            orgnMndtId.InnerText = mandate.UMRNo;
            orgnMndt.AppendChild(orgnMndtId);
            underCancel.AppendChild(orgnMndtId);
            mndtCxlReq.AppendChild(underCancel);
            documentNode.AppendChild(mndtCxlReq);
            doc.AppendChild(documentNode);
            return doc;
        }

        public XmlDocument MandateAmendXML(ECS mandate)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode documentNode = doc.CreateElement("Document");
            documentNode.Attributes.Append(documentNode.OwnerDocument.CreateAttribute("xmlns"));
            documentNode.Attributes["xmlns"].Value = "urn:iso:std:iso:20022:tech:xsd:pain.010.001.01";

            XmlNode mndtAmdReqNode = doc.CreateElement("MndtAmdmntReq");
            XmlNode grpNode = doc.CreateElement("GrpHdr");
            //productsNode.AppendChild(productNode);

            string msgId = MandateCommonService.CreateMsgId();
            XmlNode msgNode = doc.CreateElement("MsgId");//
            msgNode.InnerText = msgId;
            grpNode.AppendChild(msgNode);

            XmlNode creNode = doc.CreateElement("CreDtTm");
            creNode.InnerText = DateTime.Now.ToString("s");
            grpNode.AppendChild(creNode);

            //if (!string.IsNullOrEmpty(mandate.UserNACHId))
            //{
            //    XmlNode initNode = doc.CreateElement("InitgPty");
            //    XmlNode initId = doc.CreateElement("Id");
            //    XmlNode initOrg = doc.CreateElement("OrgId");
            //    XmlNode initOthr = doc.CreateElement("Othr");
            //    XmlNode othrNId = doc.CreateElement("Id");
            //    othrNId.InnerText = mandate.UserNACHId;

            //    initOthr.AppendChild(othrNId);
            //    initOrg.AppendChild(initOthr);
            //    initId.AppendChild(initOrg);
            //    initNode.AppendChild(initId);
            //    grpNode.AppendChild(initNode);
            //}
            //Instructing Agent
            XmlNode instNode = doc.CreateElement("InstgAgt");
            XmlNode finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysNode = doc.CreateElement("ClrSysMmbId");
            XmlNode memIdNode = doc.CreateElement("MmbId");
            memIdNode.InnerText = mandate.SpnsrBnkCd;

            clrSysNode.AppendChild(memIdNode);
            XmlNode name = doc.CreateElement("Nm");
            name.InnerText = mandate.SpnsrBankName;

            finInstNode.AppendChild(clrSysNode);
            finInstNode.AppendChild(name);
            instNode.AppendChild(finInstNode);
            grpNode.AppendChild(instNode);

            //Instructed Agent
            XmlNode instdAgtNode = doc.CreateElement("InstdAgt");
            finInstNode = doc.CreateElement("FinInstnId");
            XmlNode clrSysMmbNode = doc.CreateElement("ClrSysMmbId");
            XmlNode mmbIdNode = doc.CreateElement("MmbId");
            mmbIdNode.InnerText = mandate.BnkCd;

            clrSysMmbNode.AppendChild(mmbIdNode);
            XmlNode spnrName = doc.CreateElement("Nm");
            spnrName.InnerText = mandate.BankName;

            finInstNode.AppendChild(clrSysMmbNode);
            finInstNode.AppendChild(spnrName);

            instdAgtNode.AppendChild(finInstNode);
            grpNode.AppendChild(instdAgtNode);
            mndtAmdReqNode.AppendChild(grpNode);
            //END GrpHdr tag

            //Underlying Amendment Details
            XmlNode underAmdmnt = doc.CreateElement("UndrlygAmdmntDtls");
            XmlNode amndRsn = doc.CreateElement("AmdmntRsn");
            XmlNode rsn = doc.CreateElement("Rsn");
            XmlNode prtyNd = doc.CreateElement("Prtry");
            prtyNd.InnerText = mandate.RtnRsn;//mandate.MndtReqId;  reason Code
            rsn.AppendChild(prtyNd);
            amndRsn.AppendChild(rsn);

            XmlNode mndtNd = doc.CreateElement("Mndt");
            XmlNode mndtId = doc.CreateElement("MndtId");
            mndtId.InnerText = mandate.UMRNo;
            mndtNd.AppendChild(mndtId);

            XmlNode mndtReqId = doc.CreateElement("MndtReqId");
            mndtReqId.InnerText = msgId;//mandate.MsgId; //Dont know...need to discuss
            mndtNd.AppendChild(mndtReqId);

            XmlNode tpNd = doc.CreateElement("Tp");
            XmlNode SvcLvl = doc.CreateElement("SvcLvl");
            XmlNode prtry = doc.CreateElement("Prtry");
            prtry.InnerText = mandate.Category;//need to chk
            SvcLvl.AppendChild(prtry);
            tpNd.AppendChild(SvcLvl);

            XmlNode LclInstrm = doc.CreateElement("LclInstrm");
            XmlNode lcPrtry = doc.CreateElement("Prtry");
            lcPrtry.InnerText = "Debit";
            LclInstrm.AppendChild(lcPrtry);
            tpNd.AppendChild(LclInstrm);

            XmlNode ocrns = doc.CreateElement("Ocrncs");
            XmlNode seqTp = doc.CreateElement("SeqTp");
            seqTp.InnerText = mandate.Seq;// Identifies the underlying transaction sequence as either recurring or one-off
            ocrns.AppendChild(seqTp);

            XmlNode frq = doc.CreateElement("Frqcy");
            frq.InnerText = mandate.Freq;
            ocrns.AppendChild(frq);

            XmlNode frstColnDt = doc.CreateElement("FrstColltnDt");
            frstColnDt.InnerText = Convert.ToDateTime(mandate.FrmPrd).ToString("yyyy-MM-dd");
            ocrns.AppendChild(frstColnDt);

            XmlNode fnlColnDt = doc.CreateElement("FnlColltnDt");
            fnlColnDt.InnerText = mandate.ToPrd == null ? "" : Convert.ToDateTime(mandate.ToPrd).ToString("yyyy-MM-dd");
            ocrns.AppendChild(fnlColnDt);
            mndtNd.AppendChild(ocrns);

            //Collection and max amt ///need to be discussed
            if (mandate.DebitTyp == "FixedAmt")
            {
                XmlNode colAmt = doc.CreateElement("ColltnAmt");
                XmlAttribute amtAttr = doc.CreateAttribute("Ccy");
                amtAttr.Value = "INR";
                colAmt.Attributes.Append(amtAttr);
                colAmt.InnerText = mandate.Amt.ToString();
                mndtNd.AppendChild(colAmt);
            }
            else
            {
                XmlNode maxAmt = doc.CreateElement("MaxAmt");
                XmlAttribute amtAttr = doc.CreateAttribute("Ccy");
                maxAmt.InnerText = mandate.Amt.ToString();
                amtAttr.Value = "INR";
                maxAmt.Attributes.Append(amtAttr);
                maxAmt.InnerText = mandate.Amt.ToString();
                mndtNd.AppendChild(maxAmt);
            }

            //max Amount

            XmlNode cdtr = doc.CreateElement("Cdtr");
            XmlNode nmNode = doc.CreateElement("Nm");
            nmNode.InnerText = mandate.UtilityName;
            cdtr.AppendChild(nmNode);
            mndtNd.AppendChild(cdtr);

            XmlNode cdtrAcct = doc.CreateElement("CdtrAcc");
            XmlNode cdtId = doc.CreateElement("Id");
            XmlNode othrNd = doc.CreateElement("Othr");
            XmlNode othId = doc.CreateElement("Id");
            othId.InnerText = mandate.UtilityCd;
            othrNd.AppendChild(othId);
            cdtId.AppendChild(othrNd);
            cdtrAcct.AppendChild(cdtId);

            //Creditor Agent
            XmlNode cdtrAgt = doc.CreateElement("CdtrAgt");
            XmlNode finInstId = doc.CreateElement("FinInstnId");
            XmlNode clrSysId = doc.CreateElement("ClrSysMmbId");
            XmlNode mmbId = doc.CreateElement("MmbId");
            mmbId.InnerText = mandate.SpnsrBnkCd;
            clrSysId.AppendChild(mmbId);
            finInstId.AppendChild(clrSysId);

            XmlNode mbrBnkName = doc.CreateElement("Nm");
            mbrBnkName.InnerText = mandate.CredtrAgentName;
            finInstId.AppendChild(mbrBnkName);
            cdtrAgt.AppendChild(finInstId);
            mndtNd.AppendChild(cdtrAgt);

            XmlNode dbtrNd = doc.CreateElement("Dbtr");
            XmlNode dbtNm = doc.CreateElement("Nm");
            dbtNm.InnerText = mandate.CrdtrName;
            dbtrNd.AppendChild(dbtNm);

            XmlNode dbtId = doc.CreateElement("Id");
            XmlNode prvtId = doc.CreateElement("PrvtId");
            XmlNode othr = doc.CreateElement("Othr");
            XmlNode othrId = doc.CreateElement("Id");
            othrId.InnerText = mandate.RefNo1;
            othr.AppendChild(othrId);

            XmlNode schmeNm = doc.CreateElement("SchmeNm");
            XmlNode schPrtry = doc.CreateElement("Prtry");
            schPrtry.InnerText = mandate.RefNo2;
            schmeNm.AppendChild(schPrtry);
            othr.AppendChild(schmeNm);
            prvtId.AppendChild(othr);
            dbtId.AppendChild(prvtId);
            dbtrNd.AppendChild(dbtId);

            XmlNode ctctDtls = doc.CreateElement("CtctDtls");
            XmlNode phoneNb = doc.CreateElement("PhneNb");
            phoneNb.InnerText = mandate.PhoneNo;
            ctctDtls.AppendChild(phoneNb);

            if (string.IsNullOrEmpty(mandate.Mobile))
            {
                XmlNode phoneNode = doc.CreateElement("PhneNb");
                phoneNode.InnerText = "+91-" + mandate.Mobile; //mandate.PhoneNo;
                ctctDtls.AppendChild(phoneNode);

                XmlNode mobNode = doc.CreateElement("MobNb");
                mobNode.InnerText = "+91-" + mandate.Mobile;
                ctctDtls.AppendChild(mobNode);
            }

            if (string.IsNullOrEmpty(mandate.EMail.Trim()))
            {
                XmlNode emailNode = doc.CreateElement("EmailAdr");
                emailNode.InnerText = mandate.EMail;
                ctctDtls.AppendChild(emailNode);
            }
            XmlNode othrDet = doc.CreateElement("Othr");
            othrDet.InnerText = mandate.OthrDetail;
            ctctDtls.AppendChild(othrDet);
            dbtrNd.AppendChild(ctctDtls);
            mndtNd.AppendChild(dbtrNd);

            //Debtor Account

            XmlNode dbtrAcct = doc.CreateElement("DbtrAcct");
            XmlNode dbtrId = doc.CreateElement("Id");
            XmlNode dbtrOthr = doc.CreateElement("Othr");
            XmlNode dbtrOthrId = doc.CreateElement("Id");
            dbtrOthrId.InnerText = mandate.AcNo;
            dbtrOthr.AppendChild(dbtrOthrId);

            dbtrId.AppendChild(dbtrOthr);
            dbtrAcct.AppendChild(dbtrId);

            XmlNode dbtrTp = doc.CreateElement("Tp");
            XmlNode dbtrTpPrtry = doc.CreateElement("Prtry");
            dbtrTpPrtry.InnerText = mandate.AcTyp;
            dbtrTp.AppendChild(dbtrTpPrtry);
            dbtrAcct.AppendChild(dbtrTp);
            mndtNd.AppendChild(dbtrAcct);

            //Debtor Agent
            XmlNode dbtrAgt = doc.CreateElement("DbtrAgt");
            XmlNode finInstnId = doc.CreateElement("FinInstnId");
            XmlNode clrSysMmbId = doc.CreateElement("ClrSysMmbId");
            XmlNode drMmbId = doc.CreateElement("MmbId");
            drMmbId.InnerText = mandate.BnkCd;
            clrSysMmbId.AppendChild(drMmbId);
            finInstnId.AppendChild(clrSysMmbId);

            //XmlNode drNm = doc.CreateElement("Nm");
            //drNm.InnerText = mandate.DebtrBnk;
            //finInstnId.AppendChild(drNm);
            //dbtrAgt.AppendChild(finInstnId);
            mndtNd.AppendChild(dbtrAgt);

            underAmdmnt.AppendChild(mndtNd);

            XmlNode orgnlMndt = doc.CreateElement("OrgnlMndt");
            XmlNode orgnlMndtId = doc.CreateElement("OrgnlMndtId");
            orgnlMndtId.InnerText = mandate.UMRNo;
            orgnlMndt.AppendChild(orgnlMndtId);
            underAmdmnt.AppendChild(orgnlMndt);

            mndtAmdReqNode.AppendChild(underAmdmnt);
            documentNode.AppendChild(mndtAmdReqNode);
            doc.AppendChild(documentNode);

            return doc;
        }

        public static Dictionary<string, string> GetAllAcTyp()
        {
            var typList = new Dictionary<string, string>();

            //typList.Add("SB", "Savings");
            //typList.Add("CA", "Current");
            //typList.Add("CC", "CC");
            //typList.Add("NRE", "SB-NRE");
            //typList.Add("NRO", "SB-NRO");
            //typList.Add("OT", "Other");

            typList.Add("Savings", "Savings");
            typList.Add("Current", "Current");
            typList.Add("CC", "CC");
            typList.Add("SB-NRE", "SB-NRE");
            typList.Add("SB-NRO", "SB-NRO");
            typList.Add("Other", "Other");

            return typList;
        }

        public static Dictionary<string, string> GetAllFreq()
        {
            var freqList = new Dictionary<string, string>();
            freqList.Add("MNTH", "Monthly");
            freqList.Add("ADHO", "Adhoc");
            freqList.Add("INDA", "Intra Day");
            freqList.Add("DAIL", "Daily");
            freqList.Add("WEEK", "Weekly");
            freqList.Add("QURT", "Querterly");
            freqList.Add("MIAN", "Semi Annually");
            freqList.Add("YEAR", "Yearly");
            freqList.Add("BIMN", "Bi-Monthly");

            return freqList;
        }

        public static Dictionary<string, string> GetAllDrTyp()
        {
            var drTypList = new Dictionary<string, string>();
            drTypList.Add("FixedAmt", "Fixed Amount");
            drTypList.Add("MaxAmt", "Maximum Amount");

            return drTypList;
        }
        public static Dictionary<string, string> GetMandateStatus()
        {
            var statList = new Dictionary<string, string>();
            statList.Add("All", "All");
            statList.Add("Uploaded", "Uploaded");
            statList.Add("Auth", "Authenticated");
            statList.Add("Return", "Return");
            statList.Add("Hold", "Hold");

            return statList;
        }

        public static Dictionary<string, string> GetAllSquence()
        {
            var occrList = new Dictionary<string, string>();

            occrList.Add("RCUR", "Recurring");
            occrList.Add("OOFF", "One-Off");

            return occrList;
        }


        public static string CreateXMLName(string mandateFile, string type, string reqTyp)
        {
            var configList = new MandateService().GetH2HConfig();
            string bankName = configList.Where(c => c.ConfigKey.ToUpper() == "BS_SHORTNAME").FirstOrDefault().ConfigValue;
            string userName = configList.Where(c => c.ConfigKey == "SS_USERNAME").FirstOrDefault().ConfigValue;

            string date = DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Year;

            if (string.IsNullOrEmpty(mandateFile))
            {
                var globalIdGenerator = new GlobalIdGeneratorService().GetMaxId("OWMandate");
                var serlNo = globalIdGenerator.LastNo.PadLeft(globalIdGenerator.Len, '0');

                //string date = DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Year;
                return "MMS-CREATE-" + bankName + "-" + userName + "-" + date + "-" + serlNo; //pending...serialNo
            }

            var manArry = mandateFile.Split('-');
            if (reqTyp == ECS.ACCEPT)
            {
                if (type == "XML")
                    return manArry[0] + "-" + "ACCEPT" + "-" + bankName + "-" + userName + "-" + date + "-" + manArry[5] + "-" + manArry[6];
                else
                    return manArry[0] + "-" + "CREATE" + "-" + bankName + "-" + userName + "-" + date + "-" + manArry[5].Replace("MWI", "") + "-" + "ACCEPT";
            }
            else if (reqTyp == ECS.CANCEL)
            {
                if (type == "XML")
                    return manArry[0] + "-" + "CANCEL" + "-" + bankName + "-" + userName + "-" + manArry[4] + "-" + manArry[5] + "-" + "INP";
                else
                    return manArry[0] + "-" + "CREATE" + "-" + bankName + "-" + userName + "-" + manArry[4] + "-" + manArry[5] + "-INP";
            }
            else if (reqTyp == ECS.CREATE)
            {
                if (type == "XML")
                    return manArry[0] + "-" + "CREATE" + "-" + bankName + "-" + userName + "-" + manArry[4] + "-" + manArry[5] + "-INP";
                else
                    return manArry[0] + "-" + "CREATE" + "-" + bankName + "-" + userName + "-" + manArry[4] + "-" + manArry[5] + "-INP";
            }
            else
            {
                if (type == "XML")
                    return manArry[0] + "-" + "AMEND" + "-" + bankName + "-" + userName + "-" + manArry[4] + "-" + manArry[5] + "-INP";
                else
                    return manArry[0] + "-" + "AMEND" + "-" + bankName + "-" + userName + "-" + manArry[4] + "-" + manArry[5] + "-INP";
            }
        }

        public static string CreateMsgId()
        {
            IGlobalIdGeneratorService idGeneratorService = new GlobalIdGeneratorService();

            //string bankCd= ConfigurationManager.AppSettings["BANK_CODE"];

            string bankCd = new MandateService().GetEhConfigByField("BS_SHORTCODE");
            string dtStr = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year;
            dtStr = dtStr.PadLeft(14, '0');

            var globalIdGenerator = new GlobalIdGeneratorService().GetMaxId("MandateId");

            if (Convert.ToInt16(globalIdGenerator.LastNo) == 9999)
            {
                globalIdGenerator.LastNo = "1";
                idGeneratorService.Update(globalIdGenerator);
            }
            var serlNo = globalIdGenerator.LastNo.PadLeft(globalIdGenerator.Len, '0');

            string msgId = "MMS" + bankCd + dtStr + serlNo;
            return msgId;
        }

        public void CreateOutputZip(ECS mandate, XmlDocument doc, string mndtTyp)
        {
            MandateService mandateService = new MandateService();
            string xmlFileName = MandateCommonService.CreateXMLName(mandate.MandateFile, "XML", mndtTyp);
            string zipFileName = MandateCommonService.CreateXMLName(mandate.MandateFile, "ZIP", mndtTyp);

            string tempFilePath = mandateService.GetEhConfigByField("Inward_Output_Path");
           

            UtilityExtensions.IPersonateuser();

            if (!Directory.Exists(tempFilePath))
            {
                Directory.CreateDirectory(tempFilePath);
            }
            // doc.Save(tempFilePath + @"\" + xmlFileName + ".xml");

            using (TextWriter sw = new StreamWriter(tempFilePath + @"\" + xmlFileName + ".xml"))//, false, Encoding.UTF8)) //Set encoding
            {
                doc.Save(sw);
            }

            using (ZipArchive zipArchive = ZipFile.Open(tempFilePath + @"\" + zipFileName + ".zip", ZipArchiveMode.Create))
            {
                FileInfo fileToArchive = new FileInfo(tempFilePath + @"\" + xmlFileName + ".xml");
                zipArchive.CreateEntryFromFile(fileToArchive.FullName, fileToArchive.Name, CompressionLevel.Optimal);
                System.IO.File.Delete(tempFilePath + @"\" + xmlFileName + ".xml");
            }
            //END

            //Update entry in History table

            //MandateFileHistory manFileHis = new MandateFileHistory();
            //manFileHis.MndtId = mandate.Id;
            //manFileHis.MndtReqTyp = mndtTyp;
            //manFileHis.AuthUser = StartUp.LOGIN_ID;
            //manFileHis.DtStamp = DateTime.Now;
            //manFileHis.FilePath = tempFilePath;
            //manFileHis.XMLFile = xmlFileName;
            //if (mndtTyp == ECS.AMEND || mndtTyp == ECS.CANCEL)
            //    manFileHis.Reason = mandate.RtnRsn;

            //new MandateService().UpdateHistory(manFileHis);
        }

        public void GenerateMandateXML(ECS mandate, bool isAccp)
        {
            MandateService mandateService = new MandateService();
            MandateCommonService mandateCommonService = new MandateCommonService();
            //File Creation 
            string xmlFileName, reqTyp;
            xmlFileName = CreateXMLName(mandate.MandateFile, "XML", ECS.ACCEPT);

            //if (!isAccp)
            // xmlFileName= xmlFileName.Replace("-INP", "-RTN");
            //XML file
            if (xmlFileName.Contains("-MWI"))
                xmlFileName = xmlFileName.Replace("-MWI", "-");

            if (xmlFileName.Contains("-ESIGN"))
                xmlFileName = xmlFileName.Replace("-ESIGN", "-");

            var doc = CreateAccept(mandate, isAccp);

            string tempFilePath = mandateService.GetEhConfigByField("Inward_Output_Path");

            UtilityExtensions.IPersonateuser();

            if (!Directory.Exists(tempFilePath))
            {
                Directory.CreateDirectory(tempFilePath);
            }
            // doc.Save(tempFilePath + @"\" + xmlFileName + ".xml");

            using (TextWriter sw = new StreamWriter(tempFilePath + @"\" + xmlFileName + ".xml"))//, false, Encoding.UTF8)) //Set encoding
            {
                doc.Save(sw);
            }

            //if(ECS.ACCEPT == "Accept")
            //{
            //    //var logn = StartUp.LOGIN_ID;
            //    if (xmlFileName.Contains("993"))
            //        xmlFileName = xmlFileName.Replace(StartUp.LOGIN_ID);
            //}

            string zipFileName = MandateCommonService.CreateXMLName(mandate.MandateFile, "ZIP", ECS.ACCEPT);

            using (ZipArchive zipArchive = ZipFile.Open(tempFilePath + @"\" + zipFileName + ".zip", ZipArchiveMode.Create))
            {
                FileInfo fileToArchive = new FileInfo(tempFilePath + @"\" + xmlFileName + ".xml");
                zipArchive.CreateEntryFromFile(fileToArchive.FullName, fileToArchive.Name, CompressionLevel.Optimal);
                System.IO.File.Delete(tempFilePath + @"\" + xmlFileName + ".xml");
            }
            //END
            mandate.ResFile = xmlFileName;
            mandateService.Update(mandate);

            //Update entry in History table

            //MandateFileHistory manFileHis = new MandateFileHistory();
            //manFileHis.MndtId = mandate.Id;
            //manFileHis.ReqTyp = ECS.ACCEPT;
            //manFileHis.AuthUser = StartUp.LOGIN_ID;
            //manFileHis.DtStamp = DateTime.Now;
            //manFileHis.FilePath = tempFilePath;
            //manFileHis.FileName = xmlFileName;
            //manFileHis.FileExtn = ".xml";
            //new MandateService().UpdateHistory(manFileHis);

        }

        public string GenerateACHFile(List<MandateInstruction> MandateinstructionList, DateTime SetDate)
        {
            var outputPath = mandateService.GetEhConfigByField("Inward_Output_Path");
           // var outputPath = "D:\\All_DCB_ECS_Data\\test";
            try
            {
                //System.IO.File.Create(form24QPath).Dispose();
                UtilityExtensions.IPersonateuser();

                var configList = new MandateService().GetH2HConfig();
                string bankName = configList.Where(c => c.ConfigKey == "BS_SHORTNAME").FirstOrDefault().ConfigValue;
                string userNumb = configList.Where(c => c.ConfigKey == "SS_USERNUMBER").FirstOrDefault().ConfigValue;
                string bankAccNo = configList.Where(c => c.ConfigKey == "MNDT_BNK_ACNO").FirstOrDefault().ConfigValue;
                string userName = new MandateService().GetEhConfigByField("SS_USERNAME");
                var utilityCd = new MandateService().GetEhConfigByField("Utility_Code");
                var ifscCd = new MandateService().GetEhConfigByField("IFSC_CD");

                string date = DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Year;

                var globalIdGenerator = new GlobalIdGeneratorService().GetMaxId("EBTInputFileNumber");
                var achFileNo = globalIdGenerator.LastNo.PadLeft(globalIdGenerator.Len, '0');

                var globalUserRef = new GlobalIdGeneratorService().GetMaxId("ACHUserRef");
                var userRef = globalUserRef.LastNo.PadLeft(globalUserRef.Len, '0');
                string fileName = "ACH-DR-" + bankName + "-" + userName + "-" + date + "-" + achFileNo + "-INP.txt";
                FileStream fs1 = new FileStream(outputPath + @"\ACH-DR-" + bankName + "-" + userName + "-" + date + "-" + achFileNo + "-INP.txt", FileMode.OpenOrCreate, FileAccess.Write);
                var FileName = "ACH-DR-" + bankName + "-" + userName + "-" + date + "-" + achFileNo + "-INP.txt";

                if (userName.Length > 40)
                    userName = userName.Remove(40);

                decimal totAmt = Convert.ToInt32(MandateinstructionList.Sum(s => s.AMT) * 100);

                StreamWriter writer = new StreamWriter(fs1);
                writer.Write(ACH.ACH_DEBIT);//ACH trnsaction Code
                writer.Write(SetBlank(7));// Control Spaces
                writer.Write(userName.PadRight(40, ' '));//user NAme
                writer.Write(SetBlank(14));// Control Spaces
                writer.Write(achFileNo.ToString().PadLeft(9, '0'));//ACh file No
                writer.Write(SetBlank(9));//Control Spaces
                writer.Write(SetBlank(15));// Control Spaces
                writer.Write(SetBlank(3));//Ledger Foilio No
                writer.Write(SetBlank(13));// user defined limit for indi item
                writer.Write((totAmt).ToString().PadLeft(13, '0'));//tot amount in paise
                writer.Write(SetDate.Day.ToString().PadLeft(2, '0') + SetDate.Month.ToString().PadLeft(2, '0') + SetDate.Year.ToString());//settlement date
                writer.Write(SetBlank(10));//Reserved
                writer.Write(SetBlank(10));//Reserved
                writer.Write(SetBlank(3));//Filler
                writer.Write(utilityCd.PadRight(18, ' '));//Utility Code--  user No to correct
                writer.Write((bankName + userRef).PadRight(18, ' '));//user referance
                writer.Write(ifscCd.PadRight(11, ' '));//Sponser Bank IFSC --to correct Utility Code
                writer.Write(bankAccNo.PadRight(35, ' '));//User's Bank Account
                writer.Write(MandateinstructionList.Count.ToString().PadLeft(9, '0'));//Total Items
                writer.Write(SetBlank(6));//Settlement cycle
                writer.Write(SetBlank(53));

                writer.WriteLine("");

               // MandateFileHistory mandateFileHistory = new MandateFileHistory();
                foreach (MandateInstruction mandateinst in MandateinstructionList)
                {
                    var ecs = new MandateService().GetById(mandateinst.MNDTID);
                    writer.Write("67");//ACH transaction code
                    writer.Write(SetBlank(9));//Control

                    string acTyp = "  ";
                    switch (ecs.AcTyp)
                    {
                        case "Savings":
                            acTyp = "10";
                            break;
                        case "Current":
                            acTyp = "29";
                            break;
                        case "CC":
                            acTyp = "11";
                            break;
                        case "SB - NRE":
                            acTyp = "  ";
                            break;
                            case "SB-NRO":
                            acTyp = "  ";
                            break;
                        case "Other":
                            acTyp = "  ";
                            break;
                    }

                    var ecsAmt = Convert.ToInt32(ecs.Amt * 100);
                    writer.Write(acTyp);//Destination acctyp
                    writer.Write(SetBlank(3));//ledger folio no
                    writer.Write(SetBlank(15));//control

                    if (ecs.CrdtrName.Length > 35)
                    {
                        var crdtrname = ecs.CrdtrName.Substring(0, 35);
                        writer.Write(crdtrname.PadRight(40, ' '));//beneficiary acc holder name...... need to ask
                    }
                    else
                        writer.Write(ecs.CrdtrName.PadRight(40, ' '));//beneficiary acc holder name...... need to ask
                    writer.Write(SetBlank(9));//control
                    writer.Write(SetBlank(7));//control
                    if (ecs.UtilityName.Length < 20)// writer.Write(SetBlank(20));//User name //need to ask
                        writer.Write(ecs.UtilityName.PadRight(20, ' '));
                    else
                        writer.Write(ecs.UtilityName.Substring(0, 19).PadRight(20, ' '));
                    writer.Write(SetBlank(13));//control
                    writer.Write(ecsAmt.ToString().PadLeft(13, '0'));
                    writer.Write(SetBlank(10));//Reserved(ACh item seq no)
                    writer.Write(SetBlank(10));//Reserved (checksum)
                    writer.Write(SetBlank(1));//Reserved(Flag for success)
                    writer.Write(SetBlank(2));//Reserved(Reason code)
                    writer.Write(ecs.BnkCd.PadRight(11, ' '));//Destination bank IFSC 
                    writer.Write(ecs.AcNo.PadRight(35, ' '));//Benificiery's bank account no
                                                             //writer.Write(ecs.SpnsrBnkCd.PadRight(11, ' '));//Sponser's bank IFSC
                    writer.Write(ifscCd.PadRight(11, ' '));//Sponser's bank IFSC
                    writer.Write(userNumb.PadRight(18, ' '));//user number ///need to ask Manish
                    var todayDay = DateTime.Now;
                    var tranRefNo = "REF" + todayDay.Day.ToString().PadLeft(2, '0') + todayDay.Month.ToString().PadLeft(2, '0') + todayDay.Year.ToString().PadLeft(2, '0') + todayDay.Hour.ToString() + todayDay.Minute.ToString() + todayDay.Second.ToString() + ecs.UMRNo.Substring(16, 4);
                    writer.Write(tranRefNo.PadRight(30, ' '));//Transaction ref No

                    writer.Write(acTyp);//writer.Write("ACH");//Product Typ
                    writer.Write(SetBlank(16));//Bef Aadhar No(compl for APBS)
                    writer.Write(ecs.UMRNo.PadRight(20, ' '));//UMRN no
                    writer.WriteLine(SetBlank(7));//Filler

                    mandateinst.STAT = "G";
                    mandateinst.REFNO = tranRefNo.Trim();
                    mandateinst.INPFILENAME = FileName;
                    mandateService.Update(mandateinst);

                    DateTime NextDt;
                    if (ecs.Freq == "MNTH")
                        NextDt = mandateinst.DMDDT.AddMonths(1);
                    else if (ecs.Freq == "DAIL")
                        NextDt = mandateinst.DMDDT.AddDays(1);
                    else if (ecs.Freq == "QURT")
                        NextDt = mandateinst.DMDDT.AddMonths(3);
                    else if (ecs.Freq == "YEAR")
                        NextDt = mandateinst.DMDDT.AddYears(1);
                    else
                        NextDt = mandateinst.DMDDT;

                    if(NextDt<= ecs.ToPrd && NextDt!= mandateinst.DMDDT)
                    {
                        MandateInstruction inp = new MandateInstruction();
                        inp.IsNew = true;
                        inp.MNDTID = mandateinst.MNDTID;
                        inp.DMDDT = NextDt;
                        inp.AMT = mandateinst.AMT;
                        inp.REFNO = "";
                        inp.STAT = "S";
                        inp.DTSTAMP = DateTime.Now;
						try
						{
                            if (StartupManager.StartUp.LOGIN_ID == "")
                            {
                                inp.AUTHUSER = (StartupManager.StartUp.LOGIN_ID = "Admin");
                            }

                            else
							{
                                inp.AUTHUSER = StartupManager.StartUp.LOGIN_ID;
                            }

                        }
                        catch
						{
                            inp.AUTHUSER = (StartupManager.StartUp.LOGIN_ID = "Admin");
                        }

						
                        mandateService.Update(inp);

                    }

                    //ACHInputFile inp = new ACHInputFile();
                    //inp = mandateService.GetByMndtId(ecs.Id);
                    //if (inp != null)
                    //{
                    //    if (ecs.Freq == "MNTH")
                    //    {
                    //        if (inp.DueDt == null)
                    //            inp.DueDt = Convert.ToDateTime(inp.StartDt).AddMonths(1);
                    //        else
                    //            inp.DueDt = Convert.ToDateTime(inp.DueDt).AddMonths(1);
                    //    }

                    //    else if (ecs.Freq == "DAIL")
                    //    {
                    //        if (inp.DueDt == null)
                    //            inp.DueDt = Convert.ToDateTime(inp.StartDt).AddDays(1);
                    //        else
                    //            inp.DueDt = Convert.ToDateTime(inp.DueDt).AddDays(1);
                    //    }

                    //    else if (ecs.Freq == "QURT")
                    //    {

                    //        if (inp.DueDt == null)
                    //            inp.DueDt = Convert.ToDateTime(inp.StartDt).AddMonths(3);
                    //        else
                    //            inp.DueDt = Convert.ToDateTime(inp.DueDt).AddMonths(3);

                    //    }
                    //    else if (ecs.Freq == "YEAR")
                    //    {
                    //        if (inp.DueDt == null)
                    //            inp.DueDt = Convert.ToDateTime(inp.StartDt).AddYears(1);
                    //        else
                    //            inp.DueDt = Convert.ToDateTime(inp.DueDt).AddYears(1);
                    //    }
                    //    new MandateService().Update(inp);


                    //    mandateFileHistory = new MandateFileHistory();
                    //    mandateFileHistory.MndtId = inp.MndtId;
                    //    mandateFileHistory.ReqTyp = ECS.CREATE;
                    //    mandateFileHistory.FileName = fileName;
                    //    mandateFileHistory.FileExtn = "txt";
                    //    mandateFileHistory.FilePath = outputPath + @"\" + fileName + ".txt";
                    //    mandateFileHistory.TranRefNo = tranRefNo;

                    // mandateService.Update(null, mandateFileHistory);
                }
                    
                

                writer.Close();
            }
            catch (Exception ex)
            {
                return ex.Message + ex.StackTrace;
            }
            return "";
        }

        private string SetBlank(int length)
        {
            return "".PadRight(length, ' ');
        }
        private DateTime GetOpeningDate()
        {
            DateTime dayOpDt = DateTime.MinValue;
            SysControl sysControl = new SysControlService().GetMax();
            if (sysControl != null)
                for (int i = 2; ; i++)
                {
                    dayOpDt = sysControl.DayOpDt.AddDays(i);
                    Holiday holiday = new HolidayService().GetByDate(dayOpDt);
                    if (holiday == null && dayOpDt.DayOfWeek.ToString() != StartUp.WK_OFF)
                        break;
                }

            return dayOpDt;
        }

        public static void CreateLog(string message)
        {
            string logText;

            UtilityExtensions.IPersonateuser();

            logText = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            logText += Environment.NewLine;
            logText += message + Environment.NewLine;
            logText += "-----------------------------------------------------------";

            string path = ConfigurationManager.AppSettings["LOG_FILE_PATH"];
            // path += @"\SMSLog.txt";
            string fileExten = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
            path += @"\EcsScheduleLog" + fileExten + ".txt";
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            using (StreamWriter writer = File.AppendText(path))
            {
                writer.WriteLine(logText);
                writer.Flush();
                writer.Close();
            }
        }


        public static Bitmap CaptchaGeneration(string captchatxt)
        {
            Bitmap bmp = new Bitmap(133, 30);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                Font font = new Font("Tahoma", 14);
                graphics.FillRectangle(new SolidBrush(Color.LightGray), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(captchatxt, font, new SolidBrush(Color.Black), 25, 4);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();
            }
            return bmp;
        }

        public static string GetRandomText()//string prefix, bool noisy = true)
        {
            StringBuilder randomText = new StringBuilder();
            string alphabets = "012345679ACEFGHKLMNPRSWXZabcdefghijkhlmnopqrstuvwxyz";
            Random r = new Random();
            for (int j = 0; j <= 5; j++)
            {
                randomText.Append(alphabets[r.Next(alphabets.Length)]);
            }
            return randomText.ToString();
        }
    }
}