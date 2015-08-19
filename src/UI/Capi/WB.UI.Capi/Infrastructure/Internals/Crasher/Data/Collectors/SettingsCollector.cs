using System;
using System.Reflection;
using System.Text;
using Android.Content;
using Android.Provider;
using Android.Util;

namespace Mono.Android.Crasher.Data.Collectors
{
    /// <summary>
    /// Helper to collect data from <see cref="Settings.System"/> and <see cref="Settings.Secure"/> Settings classes.
    /// </summary>
    static class SettingsCollector
    {
        /// <summary>
        /// Collect data from <see cref="Settings.System"/>. 
        /// This collector uses reflection to be sure to always get the most accurate data
        /// whatever Android API level it runs on.
        /// </summary>
        /// <param name="ctx"><see cref="Context"/> for the application being reported.</param>
        /// <returns>A human readable String containing one key=value pair per line.</returns>
        public static string CollectSystemSettings(Context ctx)
        {
            var result = new StringBuilder();
            var fields = typeof(Settings.System).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    try
                    {
                        var value = Settings.System.GetString(ctx.ContentResolver, (string)field.GetValue(null));
                        if (value != null)
                        {
                            result.AppendFormat("{0}={1}", field.Name, value).AppendLine();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(Constants.LOG_TAG, Java.Lang.Throwable.FromException(e), e.Message);
                    }
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Collect data from <see cref="Settings.Secure"/>. 
        /// This collector uses reflection to be sure to always get the most accurate data
        /// whatever Android API level it runs on.
        /// </summary>
        /// <param name="ctx"><see cref="Context"/> for the application being reported.</param>
        /// <returns>A human readable String containing one key=value pair per line.</returns>
        public static string CollectSecureSettings(Context ctx)
        {
            var result = new StringBuilder();
            var fields = typeof(Settings.Secure).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.FieldType != typeof(string)) continue;
                try
                {
                    var value = Settings.Secure.GetString(ctx.ContentResolver, (string)field.GetValue(null));
                    if (value != null)
                    {
                        result.AppendFormat("{0}={1}", field.Name, value).AppendLine();
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(Constants.LOG_TAG, Java.Lang.Throwable.FromException(e), e.Message);
                }
            }
            return result.ToString();
        }

    }
}