using System.Collections.Generic;
using System.IO;
using Android.Util;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Utils
{
    public class CrashReporting
    {
        public static bool IsCrashReportingConfigured;
        public static object CrashLockObject = new object();

        public static void Init(string apiKey)
        {
            Log.Verbose("CrashReporting", $"Init IsCrashReportingConfigured:{IsCrashReportingConfigured}");
            if (!IsCrashReportingConfigured)
            {
                lock (CrashLockObject)
                {
                    if (!IsCrashReportingConfigured)
                    {
                        Crashes.GetErrorAttachments = report =>
                        {
                            Log.Verbose("CrashReporting", "GetErrorAttachments");
                            var result =  new List<ErrorAttachmentLog>();
                            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();
                            
                            var lastLogFile = Path.Combine(pathToLocalDirectory, "Logs", report.AppErrorTime.ToString("yyyy-MM-dd") + ".log");

                            Log.Debug("CrashReporting", $"Appending appcenter attachment {lastLogFile}, exists: {File.Exists(lastLogFile)}");
                            if (File.Exists(lastLogFile)&& new FileInfo(lastLogFile).Length < 2 * 1024 * 1024)
                            {
                                /* 2mb */
                                result.Add(ErrorAttachmentLog.AttachmentWithText(File.ReadAllText(lastLogFile), "Log.txt"));
                            }

                            return result;
                        };
                        Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);

                        AppCenter.Start(apiKey, typeof(Analytics), typeof(Crashes));

                        IsCrashReportingConfigured = true;
                        Log.Info("CrashReporting", "Initialized appcenter");
                    }
                }
            }
        }
    }
}
