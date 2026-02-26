using EASYECS.BussinessLayer.ECSService.Extension;
using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ECSMandate.Models;
using ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECSMandate.Controllers
{
    public class MandateCheckDataController : AppController
    {
        MandateService mandateService = new MandateService();
        // GET: MandateCheckData
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReserveECS(string BatchSize)
        {
            Response response = new Response();
            SetViewBag();
            var reserveMandateList = mandateService.Reserve(BatchSize, StartUp.LOGIN_ID);
            var modelList = ConvertMandateTOModel(reserveMandateList);

            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_ReserveList", modelList, null) }, JsonRequestBehavior.AllowGet);
        }

        public List<MandateModel> ConvertMandateTOModel(List<ECS> EcsMandateList)
        {
            MandateModel model;
            List<MandateModel> mandateMdlList = new List<MandateModel>();
            foreach (ECS mandate in EcsMandateList)
            {
                model = ConvertECSModel(mandate);
                mandateMdlList.Add(model);
            }
            return mandateMdlList;
        }
        public ActionResult ShowReserveECS()
        {
            Response response = new Response();
            List<MandateModel> mandateMdlList = new List<MandateModel>();
            MandateModel model;
            var reservedMandateList = mandateService.GetReservedMandate(StartUp.LOGIN_ID);
            SetViewBag();
            mandateMdlList = ConvertMandateTOModel(reservedMandateList);

            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_ReserveList", mandateMdlList, null) }, JsonRequestBehavior.AllowGet);
        }

        private MandateModel ConvertECSModel(ECS ECSMandate)
        {
            MandateModel manModel = new MandateModel();
            string catName = "";
            var category = mandateService.GetCategoryByCd(ECSMandate.Category);
            if (category != null)
            {
                catName = category.Descrp;
            }
            manModel.Id = ECSMandate.Id;
            manModel.TrnInd = ECSMandate.TrnInd;
            manModel.UMRDt = ECSMandate.UMRDt;
            manModel.UMRNo = ECSMandate.UMRNo;
            manModel.SpnsrBnkCd = ECSMandate.SpnsrBnkCd;
            manModel.UtilityName = ECSMandate.UtilityName;
            manModel.AcNo = ECSMandate.AcNo;
            manModel.Amt = ECSMandate.Amt;
            manModel.CatName = catName;
            manModel.MandateStatus = ECSMandate.MandateStatus;

            return manModel;
        }

        private void SetViewBag()
        {
            var rsnList = mandateService.GetAllReason("Accept", "All");
            ViewBag.ReasonList = rsnList.Select(s => new { Name = s.Reason, RsnCd = s.RsnCd });
        }

        [HttpPost]
        public ActionResult GetMandateImages(Int32 Id)
        {
            Response response = new Response();
            List<ImageModel> imgModelList = new List<ImageModel>();
            ImageModel imgModel;

            List<string> imageList = new List<string>();
            string frntImgSrc, detailFrntImgSrc;

            var ecsMandate = mandateService.GetById(Id);

            var imagePath = ConfigStartUp.INWARD_IMAGE_PATH;

            if (ecsMandate.FrntImg == null && ecsMandate.DetailFrntImg == null)
            {
                response.Message = "Image Not Found";
                response.Status = ServiceStatus.ValidationError;
                return Json(response);
            }
            frntImgSrc = imagePath + @"\" + ecsMandate.FrntImg;
            detailFrntImgSrc = imagePath + @"\" + ecsMandate.DetailFrntImg;
            string name = "";
            string base64frntImgSrc = string.Empty;

            UtilityExtensions.IPersonateuser();

            if (ecsMandate.DetailFrntImg != string.Empty)
            {
                imgModel = new ImageModel();
                imgModel.ImgName = ecsMandate.DetailFrntImg;
                imgModel.ImgSrc = imagePath;
                imgModel.ImageTyp = "DetailFrnt";
                imgModelList.Add(imgModel);
            }

            Image bitMapImg = null;
            if (ecsMandate.FrntImg != string.Empty)
            {
                bitMapImg = UtilityExtensions.ConvertTiffToJPG(frntImgSrc, ref name);
                ImageConverter converter = new ImageConverter();
                base64frntImgSrc = "data:image/jpg;base64," + Convert.ToBase64String((byte[])converter.ConvertTo(bitMapImg, typeof(byte[])));//ecsMandate.FrntImg,

                imgModel = new ImageModel();
                imgModel.ImgName = ecsMandate.FrntImg;
                imgModel.ImgSrc = base64frntImgSrc;
                imgModel.ImageTyp = "Front";
                imgModelList.Add(imgModel);
            }
            var rsnList = mandateService.GetAllReason("Accept", ecsMandate.Variant);
            var rsnListNew = rsnList.Select(s => new { Name = s.RsnCd + "-" + s.Reason, RsnCd = s.RsnCd });

            response.Item = new
            {
                ImgList = imgModelList,
                FrntImgSrc = base64frntImgSrc,
                DetailFrntImgSrc = ecsMandate.DetailFrntImg,
                ImgPath = imagePath,
                Mandate = ecsMandate,
                RsnList = rsnListNew
            };

            //if (ecsMandate.FrntImg != string.Empty)
            //{
            //    if (bitMapImg != null)
            //        bitMapImg.Dispose();
            //    System.IO.File.Delete(name);
            //}
            response.Status = ServiceStatus.Success;
            var jsonResonse = Json(response, JsonRequestBehavior.AllowGet);
            jsonResonse.MaxJsonLength = Int32.MaxValue;
            return jsonResonse;
        }

        public ActionResult UpdateCheckMandate(string[] MandateList)
        {
            Response response = new Response();

            List<string> updatedList = new List<string>();
            List<ECS> ecsMandateList = new List<ECS>();

            string mList = MandateList[0].Remove(0, 2);

            //var newArray = mList.Split(',');
            var newArray = mList.Split(new string[] { "],[" }, StringSplitOptions.None);
            ECS ecs;
            bool isAccept = true;

            foreach (string arr in newArray)
            {
                isAccept = true;
                var rowDetail = arr.Replace("\"", "").Replace("]]", "");

                //id = id.Replace("]", "");
                var rowArry = rowDetail.Split(',');

                if (rowArry[1] == "") continue;
                var id = rowArry[0];

                ecs = mandateService.GetById(Convert.ToInt32(id));
                if (rowArry[1] == "Checked")
                    ecs.TrnInd = "Q";
                else
                {
                    ecs.RtnRsn = rowArry[1];
                    if (!ecs.RtnRsn.StartsWith("M0"))
                    {
                        response.Status = ServiceStatus.Failure;
                        response.Message = "Please select return reason!.";
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    ecs.TrnInd = "R";
                    ecs.MandateStatus = "Return";

                    isAccept = false;
                }

                ecs.AuthDt = DateTime.Now;
                ecs.Operation = "Update";
                //ecs.AuthUser = StartUp.LOGIN_ID;
                ecs.ReserveBy = StartUp.LOGIN_ID;
                ecs.ReserveDt = DateTime.Now;
                // DateTime.Now;
                ecsMandateList.Add(ecs);

                updatedList.Add(id);

                //Create XMl response

                if (!isAccept)
                {
                    try
                    {
                        new MandateCommonService().GenerateMandateXML(ecs, isAccept);
                        response.Status = ServiceStatus.Success;
                        response.Message = "Record updated successfully!.";
                    }
                    catch (Exception ex)
                    {
                        response.Status = ServiceStatus.ValidationError;
                        response.Message = ex.Message;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    //string xmlFileName;
                    //xmlFileName = ecs.MandateFile.Replace("-INP", "-RTN"); //need to discuss response file name format

                    //var doc = MandateCommonService.CreateAccept(ecs, false);
                    //UtilityExtensions.IPersonateuser();
                    //SaveXMLToDir(doc, xmlFileName);
                }
            }

            //mandateService.BulkUpdate(ecsMandateList);
            mandateService.Update(ecsMandateList);

            var reservedMandateList = mandateService.GetReservedMandate(StartUp.LOGIN_ID);
            SetViewBag();
            var mandateMdlList = ConvertMandateTOModel(reservedMandateList);
            response.Status = ServiceStatus.Success;
            response.Message = "Updated successfully!.";
            response.Item = new { partialView = MvcHelpers.RenderPartialView(this, "_ReserveList", mandateMdlList, null) };

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}