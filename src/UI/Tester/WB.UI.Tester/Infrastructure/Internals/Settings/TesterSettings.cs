using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Preferences;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Tester.Infrastructure.Internals.Settings
{
    internal class TesterSettings : IEnumeratorSettings
    {
        //settings could be backed up by OS to Google drive automaticaly
        internal const string DesignerEndpointParameterName = "DesignerEndpointGen1";
        private const string HttpResponseTimeoutParameterName = "HttpResponseTimeout";
        private const string BufferSizeParameterName = "BufferSize";
        private const string AcceptUnsignedSslCertificateParameterName = "AcceptUnsignedSslCertificate";
        private const string GpsReceiveTimeoutSecParameterName = "GpsReceiveTimeoutSec";
        private const string GpsDesiredAccuracyParameterName = "GpsDesiredAccuracy";
        internal const string VibrateOnErrorParameterName = "VibrateOnError";
        internal const string ShowVariablesParamterName = "ShowVariables";
        internal const string ShowLocationOnMapParamName = "ShowLocationOnMap";
        internal const string ShowAnsweringTimeName = "ShowAnsweringTime";

        private static ISharedPreferences SharedPreferences => PreferenceManager.GetDefaultSharedPreferences(Application.Context);

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

        public int BufferSize =>
            SharedPreferences.GetInt(BufferSizeParameterName, Application.Context.Resources.GetInteger(Resource.Integer.BufferSize));

        public bool AcceptUnsignedSslCertificate => SharedPreferences.GetBoolean(AcceptUnsignedSslCertificateParameterName, 
            Application.Context.Resources.GetBoolean(Resource.Boolean.AcceptUnsignedSslCertificate));

        private string GetApplicationVersionName()
        {
            var packageInfo = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0);
            return packageInfo.VersionName;
        }

        public string UserAgent
        {
            get
            {
                var flags = new List<string>();
#if DEBUG
                flags.Add("DEBUG");
#endif
                if (AcceptUnsignedSslCertificate)
                {
                    flags.Add("UNSIGNED_SSL");
                }
                
                return $"{Application.Context.PackageName}/{this.GetApplicationVersionName()} ({string.Join(" ", flags)})";
            }
        }

        public Version GetSupportedQuestionnaireContentVersion() => null;

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

        public bool VibrateOnError
        {
            get
            {
                var defValue = Application.Context.Resources.GetBoolean(Resource.Boolean.VibrateOnError);
                return SharedPreferences.GetBoolean(VibrateOnErrorParameterName, defValue);
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
        public int EventChunkSize => 1000;

        public bool ShowVariables => SharedPreferences.GetBoolean(ShowVariablesParamterName, false);

        public bool ShowLocationOnMap => SharedPreferences.GetBoolean(ShowLocationOnMapParamName, true);

        public bool ShowAnswerTime => SharedPreferences.GetBoolean(ShowAnsweringTimeName, false);
        public long? LastHqSyncTimestamp { get; } = null;
        public void SetLastHqSyncTimestamp(long? lastHqSyncTimestamp)
        {
            throw new NotImplementedException();
        }
    }
}
