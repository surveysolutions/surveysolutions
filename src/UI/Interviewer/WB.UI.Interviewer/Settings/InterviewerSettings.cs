using System;
using Android.App;
using Android.Content;
using Android.Preferences;
using Java.Util;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Interviewer.SharedPreferences;

namespace WB.UI.Interviewer.Settings
{
    internal class InterviewerSettings : IInterviewerSettings
    {
        public string GetDeviceId()
        {
            return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
        }

        public Guid GetInstallationId()
        {
            var installationId = GetSetting(SettingsNames.INSTALLATION);
            if (string.IsNullOrEmpty(installationId))
            {
                installationId = UUID.RandomUUID().ToString();
                SetSetting(SettingsNames.INSTALLATION, installationId);    
            }
            return Guid.Parse(installationId);
        }

        public Guid? GetClientRegistrationId()
        {
            var sClientRegistrationId = GetSetting(SettingsNames.RegistrationKeyName);

            return string.IsNullOrEmpty(sClientRegistrationId) ? (Guid?) null : Guid.Parse(sClientRegistrationId);
        }

        public string GetSyncAddressPoint()
        {
            return GetSetting(SettingsNames.Endpoint);
        }

        public string GetApplicationVersionName()
        {
            return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
        }

        public int GetApplicationVersionCode()
        {
            return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionCode;
        }

        public string GetOperatingSystemVersion()
        {
            return global::Android.OS.Build.VERSION.Release;
        }

        public void SetClientRegistrationId(Guid? clientRegistrationId)
        {
            SetSetting(SettingsNames.RegistrationKeyName,
                clientRegistrationId.HasValue ? clientRegistrationId.ToString() : string.Empty);
        }

        public void SetSyncAddressPoint(string syncAddressPoint)
        {
            if (!syncAddressPoint.IsValidWebAddress())
            {
                throw new ArgumentException(Properties.Resources.InvalidSyncPointAddressUrl, "syncAddressPoint");
            }

            SetSetting(SettingsNames.Endpoint, syncAddressPoint.Trim());
        }


        private static string GetSetting(string settingName)
        {
            return GetSetting(settingName, String.Empty);
        }

        private static string GetSetting(string settingName, string defaultValue)
        {
            return SharedPreferences.GetString(settingName, defaultValue);
        }

        private static void SetSetting(string settingName, string settingValue)
        {
            ISharedPreferencesEditor prefEditor = SharedPreferences.Edit();
            prefEditor.PutString(settingName, settingValue);
            prefEditor.Commit();

        }

        private static ISharedPreferences SharedPreferences
        {
            get { return PreferenceManager.GetDefaultSharedPreferences(Application.Context); }
        }
    }
}
