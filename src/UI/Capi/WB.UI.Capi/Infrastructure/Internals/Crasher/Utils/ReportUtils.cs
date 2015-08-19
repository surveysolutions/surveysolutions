using System;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Java.Lang;
using Environment = Android.OS.Environment;
using StringBuilder = System.Text.StringBuilder;

namespace Mono.Android.Crasher.Utils
{
    /// <summary>
    /// Responsible for providing base utilities used when constructing the report.
    /// </summary>
    class ReportUtils
    {
        /// <summary>
        /// Calculates the free memory of the device. This is based on an inspection of the filesystem, which in android
        /// devices is stored in RAM. Number of bytes available.
        /// </summary>
        public static long AvailableInternalMemorySize
        {
            get
            {
                var path = Environment.DataDirectory;
                var stat = new StatFs(path.Path);
                long blockSize = stat.BlockSize;
                long availableBlocks = stat.AvailableBlocks;
                return availableBlocks * blockSize;
            }
        }

        /// <summary>
        /// Calculates the total memory of the device. This is based on an inspection of the filesystem, which in android
        /// devices is stored in RAM. Total number of bytes.
        /// </summary>
        public static long TotalInternalMemorySize
        {
            get
            {
                var path = Environment.DataDirectory;
                var stat = new StatFs(path.Path);
                var blockSize = stat.BlockSize;
                var totalBlocks = stat.BlockCount;
                return totalBlocks * blockSize;
            }
        }

        /// <summary>
        /// Returns the DeviceId according to the <see cref="TelephonyManager"/>.
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported</param>
        /// <returns>Returns the DeviceId according to the <see cref="TelephonyManager"/> or null if there is no <see cref="TelephonyManager"/>.</returns>
        public static string GetDeviceId(Context context)
        {
            try
            {
                using (var tm = TelephonyManager.FromContext(context))
                {
                    return tm.DeviceId;
                }
            }
            catch (RuntimeException e)
            {
                Log.Warn(Constants.LOG_TAG, e, "Couldn't retrieve DeviceId for : " + context.PackageName);
                return null;
            }
        }

        /// <summary>
        /// Returns Application file path
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported</param>
        /// <returns>Returns Application file path</returns>
        public static string GetApplicationFilePath(Context context)
        {
            var filesDir = context.FilesDir;
            if (filesDir != null)
            {
                return filesDir.AbsolutePath;
            }
            Log.Warn(Constants.LOG_TAG, "Couldn't retrieve ApplicationFilePath for : " + context.PackageName);
            return "Couldn't retrieve ApplicationFilePath";
        }

        /// <summary>
        /// Returns a String representation of the content of a <see cref="Display"/> object.
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported.</param>
        /// <returns>A String representation of the content of the default Display of the Window Service.</returns>
        [Obsolete("IWindowManager is not accessible in MonoDroid 1.9.2")]
        public static string GetDisplayDetails(Context context)
        {
            try
            {
                var service = context.GetSystemService(Context.WindowService);
                var windowManager = service as IWindowManager;
                if (windowManager == null) return "Could not get WindowManager instance";

                var display = windowManager.DefaultDisplay;
                var metrics = new DisplayMetrics();
                display.GetMetrics(metrics);
                var result = new StringBuilder();
                result.Append("width=").Append(display.Width).Append('\n');
                result.Append("height=").Append(display.Height).Append('\n');
                result.Append("pixelFormat=").Append(display.PixelFormat).Append('\n');
                result.Append("refreshRate=").Append(display.RefreshRate).Append("fps").Append('\n');
                result.Append("metrics.density=x").Append(metrics.Density).Append('\n');
                result.Append("metrics.scaledDensity=x").Append(metrics.ScaledDensity).Append('\n');
                result.Append("metrics.widthPixels=").Append(metrics.WidthPixels).Append('\n');
                result.Append("metrics.heightPixels=").Append(metrics.HeightPixels).Append('\n');
                result.Append("metrics.xdpi=").Append(metrics.Xdpi).Append('\n');
                result.Append("metrics.ydpi=").Append(metrics.Ydpi);
                return result.ToString();

            }
            catch (RuntimeException e)
            {
                Log.Warn(Constants.LOG_TAG, e, "Couldn't retrieve DisplayDetails for : " + context.PackageName);
                return "Couldn't retrieve Display Details";
            }
        }

        /// <summary>
        /// Returns the current <see cref="Configuration"/> for this application.
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported.</param>
        /// <returns>A String representation of the current configuration for the application.</returns>
        public static string GetCrashConfiguration(Context context)
        {
            try
            {
                var crashConf = context.Resources.Configuration;
                return ConfigurationInspector.ToString(crashConf);
            }
            catch (RuntimeException e)
            {
                Log.Warn(Constants.LOG_TAG, e, "Couldn't retrieve CrashConfiguration for : " + context.PackageName);
                return "Couldn't retrieve crash config";
            }
        }
    }
}