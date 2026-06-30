using System;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class InterviewerSettings : AppSetting
    {
        public const bool AutoUpdateEnabledDefault = true;
        public const bool DeviceNotificationsEnabledDefault = true;
        public const bool PartialSynchronizationEnabledDefault = false;
        public const int GeographyQuestionAccuracyInMetersDefault = 10;
        public const int GeographyQuestionPeriodInSecondsDefault = 10;
        public const bool AllowSupervisorChangeAssignmentStatusDefault = true;
        public const bool AllowInterviewerChangeAssignmentStatusDefault = true;

        public bool AutoUpdateEnabled { get; set; }
        public bool? DeviceNotificationsEnabled { get; set; }
        public bool? PartialSynchronizationEnabled { get; set; }
        public int? GeographyQuestionAccuracyInMeters { get; set; }
        public int? GeographyQuestionPeriodInSeconds { get; set; }
        
        public string EsriApiKey { get; set; }

        public bool? AllowSupervisorChangeAssignmentStatus { get; set; }
        public bool? AllowInterviewerChangeAssignmentStatus { get; set; }
    }

    public static class InterviewerSettingsExtensions
    {
        public static bool IsAutoUpdateEnabled(this InterviewerSettings settings)
        {
            if (settings == null) return InterviewerSettings.AutoUpdateEnabledDefault;

            return settings.AutoUpdateEnabled;
        }

        public static bool IsDeviceNotificationsEnabled(this InterviewerSettings settings)
        {
            if (settings?.DeviceNotificationsEnabled == null)
                return InterviewerSettings.DeviceNotificationsEnabledDefault;

            return settings.DeviceNotificationsEnabled.Value;
        }

        public static bool IsPartialSynchronizationEnabled(this InterviewerSettings settings)
        {
            if (settings?.PartialSynchronizationEnabled == null)
                return InterviewerSettings.PartialSynchronizationEnabledDefault;

            return settings.PartialSynchronizationEnabled.Value;
        }
        
        public static int GetGeographyQuestionAccuracyInMeters(this InterviewerSettings settings)
        {
            if (settings?.GeographyQuestionAccuracyInMeters == null)
                return InterviewerSettings.GeographyQuestionAccuracyInMetersDefault;

            return settings.GeographyQuestionAccuracyInMeters.Value;
        }

        public static int GetGeographyQuestionPeriodInSeconds(this InterviewerSettings settings)
        {
            if (settings?.GeographyQuestionPeriodInSeconds == null)
                return InterviewerSettings.GeographyQuestionPeriodInSecondsDefault;

            return settings.GeographyQuestionPeriodInSeconds.Value;
        }
        
        public static string GetEsriApiKey(this InterviewerSettings settings)
        {
            if (settings?.EsriApiKey == null)
                return String.Empty;

            return settings.EsriApiKey;
        }

        public static bool IsAllowSupervisorChangeAssignmentStatus(this InterviewerSettings settings)
        {
            if (settings?.AllowSupervisorChangeAssignmentStatus == null)
                return InterviewerSettings.AllowSupervisorChangeAssignmentStatusDefault;

            return settings.AllowSupervisorChangeAssignmentStatus.Value;
        }

        public static bool IsAllowInterviewerChangeAssignmentStatus(this InterviewerSettings settings)
        {
            if (settings?.AllowInterviewerChangeAssignmentStatus == null)
                return InterviewerSettings.AllowInterviewerChangeAssignmentStatusDefault;

            // Interviewer setting is only meaningful when supervisor setting is also on
            if (settings.AllowSupervisorChangeAssignmentStatus == false)
                return false;

            return settings.AllowInterviewerChangeAssignmentStatus.Value;
        }
    }
}
