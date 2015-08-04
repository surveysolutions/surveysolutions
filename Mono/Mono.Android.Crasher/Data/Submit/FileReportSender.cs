using System;
using System.IO;
using Android.App;

using WB.Core.GenericSubdomains.Portable.Services;

using Environment = Android.OS.Environment;

namespace Mono.Android.Crasher.Data.Submit
{
    public class FileReportSender : IReportSender
    {
        public FileReportSender(string appName, IInfoFileSupplierRegistry infoFileSupplierRegistry)
        {
            this.appName = appName;
            this.infoFileSupplierRegistry = infoFileSupplierRegistry;
        }

        public FileReportSender(string appName)
            : this(appName,null)
        {
        }

        private readonly IInfoFileSupplierRegistry infoFileSupplierRegistry;
        private readonly string appName;
        private string filePath;
        private const string FILE_NAME = "CAPI_log.txt";
        private const string BUGREPORTS = "bugreports";
        private string bugreports;
        #region Implementation of IReportSender

        public void Initialize(Application application)
        {
            bugreports = Environment.ExternalStorageDirectory.AbsolutePath;
            if (Directory.Exists(bugreports))
            {
                bugreports = System.IO.Path.Combine(bugreports, BUGREPORTS);
                if (!Directory.Exists(bugreports))
                {
                    Directory.CreateDirectory(bugreports);
                }
                bugreports = System.IO.Path.Combine(bugreports, appName);
                if (!Directory.Exists(bugreports))
                    Directory.CreateDirectory(bugreports);
                filePath = System.IO.Path.Combine(bugreports, FILE_NAME);

            }
            else
            {
                filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), FILE_NAME);
            }
            bool exists = File.Exists(filePath);
            if (!exists)
            {
                //File.CreateText(filePath);
                using (FileStream stOut = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    byte[] pwBytesDate = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString());
                    stOut.Write(pwBytesDate, 0, pwBytesDate.Length);
                }
            }

            if (infoFileSupplierRegistry!=null)
                infoFileSupplierRegistry.RegisterConstant(filePath);
        }

        public void Send(ReportData errorContent)
        {
            using (FileStream stOut = File.Open(filePath, FileMode.Append))
            {
                byte[] pwBytesDate = System.Text.Encoding.UTF8.GetBytes(errorContent[ReportField.UserCrashDate]);
                stOut.Write(pwBytesDate, 0, pwBytesDate.Length);
                byte[] pwBytes = System.Text.Encoding.UTF8.GetBytes(errorContent[ReportField.StackTrace]);
                stOut.Write(pwBytes, 0, pwBytes.Length);
            }
        }

        #endregion
    }
}