using System;
using Android.App;
using Android.Content;

using Java.Util;

using WB.Core.BoundedContexts.Capi.Services;
using WB.UI.Capi.Properties;
using WB.UI.Capi.SharedPreferences;

namespace WB.UI.Capi.Settings
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
            return GetSetting(SettingsNames.SyncAddressSettingsName);
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
            Uri syncUrl;
            if (
                !(Uri.TryCreate(syncAddressPoint, UriKind.Absolute, out syncUrl) &&
                  (syncUrl.Scheme == "http" || syncUrl.Scheme == "https")))
            {
                throw new ArgumentException(Resources.InvalidSyncPointAddressUrl, "syncAddressPoint");
            }

            SetSetting(SettingsNames.SyncAddressSettingsName, syncAddressPoint.Trim());
        }


        private static string GetSetting(string settingName)
        {
            return GetSetting(settingName, String.Empty);
        }

        private static string GetSetting(string settingName, string defaultValue)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private);
            return prefs.GetString(settingName, defaultValue);
        }

        private static void SetSetting(string settingName, string settingValue)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = prefs.Edit();
            prefEditor.PutString(settingName, settingValue);
            prefEditor.Commit();

        }
    }
}
