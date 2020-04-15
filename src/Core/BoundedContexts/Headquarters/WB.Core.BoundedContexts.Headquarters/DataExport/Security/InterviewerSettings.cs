using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class InterviewerSettings : AppSetting
    {
        public const bool AutoUpdateEnabledDefault = true;
        public const bool DeviceNotificationsEnabledDefault = true;
        public const bool CustomSynchronizationEnabledDefault = false;

        public bool AutoUpdateEnabled { get; set; }
        public bool? DeviceNotificationsEnabled { get; set; }
        public bool? CustomSynchronizationEnabled { get; set; }
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

        public static bool IsCustomSynchronizationEnabled(this InterviewerSettings settings)
        {
            if (settings?.CustomSynchronizationEnabled == null)
                return InterviewerSettings.CustomSynchronizationEnabledDefault;

            return settings.CustomSynchronizationEnabled.Value;
        }
    }
}
