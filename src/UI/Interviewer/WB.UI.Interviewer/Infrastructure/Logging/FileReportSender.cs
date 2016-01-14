using System.IO;
using Android.App;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher.Data;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher.Data.Submit;

namespace WB.UI.Interviewer.Infrastructure.Logging
{
    public class FileReportSender : IReportSender
    {
        private readonly string LogFilePath;
        public FileReportSender(string pathToLogFile)
        {
            this.LogFilePath = pathToLogFile;
        }

        public void Initialize(Application application)
        {
            
        }

        public void Send(ReportData errorContent)
        {
            using (var fileStream = File.AppendText(this.LogFilePath))
            {
                fileStream.WriteLine($"{errorContent[ReportField.UserCrashDate]} {errorContent[ReportField.StackTrace]}");
            }
        }
    }
}