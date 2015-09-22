using Android.App;
using Android.OS;
using Android.Preferences;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.UI.Interviewer.SharedPreferences;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "Preferences activity", NoHistory = false, Theme = "@style/GrayAppTheme")]
    public class PrefsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.AddPreferencesFromResource(Resource.Xml.preferences);

            this.SetupReadOnlyPreferences();
        }

        private void SetupReadOnlyPreferences()
        {
            var interviewerSettings = Mvx.Resolve<IInterviewerSettings>();

            this.SetPreferenceTitleAndSummary("interview_settings_category", InterviewerUIResources.Prefs_InterviewSettings, string.Empty);
            this.SetPreferenceTitleAndSummary("about_category", InterviewerUIResources.Prefs_AboutApplication, string.Empty);
            this.SetPreferenceTitleAndSummary("connection_settings_category", InterviewerUIResources.Prefs_ConnectionSettings, string.Empty);

            this.SetPreferenceTitleAndSummary("version", InterviewerUIResources.Prefs_ApplicationVersionTitle, interviewerSettings.GetApplicationVersionName());
            this.SetPreferenceTitleAndSummary("deviceid", InterviewerUIResources.Prefs_DeviceIdTitle, interviewerSettings.GetDeviceId());

            this.SetPreferenceTitleAndSummary(SettingsNames.Endpoint, InterviewerUIResources.Prefs_EndpointTitle, InterviewerUIResources.Prefs_EndpointSummary);
            this.SetPreferenceTitleAndSummary(SettingsNames.HttpResponseTimeout, InterviewerUIResources.Prefs_HttpResponseTimeoutTitle, InterviewerUIResources.Prefs_HttpResponseTimeoutSummary);
            this.SetPreferenceTitleAndSummary(SettingsNames.BufferSize, InterviewerUIResources.Prefs_BufferSizeTitle, InterviewerUIResources.Prefs_BufferSizeSummary);
            this.SetPreferenceTitleAndSummary(SettingsNames.GpsReceiveTimeoutSec, InterviewerUIResources.Prefs_GpsReceiveTimeoutSecTitle, InterviewerUIResources.Prefs_GpsReceiveTimeoutSecSummary);
            
        }

        private void SetPreferenceTitleAndSummary(string preferenceKey, string title, string summary)
        {
            var preference = this.FindPreference(preferenceKey);

            preference.Title = title;
            preference.Summary = summary;
        }
    }
}