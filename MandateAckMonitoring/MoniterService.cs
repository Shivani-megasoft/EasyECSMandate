using EASYECS.BussinessLayer.ECSService.Extension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MandateAckMonitoring
{
    public class MoniterService
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        delegate void invokeMethodDelegate(object sender, FileSystemEventArgs e);
        [STAThread()]
        static void Main()
        {
            //MandateUsynchService smsUsynchService = new MandateUsynchService();

            //string inputPath1 = ConfigurationManager.AppSettings["ACK_PATH"].ToString();
            ////string inputPath2 = ConfigurationManager.AppSettings["RES_PATH"].ToString();


            //FileSystemWatcher watcher1 = new FileSystemWatcher();
            //string _path1 = inputPath1;
            //watcher1.Path = _path1;
            //watcher1.EnableRaisingEvents = true;
            //watcher1.Created += new FileSystemEventHandler(watcher_Created);
            //watcher1.Filter = "*.zip";
            //Console.WriteLine("Watcher is ready and listening to changes in : " + _path1);
            Thread printer = new Thread(new ThreadStart(Run));
            printer.Start();


            //string inputPath2 = ConfigurationManager.AppSettings["RES_PATH"].ToString();
            //FileSystemWatcher watcher2 = new FileSystemWatcher();


            //string _path2 = inputPath2;
            //watcher2.Path = _path2;
            //watcher2.EnableRaisingEvents = true;
            //watcher2.Created += new FileSystemEventHandler(watcher_Created);
            //watcher2.Filter = "*.zip";
            //Console.WriteLine("FileSystemWatcher ready and listening to changes in :\n\n" + _path2);


            //invokeMethodDelegate mymethod = new invokeMethodDelegate(InvokeMethod);
            //Watcher w1 = new Watcher(inputPath1, "*.zip", mymethod);
            //w1.StartWatch();

            //Console.WriteLine("Watcher ready listening to changes in : " + inputPath1);

            //Watcher w2 = new Watcher(inputPath2, "*.zip", mymethod);
            //w2.StartWatch();
            //Console.WriteLine("Watcher ready listening to changes in : " + inputPath2);
            //Thread printer = new Thread(new ThreadStart(InvokeMethod));
            //printer.Start();

            new System.Threading.AutoResetEvent(false).WaitOne();
        }

        static void InvokeMethod()
        {
            //while (true)
            //{
            GetPendingFiles();
            //Thread.Sleep(30000); // 5 Minutes
            //Have a break condition
            //}
        }

        static void watcher_Created(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(5000);

            MandateUsynchService mandateUsynchService = new MandateUsynchService();
            mandateUsynchService.USynchAcknowledge(e.FullPath);
        }
        static void GetPendingFiles()
        {
            //SMSHistoryService smsHistoryService = new SMSHistoryService();
            //List<EbSmsHistry> smsDetailList = smsHistoryService.GetAllSMSHistry().Where(a => a.Status.ToLower() == "pending").ToList();
            MandateUsynchService unsyncService = new MandateUsynchService();
            unsyncService.GetAllPendingAck();
        }                                                                                                                                                                                                                               

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            string inputPath1 = ConfigurationManager.AppSettings["ACK_PATH"].ToString();
            string inputPath2 = ConfigurationManager.AppSettings["RES_PATH"].ToString();
            string[] args = new[] { inputPath1, inputPath2 };

            // If a directory is not specified, exit program.
            if (args.Length < 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: Watcher.exe PATH [...] [PATH]");
                return;
            }
            List<string> list = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                list.Add(args[i]);
            }
            foreach (string my_path in list)
            {
                Watch(my_path);
            }

            // Wait for the user to quit the program.
            Console.WriteLine("Press \'q\' to quit the sample.");
            while (Console.Read() != 'q') ;
        }
        private static void Watch(string watch_folder)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = watch_folder;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "*.zip";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            MandateUsynchService unsyncService = new MandateUsynchService();
            unsyncService.GetAllPendingAck();
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}
