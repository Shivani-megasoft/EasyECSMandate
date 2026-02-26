using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using EasyBank.Framework.EasySecurity;
using System.Configuration;

namespace EASYECS.BussinessLayer.ECSService.Extension
{
    public static class UtilityExtensions//:AppController
    {
        
        //Get File Data in Bytes
        public static byte[] GetFileData(this string fileName, string filePath)
        {
            var fullFilePath = string.Format("{0}/{1}", filePath, fileName);
            if (!File.Exists(fullFilePath))
                throw new FileNotFoundException("The file does not exist.",
                fullFilePath);
            return File.ReadAllBytes(fullFilePath);
        }

        public static Image ConvertTiffToJPG(string fileName, ref string imgFileName)
        {
            Image bitmapImg = null;

            try
            {
                IPersonateuser();
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
                            bitmapImg = Image.FromFile(jpegPaths[frame]);

                            imgFileName = jpegPaths[frame];
                        }
                    }
                    return bitmapImg;
                }
            }
            catch(Exception ex)
            {
                CreateLog(ex.Message + ex.StackTrace);
            }

            return bitmapImg;
        }

       
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

            UtilityExtensions.CreateLog(domain + "-" + userName + "-" + pwd );
            iU.Impersonate(domain, userName, pwd);

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
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.Create(path).Dispose();
            }

            using (StreamWriter writer = System.IO.File.AppendText(path))
            {
                writer.WriteLine(logText);
                writer.Flush();
                writer.Close();
            }
        }
    }
}