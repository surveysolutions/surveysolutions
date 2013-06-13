// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsManager.cs" company="">
//   
// </copyright>
// <summary>
//   The settings manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using Android.App;
using Android.Content;
using Java.Util;

namespace CAPI.Android.Settings
{
    /// <summary>
    /// The settings manager.
    /// </summary>
    public static class SettingsManager
    {
        #region Constants

        /// <summary>
        /// The app name.
        /// </summary>
        private const string AppName = "CapiApp";

        /// <summary>
        /// The remote sync node.
        /// </summary>
        private const string RemoteSyncNode = // "http://ec2-54-217-244-125.eu-west-1.compute.amazonaws.com/";
            "http://192.168.173.1:8000/";

        // "http://217.12.197.135/DEV-Supervisor/";
        // "http://192.168.173.1:8084/";
        // "http://192.168.173.1:9089/";
        // "http://10.0.2.2:8084";

        /// <summary>
        /// The sync address settings name.
        /// </summary>
        private const string SyncAddressSettingsName = "SyncAddress";

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
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(AppName, FileCreationMode.Private);
            return prefs.GetString(SyncAddressSettingsName, RemoteSyncNode);
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

            // saving into the settings
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(AppName, FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = prefs.Edit();
            prefEditor.PutString(SyncAddressSettingsName, syncPoint);
            prefEditor.Commit();

            return true;
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
            private const String INSTALLATION = "INSTALLATION";

            public static String ID
            {
                get
                {
                    {
                        if (sID == null)
                        {
                            try
                            {
                                sID = ReadInstallationSetting();
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

            private static String ReadInstallationSetting()
            {
                ISharedPreferences prefs = Application.Context.GetSharedPreferences(AppName, FileCreationMode.Private);
                var retval= prefs.GetString(INSTALLATION, string.Empty);
                if (string.IsNullOrEmpty(retval))
                    return WriteInstallationSetting();
                return retval;
            }

            private static string WriteInstallationSetting()
            {
                ISharedPreferences prefs = Application.Context.GetSharedPreferences(AppName, FileCreationMode.Private);
                ISharedPreferencesEditor prefEditor = prefs.Edit();
                var sid = UUID.RandomUUID().ToString();
                prefEditor.PutString(sid, INSTALLATION);
                prefEditor.Commit();
                return sid;
            }

        }
    }
}