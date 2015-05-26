using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Settings
{
    internal class ApplicationSettings : ISettingsProvider
    {
        private const string ApplicationNameParameterName = "ApplicationName";
        private const string DesignerEndpointParameterName = "DesignerEndpoint";
        private const string HttpResponseTimeoutParameterName = "HttpResponseTimeout";
        private const string BufferSizeParameterName = "BufferSize";
        private const string AcceptUnsignedSslCertificateParameterName = "AcceptUnsignedSslCertificate";
        private readonly IExpressionsEngineVersionService versionService;

        public ApplicationSettings(IExpressionsEngineVersionService versionService)
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
            get { return new TimeSpan(0, 0, SharedPreferences.GetInt(HttpResponseTimeoutParameterName,
                                            Application.Context.Resources.GetInteger(Resource.Integer.HttpResponseTimeout))); }
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

        public string ApplicationVersion
        {
            get
            {
                return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
            }
        }

        public string EngineVersion
        {
            get { return versionService.GetExpressionsEngineSupportedVersion().ToString(); }
        }

        public string OSVersion
        {
            get { return Build.VERSION.Release; }
        }

        public string ApplicationName
        {
            get { return SharedPreferences.GetString(ApplicationNameParameterName, Application.Context.Resources.GetString(Resource.String.ApplicationName)); }
        }
    }
}
