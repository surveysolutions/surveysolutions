using System;
using Android.App;
using Android.Content;
using CAPI.Android.Settings;
using Java.Util;

namespace WB.UI.Capi.Settings
{
    internal class InterviewerSettings : IInterviewerSettings
    {
        public string GetDeviceId()
        {
            return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);
        }

        public string GetInstallationId()
        {
            var installationId = GetSetting(SettingsNames.INSTALLATION);
            if (!string.IsNullOrEmpty(installationId)) return installationId;

            installationId = UUID.RandomUUID().ToString();
            SetSetting(SettingsNames.INSTALLATION, installationId);
            return installationId;
        }

        public string GetClientRegistrationId()
        {
            return GetSetting(SettingsNames.RegistrationKeyName);
        }

        public string GetLastReceivedPackageId()
        {
            return GetSetting(SettingsNames.LastTimestamp);
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

        public void SetClientRegistrationId(string clientRegistrationId)
        {
            SetSetting(SettingsNames.RegistrationKeyName, clientRegistrationId);
        }

        public void SetLastReceivedPackageId(string lastReceivedPackageId)
        {
            SetSetting(SettingsNames.LastTimestamp, lastReceivedPackageId);
        }

        public void SetSyncAddressPoint(string syncAddressPoint)
        {
            Uri syncUrl;
            if (!(Uri.TryCreate(syncAddressPoint, UriKind.Absolute, out syncUrl) && (syncUrl.Scheme == "http" || syncUrl.Scheme == "https")))
            {
                throw new ArgumentException();
            }

            SetSetting(SettingsNames.SyncAddressSettingsName, syncAddressPoint);
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
