using System;
using Android.App;
using Android.Content;
using Android.Preferences;
using WB.Core.SharedKernels.Enumerator;

namespace WB.UI.Tester.Infrastructure.Internals.Settings
{
    internal class TesterSettings : IEnumeratorSettings
    {
        private const string DesignerEndpointParameterName = "DesignerEndpointV13";
        private const string HttpResponseTimeoutParameterName = "HttpResponseTimeout";
        private const string BufferSizeParameterName = "BufferSize";
        private const string AcceptUnsignedSslCertificateParameterName = "AcceptUnsignedSslCertificate";
        private const string GpsReceiveTimeoutSecParameterName = "GpsReceiveTimeoutSec";
        private const string GpsDesiredAccuracyParameterName = "GpsDesiredAccuracy";

        private static ISharedPreferences SharedPreferences
        {
            get
            {
                return PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            }
        }

        public string Endpoint
        {
            get
            {
                var defaultValue = Application.Context.Resources.GetString(Resource.String.DesignerEndpoint);
                var endpoint = SharedPreferences.GetString(DesignerEndpointParameterName, defaultValue);
                return endpoint;
            }
        }

        public TimeSpan Timeout
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.HttpResponseTimeout);
                string httpResponseTimeoutSec = SharedPreferences.GetString(HttpResponseTimeoutParameterName, defValue.ToString());

                int result;

                return int.TryParse(httpResponseTimeoutSec, out result) ? new TimeSpan(0, 0, result) : new TimeSpan(0, 0, defValue);
            }
        }

        public int BufferSize
        {
            get { return SharedPreferences.GetInt(BufferSizeParameterName, Application.Context.Resources.GetInteger(Resource.Integer.BufferSize)); }
        }

        public bool AcceptUnsignedSslCertificate
        {
            get { return SharedPreferences.GetBoolean(AcceptUnsignedSslCertificateParameterName, 
                Application.Context.Resources.GetBoolean(Resource.Boolean.AcceptUnsignedSslCertificate)); }
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

        public double GpsDesiredAccuracy
        {
            get
            {
                var defValue = Application.Context.Resources.GetInteger(Resource.Integer.GpsDesiredAccuracy);
                string gpsReceiveTimeoutSec = SharedPreferences.GetString(GpsDesiredAccuracyParameterName, defValue.ToString());
                double result;
                if (double.TryParse(gpsReceiveTimeoutSec, out result))
                {
                    return result;
                }

                return defValue;
            }
        }
    }
}
