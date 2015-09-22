using System;
using Android.App;   
using Android.Content;
using Android.Preferences;
using WB.Core.SharedKernels.Enumerator;
using WB.UI.Interviewer.SharedPreferences;

namespace WB.UI.Interviewer.Settings
{
    public class EnumeratorSettings : IEnumeratorSettings
    {
        private static ISharedPreferences SharedPreferences
        {
            get { return PreferenceManager.GetDefaultSharedPreferences(Application.Context); }
        }

        public string Endpoint
        {
            get
            {
                var defaultValue = Application.Context.Resources.GetString(Resource.String.Endpoint);
                var endpoint = SharedPreferences.GetString(SettingsNames.Endpoint, defaultValue);
                return endpoint;
            }
        }

        public TimeSpan Timeout
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.HttpResponseTimeout);
                string httpResponseTimeoutSec = SharedPreferences.GetString(SettingsNames.HttpResponseTimeout, defValue.ToString());

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
                string bufferSize = SharedPreferences.GetString(SettingsNames.BufferSize, defValue.ToString());

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
                string gpsReceiveTimeoutSec = SharedPreferences.GetString(SettingsNames.GpsReceiveTimeoutSec, defValue.ToString());

                int result;
                return int.TryParse(gpsReceiveTimeoutSec, out result) ? result : defValue;
            }
        }
    }
}