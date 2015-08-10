using Android.App;
using Android.Content;
using Android.Preferences;
using WB.Core.BoundedContexts.Tester;

namespace WB.UI.Capi.Infrastructure.Internals.Settings
{
    internal class InterviewerSettings : IEnumeratorSettings
    {
        private const string GpsReceiveTimeoutSecParameterName = "GpsReceiveTimeoutSec";

        private static ISharedPreferences SharedPreferences
        {
            get
            {
                return PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            }
        }

        public int GpsReceiveTimeoutSec
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.GpsReceiveTimeoutSec);
                string gpsReceiveTimeoutSec = SharedPreferences.GetString(GpsReceiveTimeoutSecParameterName, defValue.ToString());
                int result;
                if (int.TryParse(gpsReceiveTimeoutSec, out result))
                {
                    return result;
                }

                return defValue;
            }
        }
    }
}
