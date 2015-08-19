using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Tester.Infrastructure.Internals.Settings
{
    internal class TesterSettings : ITesterSettings, IEnumeratorSettings
    {
        private const string ApplicationNameParameterName = "ApplicationName";
        private const string DesignerEndpointParameterName = "DesignerEndpointV9";
        private const string HttpResponseTimeoutParameterName = "HttpResponseTimeout";
        private const string BufferSizeParameterName = "BufferSize";
        private const string AcceptUnsignedSslCertificateParameterName = "AcceptUnsignedSslCertificate";
        private const string GpsReceiveTimeoutSecParameterName = "GpsReceiveTimeoutSec";

        private readonly ITesterExpressionsEngineVersionService versionService;

        public TesterSettings(ITesterExpressionsEngineVersionService versionService)
        {
            this.versionService = versionService;
        }

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

        public TimeSpan RequestTimeout
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

        public string ApplicationVersion
        {
            get
            {
                return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
            }
        }

        public string EngineVersion
        {
            get { return this.versionService.GetExpressionsEngineSupportedVersion().ToString(); }
        }

        public string OSVersion
        {
            get { return Build.VERSION.Release; }
        }

        public string ApplicationName
        {
            get { return SharedPreferences.GetString(ApplicationNameParameterName, Application.Context.Resources.GetString(Resource.String.ApplicationName)); }
        }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
