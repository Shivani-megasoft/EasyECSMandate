using EASYECS.BussinessLayer.ECSService.Object;
using EASYECS.BussinessLayer.ECSService.Service;
using ServiceObjects.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace ECSMandate.Controllers
{
    public class ScanMandateController : AppController
    {
        int cnt = 0;
        string Message;
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UploadScanMandate()
        {
            if (Session["Message"] != null)
            {
                ViewBag.Message = Session["Message"].ToString();
                Session["Message"] = null;
            }
            else
            {
                ViewBag.Message = "";
            }
            return View("UploadScanMandate");
        }

        public ActionResult SaveImage(string GreyImg, string BitonalImg)
        {
            Response response = new Response();

            try
            {
                cnt++;
                var frntGryImg = Guid.NewGuid();
                var frntBiotImg = frntGryImg + "_Bitonal";
                var scanPath = ConfigStartUp.MANDATE_SCAN_PATH + "\\" + StartUp.BRN_CD;
                string jpgFilePath = scanPath + "\\" + frntGryImg + ".jpg";
                string tiffFilePath1 = scanPath + "\\" + frntBiotImg + ".tiff";
                byte[] imageBytesJpg = Convert.FromBase64String(GreyImg);
                MemoryStream ms = new MemoryStream(imageBytesJpg, 0, imageBytesJpg.Length);
                ms.Write(imageBytesJpg, 0, imageBytesJpg.Length);
                System.Drawing.Image image0 = System.Drawing.Image.FromStream(ms, true);
                image0.Save(jpgFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytesBit = Convert.FromBase64String(BitonalImg);
                MemoryStream ms1 = new MemoryStream(imageBytesBit, 0, imageBytesBit.Length);
                ms1.Write(imageBytesBit, 0, imageBytesBit.Length);
                //System.Drawing.Image image1 = System.Drawing.Image.FromStream(ms, true);
                using (var oImage = Image.FromStream(ms))
                {
                    var originalBMP = new Bitmap(oImage);
                    int origWidth = originalBMP.Width;
                    int origHeight = originalBMP.Height;
                    int sngRatio = origWidth / origHeight;
                    int newWidth = origWidth * 40 / 100;
                    int newHeight = origHeight * 40 / 100;

                    // Create a new bitmap which will hold the previous resized bitmap
                    Bitmap newBMP = new Bitmap(originalBMP, newWidth, newHeight);
                    // Create a graphic based on the new bitmap
                    Graphics oGraphics = Graphics.FromImage(newBMP);

                    // Set the properties for the new graphic file
                    oGraphics.SmoothingMode = SmoothingMode.AntiAlias; oGraphics.InterpolationMode = InterpolationMode.Low;
                    // Draw the new graphic based on the resized bitmap
                    oGraphics.DrawImage(originalBMP, 0, 0, newWidth, newHeight);

                    // Save the new graphic file to the server
                    //newBMP = Blur(newBMP, 0.001);
                    newBMP.Save(tiffFilePath1, System.Drawing.Imaging.ImageFormat.Tiff);
                }
                response.Status = ServiceStatus.Success;
            }
            catch (Exception ex)
            {
                response.Status = ServiceStatus.Failure;
                response.Message = ex.Message;
            }
            ViewBag.Message = cnt;
            return Json(response);
        }

        public ActionResult SaveScanImage()
        {
            Response response = new Response();
            HttpPostedFileBase filesGryUp = null;
            HttpPostedFileBase filesBitUp = null;

            if (Request.Files.Count > 0)
            {
                filesGryUp = Request.Files[0];
                filesBitUp = Request.Files[1];
            }
            string gryFile, bitFile, gryExt, bitExt;
            gryFile = bitFile = gryExt = bitExt = "";
            if (filesGryUp.ContentLength > 0)
            {
                gryFile = Path.GetFileName(filesGryUp.FileName);
                gryExt = Path.GetExtension(filesGryUp.FileName);
                if (gryExt != ".jpg" && gryExt != ".jpeg")
                {
                    response.Message = "Invalid Grey Scale Mandate Image";
                    response.Status = ServiceStatus.ValidationError;
                    return Json(response);
                }
            }
            if (filesBitUp.ContentLength > 0)
            {
                bitFile = Path.GetFileName(filesBitUp.FileName);
                bitExt = Path.GetExtension(filesBitUp.FileName);
                if (bitExt != ".tif" && bitExt != ".tiff")
                {
                    response.Message = "Invalid Bitonal Mandate Image";
                    response.Status = ServiceStatus.ValidationError;
                    return Json(response);
                }
            }

            var frntGryImg = Guid.NewGuid();
            var frntBiotImg = frntGryImg + "_Bitonal";
            var scanPath = ConfigStartUp.MANDATE_SCAN_PATH;
            string filePath = scanPath + "\\" + frntGryImg + ".jpg";
            string filePath1 = scanPath + "\\" + frntBiotImg + ".tiff";// bitExt;
            filesGryUp.SaveAs(filePath);
            filesBitUp.SaveAs(filePath1);
            response.Status = ServiceStatus.Success;
            response.Message = "Mandate Uploaded successfully";
            return Json(response);
        }
        public static string FixBase64ForImage(string base64string)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(base64string, base64string.Length);
            sbText.Replace("\r\n", String.Empty);
            sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }

        private static Bitmap Blur(Bitmap image, double blurSize)
        {
            return Blur(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
        }

        private static Bitmap Blur(Bitmap image, Rectangle rectangle, double blurSize)
        {
            Bitmap blurred = new Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = Graphics.FromImage(blurred))
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            // look at every pixel in the blur rectangle
            for (int xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (int yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    int avgR = 0, avgG = 0, avgB = 0;
                    int blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (int x = xx; (x < xx + blurSize && x < image.Width); x++)
                    {
                        for (int y = yy; (y < yy + blurSize && y < image.Height); y++)
                        {
                            Color pixel = blurred.GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (int x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                        for (int y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                            blurred.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }
            return blurred;
        }
    }
}