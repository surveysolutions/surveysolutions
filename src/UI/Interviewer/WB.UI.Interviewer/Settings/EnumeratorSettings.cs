using System;
using Android.App;   
using Android.Content;
using Android.Preferences;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;

namespace WB.UI.Interviewer.Settings
{
    public class EnumeratorSettings : IRestServiceSettings, IEnumeratorSettings
    {
        private const string EndpointParameterName = "Endpoint";
        private const string HttpResponseTimeoutParameterName = "HttpResponseTimeout";
        private const string BufferSizeParameterName = "BufferSize";
        private const string GpsReceiveTimeoutSecParameterName = "GpsReceiveTimeoutSec";

        private static ISharedPreferences SharedPreferences
        {
            get { return PreferenceManager.GetDefaultSharedPreferences(Application.Context); }
        }

        public string Endpoint
        {
            get
            {
                var defaultValue = Application.Context.Resources.GetString(Resource.String.Endpoint);
                var endpoint = SharedPreferences.GetString(EndpointParameterName, defaultValue);
                return endpoint;
            }
        }

        public TimeSpan Timeout
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.HttpResponseTimeout);
                string httpResponseTimeoutSec = SharedPreferences.GetString(HttpResponseTimeoutParameterName,
                    defValue.ToString());

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
                string bufferSize = SharedPreferences.GetString(BufferSizeParameterName,
                    defValue.ToString());
                int result;
                if (int.TryParse(bufferSize, out result))
                {
                    return result;
                }

                return defValue;
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
                string gpsReceiveTimeoutSec = SharedPreferences.GetString(GpsReceiveTimeoutSecParameterName,
                    defValue.ToString());
                int result;
                if (int.TryParse(gpsReceiveTimeoutSec, out result))
                {
                    return result;
                }

                return defValue;
            }
        }

        public string ApplicationVersion
        {
            get
            {
                return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
            }
        }
    }
}