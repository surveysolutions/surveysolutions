namespace Mono.Android.Crasher
{
    public enum ReportField
    {
        /// <summary>
        /// Report Identifier
        /// </summary>
        ReportID,
        /// <summary>
        /// Application version code. This is the incremental integer version code
        /// used to differentiate versions on the android market.
        /// see android.content.pm.PackageInfo#versionCode
        /// </summary>
        AppVersionCode,
        /// <summary>
        /// Application version name.
        /// see android.content.pm.PackageInfo#versionName
        /// </summary>
        AppVersionName,
        /// <summary>
        /// Application package name.
        /// see android.content.Context#getPackageName()
        /// </summary>
        PackageName,
        /// <summary>
        /// Base path of the application's private file folder.
        /// see android.content.Context#getFilesDir()
        /// </summary>
        FilePath,
        /// <summary>
        /// Device model name.
        /// see android.os.Build#MODEL
        /// </summary>
        PhoneModel,
        /// <summary>
        /// Device android version name.
        /// see android.os.Build.VERSION#RELEASE
        /// </summary>
        AndroidVersion,
        /// <summary>
        /// Android Build details.
        /// see android.os.Build
        /// </summary>
        Build,
        /// <summary>
        /// Device brand (manufacturer or carrier).
        /// see android.os.Build#BRAND
        /// </summary>
        Brand,
        /// <summary>
        /// Device overall product code.
        /// see android.os.Build#PRODUCT
        /// </summary>
        Product,
        /// <summary>
        /// Estimation of the total device memory size based on filesystem stats.
        /// </summary>
        TotalMemSize,
        /// <summary>
        /// Estimation of the available device memory size based on filesystem stats.
        /// </summary>
        AvailableMemSize,
        /// <summary>
        /// The Holy Stack Trace.
        /// </summary>
        StackTrace,
        /// <summary>
        /// Fields state on the application start.
        /// see Configuration
        /// </summary>
        InitialConfiguration,
        /// <summary>
        /// Fields state on the application crash.
        /// see Configuration
        /// </summary>
        CrashConfiguration,
        /// <summary>
        /// Device display specifications.
        /// see android.view.WindowManager#getDefaultDisplay()
        /// </summary>
        Display,
        /// <summary>
        /// Comment added by the user in the CrashReportDialog displayed in
        /// ReportingInteractionMode#NOTIFICATION} mode.
        /// </summary>
        UserComment,
        /// <summary>
        /// User date on application start.
        /// </summary>
        UserAppStartDate,
        /// <summary>
        /// User date immediately after the crash occurred.
        /// </summary>
        UserCrashDate,
        /// <summary>
        /// Memory state details for the application process.
        /// </summary>
        DumpsysMeminfo,
        /// <summary>
        /// Logcat default extract. Requires READ_LOGS permission.
        /// </summary>
        Logcat,
        /// <summary>
        /// Logcat eventslog extract. Requires READ_LOGS permission.
        /// </summary>
        Eventslog,
        /// <summary>
        /// Logcat radio extract. Requires READ_LOGS permission.
        /// </summary>
        Radiolog,
        /// <summary>
        /// True if the report has been explicitly sent silently by the developer.
        /// </summary>
        IsSilent,
        /// <summary>
        /// Device unique ID (IMEI). Requires READ_PHONE_STATE permission.
        /// </summary>
        DeviceID,
        /// <summary>
        /// Installation unique ID. This identifier allow you to track a specific
        /// user application installation without using any personal data.
        /// </summary>
        InstallationID,
        /// <summary>
        /// Features declared as available on this device by the system.
        /// </summary>
        DeviceFeatures,
        /// <summary>
        /// External storage state and standard directories.
        /// </summary>
        Environment,
        /// <summary>
        /// System settings.
        /// </summary>
        SettingsSystem,
        /// <summary>
        /// Secure settings (applications can't modify them).
        /// </summary>
        SettingsSecure,
        /// <summary>
        /// SharedPreferences contents
        /// </summary>
        SharedPreferences
    }
}