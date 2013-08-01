using System;
using Android.App;
using Android.Content;
using Java.Util;

namespace CAPI.Android.Settings
{
    public static class SettingsManager
    {
        #region Constants
        
        public static string GetRegistrationKey()
        {
            return GetSetting(SettingsNames.RegistrationKeyName);
        }

        /// <summary>
        /// The remote sync node.
        /// </summary>
        private const string RemoteSyncNode = 
            "http://ec2-54-217-244-125.eu-west-1.compute.amazonaws.com/";
        //"http://192.168.173.1:8000/";
        // "http://217.12.197.135/DEV-Supervisor/";
        // "http://192.168.173.1:8084/";
        // "http://192.168.173.1:9089/";
        // "http://10.0.2.2:8084";

        
        #endregion

        #region Public Methods and Operators

        public static string AndroidId
        {
            get
            {
                return global::Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver,
                                                                          global::Android.Provider.Settings
                                                                                .Secure.AndroidId);
            }
        }

        /// <summary>
        /// The get sync address point.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetSyncAddressPoint()
        {
            return GetSetting(SettingsNames.SyncAddressSettingsName, RemoteSyncNode);
        }

        /// <summary>
        /// The set sync address point.
        /// </summary>
        /// <param name="syncPoint">
        /// The sync point.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool SetSyncAddressPoint(string syncPoint)
        {
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
            string syncPoint = SettingsManager.GetSyncAddressPoint();

            if (!SettingsManager.ValidateAddress(syncPoint))
            {
                return false;
            }
            return true;
        }

        public static string InstallationId {
            get { return Installation.ID; }
        }

        #endregion


        class Installation
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
                                var id = SettingsManager.GetSetting(SettingsNames.INSTALLATION);
                                if (string.IsNullOrEmpty(id))
                                {
                                    id = UUID.RandomUUID().ToString();
                                    SettingsManager.SetSetting(SettingsNames.INSTALLATION, id);
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