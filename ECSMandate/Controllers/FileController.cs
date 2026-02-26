using EasyBank.Framework.EasySecurity;
using EASYECS.BussinessLayer.ECSService.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EASYECS.BussinessLayer.ECSService.Extension;

namespace ECSMandate.Controllers
{
    public class FileController : AppController
    {
        // GET: File

        public static void IPersonateuser()
        {
            ImpersonateUser iU = new ImpersonateUser();
            var domain = ConfigurationManager.AppSettings["Domain"];
            var userName = ConfigurationManager.AppSettings["UserName"];
            var pwd = ConfigurationManager.AppSettings["Password"];

            iU.Impersonate(domain, userName, pwd);
        }
        public static void IPersonateSignUser()
        {
            ImpersonateUser iU = new ImpersonateUser();
            var domain = ConfigurationManager.AppSettings["SignDomain"];
            var userName = ConfigurationManager.AppSettings["SignUserName"];
            var pwd = ConfigurationManager.AppSettings["SignPwd"];

            iU.Impersonate(domain, userName, pwd);
        }

        public ActionResult Index()
        {
            return View();
        }
        public FileResult GetFile(string file, string path)
        {
            if (file == "")
                return null;


            IPersonateuser();
            IPersonateSignUser();

            var fileData = file.GetFileData(path);
            if (file.Contains(".tiff") || file.Contains(".tif"))
            {
                return new FileContentResult(fileData, "image/tif");
            }
            else
                return new FileContentResult(fileData, "image/jpg");
            
        }
        public FileResult GetFileByName(string file)
        {
            if (file == "")
                return null;

            try
            {
                string signatureFilePath = ConfigurationManager.AppSettings.Get("SIGNATURE_FILE_PATH");

                var brnCd = file.Substring(0, 4);
                string path = signatureFilePath +  @"\" + brnCd;

                IPersonateuser();
                IPersonateSignUser();
                var fileData = file.GetFileData(path);
                if (file.Contains(".tiff") || file.Contains(".tif"))
                {                   
                    return new FileContentResult(fileData, "image/tif");
                }
                else
                    return new FileContentResult(fileData, "image/jpg");
            }
            catch (FileNotFoundException)
            {
                throw new HttpException(404, "File not found.");

            }
        }
        public FileResult GetFile1(int Id)
        {
            var ecsMandate = new MandateService().GetById(Id);
            //var imagePath = ConfigurationManager.AppSettings.Get("MandateImageFilePath"); 
            var imagePath = ConfigurationManager.AppSettings.Get("MandateFilePath");
            return GetFile(ecsMandate.FrntImg, imagePath);
        }

        public string[] ConvertTiffToJPG(string fileName)
        {
            UtilityExtensions.IPersonateuser();
            using (Image imageFile = Image.FromFile(fileName))
            {
                FrameDimension frameDimensions = new FrameDimension(
                    imageFile.FrameDimensionsList[0]);

                // Gets the number of pages from the tiff image (if multipage) 
                int frameNum = imageFile.GetFrameCount(frameDimensions);
                string[] jpegPaths = new string[frameNum];

                for (int frame = 0; frame < frameNum; frame++)
                {
                    // Selects one frame at a time and save as jpeg. 
                    imageFile.SelectActiveFrame(frameDimensions, frame);
                    using (Bitmap bmp = new Bitmap(imageFile))
                    {
                        jpegPaths[frame] = String.Format("{0}\\{1}{2}.jpg",
                            Path.GetDirectoryName(fileName),
                            Path.GetFileNameWithoutExtension(fileName),
                            frame);
                        bmp.Save(jpegPaths[frame], ImageFormat.Jpeg);
                        var bitmapImg = Image.FromFile(jpegPaths[frame]);
                    }
                }
                return jpegPaths;
            }
        }
    }
}