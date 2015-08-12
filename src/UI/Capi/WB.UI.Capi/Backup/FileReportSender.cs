using System;
using System.IO;

using Android.App;

using Mono.Android.Crasher;
using Mono.Android.Crasher.Data;
using Mono.Android.Crasher.Data.Submit;

using WB.Core.GenericSubdomains.Portable.Services;

using Environment = Android.OS.Environment;

namespace WB.UI.Capi.Backup
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
            this.bugreports = Environment.ExternalStorageDirectory.AbsolutePath;
            if (Directory.Exists(this.bugreports))
            {
                this.bugreports = System.IO.Path.Combine(this.bugreports, BUGREPORTS);
                if (!Directory.Exists(this.bugreports))
                {
                    Directory.CreateDirectory(this.bugreports);
                }
                this.bugreports = System.IO.Path.Combine(this.bugreports, this.appName);
                if (!Directory.Exists(this.bugreports))
                    Directory.CreateDirectory(this.bugreports);
                this.filePath = System.IO.Path.Combine(this.bugreports, FILE_NAME);

            }
            else
            {
                this.filePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), FILE_NAME);
            }
            bool exists = File.Exists(this.filePath);
            if (!exists)
            {
                //File.CreateText(filePath);
                using (FileStream stOut = File.Open(this.filePath, FileMode.OpenOrCreate))
                {
                    byte[] pwBytesDate = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString());
                    stOut.Write(pwBytesDate, 0, pwBytesDate.Length);
                }
            }

            if (this.infoFileSupplierRegistry!=null)
                this.infoFileSupplierRegistry.RegisterConstant(this.filePath);
        }

        public void Send(ReportData errorContent)
        {
            using (FileStream stOut = File.Open(this.filePath, FileMode.Append))
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