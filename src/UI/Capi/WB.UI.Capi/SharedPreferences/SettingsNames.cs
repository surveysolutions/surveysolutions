using System;

namespace WB.UI.Capi.SharedPreferences
{
    public class SettingsNames
    {
        public const string AppName = "CapiApp";
        public const string SyncAddressSettingsName = "SyncAddress";
        public const string RegistrationKeyName = "RegistrationKey";
        public const string INSTALLATION = "INSTALLATION";

        [Obsolete]
        public const string LastTimestamp = "LastTimestamp";

        [Obsolete]
        public static readonly string ReceivedChunkIds = "ReceivedChunkIds";
    }
}