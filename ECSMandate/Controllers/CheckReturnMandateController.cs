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
    public class CheckReturnMandateController : AppController
    {
        MandateService mandateService = new MandateService();

        // GET: CheckReturnMandate
        public ActionResult Index()
        {
            SetViewBag();
            return View();
        }

        public ActionResult CheckMandate()
        {
            SetViewBag();
            var ecsReturnList = new MandateService().GetAllReturnMandate();
            List<ECS> ecsList = new List<ECS>();
            foreach (ECS ecs in ecsReturnList)
            {
                ecs.Status = ecs.IsAccept == true ? "Accepted" : "Decline";
                ecs.Reason = ecs.Reason;

                ecsList.Add(ecs);
            }
            return Json(new { partialView = MvcHelpers.RenderPartialView(this, "_returnList", ecsList, null) }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetMandateImages(Int32 Id)
        {
            Response response = new Response();
            List<ImageModel> imgModelList = new List<ImageModel>();
            ImageModel imgModel;

            List<string> imageList = new List<string>();
            string frntImgSrc, detailFrntImgSrc;
            SetViewBag();


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
                //bitMapImg = UtilityExtensions.ConvertTiffToJPG(frntImgSrc, ref name);
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

            if (ecsMandate.FrntImg != string.Empty)
            {
                if (bitMapImg != null)
                    bitMapImg.Dispose();
                System.IO.File.Delete(name);
            }
            response.Status = ServiceStatus.Success;
            var jsonResonse = Json(response, JsonRequestBehavior.AllowGet);
            jsonResonse.MaxJsonLength = Int32.MaxValue;
            return jsonResonse;
        }

        private void SetViewBag()
        {
            var rsnList = mandateService.GetAllReason("Accept", "All");
            ViewBag.ReasonList = rsnList.Select(s => new { Name = s.Reason, RsnCd = s.RsnCd });
        }
        public ActionResult VerifyMandate()
        {

            return View();
        }
    }
}