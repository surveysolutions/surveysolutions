using System;
using Android.App;
using Android.Content;
using Android.Preferences;
using Java.Util;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.UI.Interviewer.SharedPreferences;

namespace WB.UI.Interviewer.Settings
{
    internal class InterviewerSettings : IInterviewerSettings
    {
        protected static ISharedPreferences NewSharedPreferences
        {
            get { return PreferenceManager.GetDefaultSharedPreferences(Application.Context); }
        }

        protected static ISharedPreferences OldSharedPreferences
        {
            get { return Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private); }
        }

        public string Endpoint
        {
            get
            {
                var endpoint = GetSetting(SettingsNames.Endpoint, string.Empty);
                return endpoint;
            }
        }

        public TimeSpan Timeout
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.HttpResponseTimeout);
                string httpResponseTimeoutSec = GetSetting(SettingsNames.HttpResponseTimeout, defValue.ToString());

                int result;
                return int.TryParse(httpResponseTimeoutSec, out result)
                    ? new TimeSpan(0, 0, result)
                    : new TimeSpan(0, 0, defValue);
            }
        }

        public int BufferSize
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.BufferSize);
                string bufferSize = GetSetting(SettingsNames.BufferSize, defValue.ToString());

                int result;
                return int.TryParse(bufferSize, out result) ? result : defValue;
            }
        }

        public bool AcceptUnsignedSslCertificate
        {
            get { return false; }
        }

        public int GpsReceiveTimeoutSec
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.GpsReceiveTimeoutSec);
                string gpsReceiveTimeoutSec = GetSetting(SettingsNames.GpsReceiveTimeoutSec, defValue.ToString());

                int result;
                return int.TryParse(gpsReceiveTimeoutSec, out result) ? result : defValue;
            }
        }

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
            SetSetting(SettingsNames.Endpoint, syncAddressPoint.Trim());
        }


        private static string GetSetting(string settingName)
        {
            return GetSetting(settingName, String.Empty);
        }

        private static string GetSetting(string settingName, string defaultValue)
        {
            var  newPreference = NewSharedPreferences.GetString(settingName, defaultValue);
            return !string.IsNullOrEmpty(newPreference)
                ? newPreference
                : OldSharedPreferences.GetString(settingName, defaultValue);
        }

        private static void SetSetting(string settingName, string settingValue)
        {
            ISharedPreferencesEditor newPrefEditor = NewSharedPreferences.Edit();
            newPrefEditor.PutString(settingName, settingValue);
            newPrefEditor.Commit();

            ISharedPreferencesEditor oldPrefEditor = OldSharedPreferences.Edit();
            oldPrefEditor.PutString(settingName, settingValue);
            oldPrefEditor.Commit();
        }
    }
}
