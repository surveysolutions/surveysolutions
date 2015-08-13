using Android.Content;
using Android.OS;

namespace Mono.Android.Crasher.Data
{
    class Compatibility
    {
        /// <summary>
        /// Retrieves Android SDK API level using the best possible method
        /// </summary>
        public static int ApiLevel
        {
            get
            {
                return (int)Build.VERSION.SdkInt;
            }
        }

        /// <summary>
        /// Retrieve the DropBoxManager service name using reflection API.
        /// </summary>
        public static string DropBoxServiceName
        {
            get
            {
                var serviceName = typeof(Context).GetField("DropboxService");
                return serviceName == null ? string.Empty : serviceName.GetValue(null) as string;
            }
        }
    }
}