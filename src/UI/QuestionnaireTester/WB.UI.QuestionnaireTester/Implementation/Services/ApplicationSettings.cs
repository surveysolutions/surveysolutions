using System;
using Android.App;
using Android.Content;
using Android.OS;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class ApplicationSettings : IApplicationSettings
    {
        private const string ApplicationNameParameterName = "ApplicationName";
        private const string DesignerEndpointParameterName = "DesignerEndpoint";
        private const string HttpResponseTimeoutParameterName = "HttpResponseTimeout";
        private const string BufferSizeParameterName = "BufferSize";
        
        private static ISharedPreferences sharedPreferences
        {
            get
            {
                return Application.Context.GetSharedPreferences(Application.Context.Resources.GetString(Resource.String.ApplicationName),
                        FileCreationMode.Private);
            }
        }

        private static void SavePreferenceValue(string preferenceParameterName, string value)
        {
            ISharedPreferencesEditor prefEditor = sharedPreferences.Edit();
            prefEditor.PutString(preferenceParameterName, value);
            prefEditor.Commit();
        }

        private static string GetPreferenceString(string preferenceParameterName, int preferenceResourceId)
        {
            return sharedPreferences.GetString(preferenceParameterName, Application.Context.Resources.GetString(preferenceResourceId));
        }

        public string DesignerEndpoint
        {
            get { return GetPreferenceString(DesignerEndpointParameterName, Resource.String.DesignerEndpoint); }
            set { SavePreferenceValue(DesignerEndpointParameterName, value); }
        }

        public TimeSpan HttpResponseTimeout
        {
            get { return new TimeSpan(0, 0, int.Parse(GetPreferenceString(HttpResponseTimeoutParameterName, Resource.String.HttpResponseTimeout))); }
            set { SavePreferenceValue(HttpResponseTimeoutParameterName, value.Seconds.ToString()); }
        }

        public int BufferSize
        {
            get { return int.Parse(GetPreferenceString(BufferSizeParameterName, Resource.String.BufferSize)); }
            set { SavePreferenceValue(BufferSizeParameterName, value.ToString()); }
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
            get { return QuestionnaireVersionProvider.GetCurrentEngineVersion().ToString(); }
        }

        public string OSVersion
        {
            get { return Build.VERSION.Release; }
        }

        public string ApplicationName
        {
            get { return GetPreferenceString(ApplicationNameParameterName, Resource.String.ApplicationName); }
        }
    }
}
