using System;
using System.Linq;
using Android;
using Android.Content;
using Android.OS;
using Android.Text.Format;
using Android.Util;
using Mono.Android.Crasher.Data.Collectors;
using Mono.Android.Crasher.Utils;
using Environment = Android.OS.Environment;

namespace Mono.Android.Crasher.Data
{
    /// <summary>
    /// Responsible for creating the <see cref="ReportData"/> for an <see cref="Exception"/>.
    /// </summary>
    static class ReportDataFactory
    {
        /// <summary>
        /// Collects crash data.
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported.</param>
        /// <param name="reportFields"><see cref="Array"/> of <see cref="ReportField"/> to include in report</param>
        /// <param name="appStartDate"><see cref="Time"/> of application start</param>
        /// <param name="initialConfiguration">Application initial configuration</param>
        /// <param name="th"><see cref="Java.Lang.Throwable"/> that caused the crash.</param>
        /// <param name="isSilentReport">Whether to report this report as being sent silently.</param>
        /// <returns>Builded report data</returns>
        public static ReportData BuildReportData(Context context, ReportField[] reportFields, Time appStartDate, string initialConfiguration, Java.Lang.Throwable th, bool isSilentReport)
        {
            var crashReportData = new ReportData();
            try
            {
                crashReportData.Add(ReportField.StackTrace, th.StackTrace);
                crashReportData.Add(ReportField.UserAppStartDate, appStartDate.Format3339(false));

                if (isSilentReport)
                {
                    crashReportData.Add(ReportField.IsSilent, "true");
                }

                if (reportFields.Contains(ReportField.ReportID))
                {
                    crashReportData.Add(ReportField.ReportID, Guid.NewGuid().ToString());
                }

                if (reportFields.Contains(ReportField.InstallationID))
                {
                    crashReportData.Add(ReportField.InstallationID, Installation.Id(context));
                }

                if (reportFields.Contains(ReportField.InitialConfiguration))
                {
                    crashReportData.Add(ReportField.InitialConfiguration, initialConfiguration);
                }

                if (reportFields.Contains(ReportField.CrashConfiguration))
                {
                    crashReportData.Add(ReportField.CrashConfiguration, ReportUtils.GetCrashConfiguration(context));
                }

                if (reportFields.Contains(ReportField.DumpsysMeminfo))
                {
                    crashReportData.Add(ReportField.DumpsysMeminfo, DumpSysCollector.CollectMemInfo());
                }

                if (reportFields.Contains(ReportField.PackageName))
                {
                    crashReportData.Add(ReportField.PackageName, context.PackageName);
                }

                if (reportFields.Contains(ReportField.Build))
                {
                    crashReportData.Add(ReportField.Build, ReflectionCollector.CollectStaticProperties(typeof(Build)));
                }

                if (reportFields.Contains(ReportField.PhoneModel))
                {
                    crashReportData.Add(ReportField.PhoneModel, Build.Model);
                }

                if (reportFields.Contains(ReportField.AndroidVersion))
                {
                    crashReportData.Add(ReportField.AndroidVersion, Build.VERSION.Release);
                }

                if (reportFields.Contains(ReportField.Brand))
                {
                    crashReportData.Add(ReportField.Brand, Build.Brand);
                }

                if (reportFields.Contains(ReportField.Product))
                {
                    crashReportData.Add(ReportField.Product, Build.Product);
                }

                if (reportFields.Contains(ReportField.TotalMemSize))
                {
                    crashReportData.Add(ReportField.TotalMemSize, ReportUtils.TotalInternalMemorySize.ToString());
                }

                if (reportFields.Contains(ReportField.AvailableMemSize))
                {
                    crashReportData.Add(ReportField.AvailableMemSize, ReportUtils.AvailableInternalMemorySize.ToString());
                }

                if (reportFields.Contains(ReportField.FilePath))
                {
                    crashReportData.Add(ReportField.FilePath, ReportUtils.GetApplicationFilePath(context));
                }

                if (reportFields.Contains(ReportField.Display))
                {
                    crashReportData.Add(ReportField.Display, ReportUtils.GetDisplayDetails(context));
                }

                if (reportFields.Contains(ReportField.UserCrashDate))
                {
                    var curDate = new Time();
                    curDate.SetToNow();
                    crashReportData.Add(ReportField.UserCrashDate, curDate.Format3339(false));
                }

                if (reportFields.Contains(ReportField.DeviceFeatures))
                {
                    crashReportData.Add(ReportField.DeviceFeatures, DeviceFeaturesCollector.GetFeatures(context));
                }

                if (reportFields.Contains(ReportField.Environment))
                {
                    crashReportData.Add(ReportField.Environment, ReflectionCollector.CollectStaticProperties(typeof(Environment)));
                }

                if (reportFields.Contains(ReportField.SettingsSystem))
                {
                    crashReportData.Add(ReportField.SettingsSystem, SettingsCollector.CollectSystemSettings(context));
                }

                if (reportFields.Contains(ReportField.SettingsSecure))
                {
                    crashReportData.Add(ReportField.SettingsSecure, SettingsCollector.CollectSecureSettings(context));
                }

                if (reportFields.Contains(ReportField.SharedPreferences))
                {
                    crashReportData.Add(ReportField.SharedPreferences, SharedPreferencesCollector.Collect(context));
                }

                var pm = new PackageManagerWrapper(context);
                var pi = pm.GetPackageInfo();
                if (pi != null)
                {
                    if (reportFields.Contains(ReportField.AppVersionCode))
                    {
                        crashReportData.Add(ReportField.AppVersionCode, pi.VersionCode.ToString());
                    }
                    if (reportFields.Contains(ReportField.AppVersionName))
                    {
                        crashReportData.Add(ReportField.AppVersionName, pi.VersionName ?? "not set");
                    }
                }
                else
                {
                    crashReportData.Add(ReportField.AppVersionName, "Package info unavailable");
                }

                if (reportFields.Contains(ReportField.DeviceID) && pm.HasPermission(Manifest.Permission.ReadPhoneState))
                {
                    var deviceId = ReportUtils.GetDeviceId(context);
                    if (deviceId != null)
                    {
                        crashReportData.Add(ReportField.DeviceID, deviceId);
                    }
                }

                if (pm.HasPermission(Manifest.Permission.ReadLogs))
                {
                    Log.Info(Constants.LOG_TAG, "READ_LOGS granted! Crasher can include LogCat and DropBox data.");
                    if (reportFields.Contains(ReportField.Logcat))
                    {
                        crashReportData.Add(ReportField.Logcat, LogCatCollector.CollectLogCat(null));
                    }
                    if (reportFields.Contains(ReportField.Eventslog))
                    {
                        crashReportData.Add(ReportField.Eventslog, LogCatCollector.CollectLogCat("events"));
                    }
                    if (reportFields.Contains(ReportField.Radiolog))
                    {
                        crashReportData.Add(ReportField.Radiolog, LogCatCollector.CollectLogCat("radio"));
                    }
                }
                else
                {
                    Log.Info(Constants.LOG_TAG, "READ_LOGS not allowed. Crasher will not include LogCat and DropBox data.");
                }

            }
            catch (Java.Lang.RuntimeException e)
            {
                Log.Error(Constants.LOG_TAG, e, "Error while retrieving crash data");
            }
            return crashReportData;
        }
    }
}