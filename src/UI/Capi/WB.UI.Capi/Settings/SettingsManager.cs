using System;
using Android.App;
using Android.Content;
using CAPI.Android.Settings;
using Java.Util;

namespace WB.UI.Capi.Settings
{
    public static class SettingsManager
    {
        #region Constants
        
        public static string GetRegistrationKey()
        {
            return GetSetting(SettingsNames.RegistrationKeyName);
        }

        private const string RemoteSyncNode = "";
        // "http://10.0.2.2";  //access to hosted computer from emulator

        #endregion

        #region Public Methods and Operators

        public static string AndroidId
        {
            get
            {
                return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                                                                          Android.Provider.Settings.Secure.AndroidId);
            }
        }

        
        public static string AppVersionName()
        {
            return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
        }

        public static int AppVersionCode()
        {
            return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionCode;
        }

        
        public static string GetSyncAddressPoint()
        {
            return GetSetting(SettingsNames.SyncAddressSettingsName, RemoteSyncNode);
        }

        public static bool SetSyncAddressPoint(string syncPoint)
        {
            syncPoint = syncPoint.Trim();

            if (!ValidateAddress(syncPoint))
            {
                return false;
            }

            SetSetting(SettingsNames.SyncAddressSettingsName, syncPoint);

            return true;
        }

        public static string GetSetting(string settingName)
        {
            return GetSetting(settingName, string.Empty, false);
        }

        public static string GetSetting(string settingName, string defaultValue)
        {
            return GetSetting(settingName, defaultValue, false);
        }

        public static string GetSetting(string settingName, bool ignoreCache)
        {
            return GetSetting(settingName, string.Empty, ignoreCache);
        }

        public static string GetSetting(string settingName, string defaultValue, bool ignoreCache)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private);
            return prefs.GetString(settingName, defaultValue);
        }

        public static void SetSetting(string settingName, string settingValue)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(SettingsNames.AppName, FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = prefs.Edit();
            prefEditor.PutString(settingName, settingValue);
            prefEditor.Commit();

        }

        public static bool ValidateAddress(string syncPoint)
        {
            Uri test = null;
            return Uri.TryCreate(syncPoint, UriKind.Absolute, out test)
                   && (test.Scheme == "http" || test.Scheme == "https");

        }

        public static bool CheckSyncPoint()
        {
            string syncPoint = GetSyncAddressPoint();

            if (!ValidateAddress(syncPoint))
            {
                return false;
            }
            return true;
        }

        public static string InstallationId {
            get { return Installation.ID; }
        }

        #endregion


        private class Installation
        {
            private static String sID = null;

            public static String ID
            {
                get
                {
                    {
                        if (sID == null)
                        {
                            try
                            {
                                var id = GetSetting(SettingsNames.INSTALLATION);
                                if (string.IsNullOrEmpty(id))
                                {
                                    id = UUID.RandomUUID().ToString();
                                    SetSetting(SettingsNames.INSTALLATION, id);
                                }
                                sID = id;
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException();
                            }
                        }
                        return sID;
                    }
                }
            }
        }
    }
}