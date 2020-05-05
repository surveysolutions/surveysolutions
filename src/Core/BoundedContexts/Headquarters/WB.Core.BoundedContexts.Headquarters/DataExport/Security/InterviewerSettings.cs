using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class InterviewerSettings : AppSetting
    {
        public const bool AutoUpdateEnabledDefault = true;
        public const bool DeviceNotificationsEnabledDefault = true;
        public const bool PartialSynchronizationEnabledDefault = false;

        public bool AutoUpdateEnabled { get; set; }
        public bool? DeviceNotificationsEnabled { get; set; }
        public bool? PartialSynchronizationEnabled { get; set; }
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
    }
}
