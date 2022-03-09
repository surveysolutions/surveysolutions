using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Util;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Services;
using Xamarin.ANRWatchDog;

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
                            var result =  new List<ErrorAttachmentLog>();
                            var pathToLocalDirectory = AndroidPathUtils.GetPathToInternalDirectory();
                            var logsDirectory = Path.Combine(pathToLocalDirectory, "Logs");
                            
                            var lastLogFile = Path.Combine(logsDirectory, report.AppErrorTime.ToString("yyyy-MM-dd") + ".log");

                            if (File.Exists(lastLogFile) && new FileInfo(lastLogFile).Length < 2 * 1024 * 1024 /* 2mb */)
                            {
                                var readAllText = File.ReadAllText(lastLogFile);
                                string logsPrefix = string.Empty;
                                if (ServiceLocator.Current != null)
                                {
                                    var settings = ServiceLocator.Current.GetInstance<IEnumeratorSettings>();
                                    var settingsEndpoint = settings.Endpoint;
                                    logsPrefix += "HQ URL: " + settingsEndpoint + Environment.NewLine;

                                    var userId = ServiceLocator.Current.GetInstance<IPrincipal>();
                                    if (userId.IsAuthenticated)
                                    {
                                        logsPrefix += "User login: " + userId.CurrentUserIdentity.Name + Environment.NewLine;
                                    }
                                    else
                                    {
                                        logsPrefix += "User login: Anonymous" + Environment.NewLine;
                                    }
                                }


                                result.Add(ErrorAttachmentLog.AttachmentWithText(logsPrefix + readAllText, "Log.txt"));
                            }

                            var lastTraceFile = Path.Combine(logsDirectory, "Trace", "Trace.log");
                            if (File.Exists(lastTraceFile))
                            {
                                var readAllLines = File.ReadAllLines(lastTraceFile);
                                var lines = readAllLines.Skip(Math.Max(0, readAllLines.Length - 700));
                                
                                result.Add(ErrorAttachmentLog.AttachmentWithText(string.Join(Environment.NewLine, lines), "Trace.log"));
                            }
                            
                            return result;
                        };

                        Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);

                        AppCenter.Start(apiKey, typeof(Analytics), typeof(Crashes));

                        IsCrashReportingConfigured = true;
                        
                        
                        Log.Info("CrashReporting", "Starting ANRWatchDog");
                        new ANRWatchDog().SetANRListener(new ANRListener()).Start();
                        
                        Log.Info("CrashReporting", "Initialized appcenter");
                    }
                }
            }
        }
    }
}
