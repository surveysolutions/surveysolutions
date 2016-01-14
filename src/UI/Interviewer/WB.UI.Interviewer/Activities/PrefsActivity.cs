using System.Globalization;
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

            this.SetupPreferences();
        }

        private void SetupPreferences()
        {
            var interviewerSettings = Mvx.Resolve<IInterviewerSettings>();

            this.SetPreferenceTitleAndSummary("interview_settings_category", InterviewerUIResources.Prefs_InterviewSettings, string.Empty);
            this.SetPreferenceTitleAndSummary("about_category", InterviewerUIResources.Prefs_AboutApplication, string.Empty);
            this.SetPreferenceTitleAndSummary("connection_settings_category", InterviewerUIResources.Prefs_ConnectionSettings, string.Empty);

            this.SetPreferenceTitleAndSummary("version", InterviewerUIResources.Prefs_ApplicationVersionTitle, interviewerSettings.GetApplicationVersionName());
            this.SetPreferenceTitleAndSummary("deviceid", InterviewerUIResources.Prefs_DeviceIdTitle, interviewerSettings.GetDeviceId());

            
            this.FindPreference(SettingsNames.Endpoint).PreferenceChange += async (sender, e) =>
            {
                await interviewerSettings.SetEndpointAsync(e.NewValue.ToString());
                this.UpdateSettings();
            };
            this.FindPreference(SettingsNames.HttpResponseTimeout).PreferenceChange += async (sender, e) =>
            {
                await interviewerSettings.SetHttpResponseTimeoutAsync(ParseIntegerSettingsValue(e.NewValue, (int)interviewerSettings.Timeout.TotalSeconds));
                this.UpdateSettings();
            };
            this.FindPreference(SettingsNames.BufferSize).PreferenceChange += async (sender, e) =>
            {
                await interviewerSettings.SetCommunicationBufferSize(ParseIntegerSettingsValue(e.NewValue, interviewerSettings.BufferSize));
                this.UpdateSettings();
            };
            this.FindPreference(SettingsNames.GpsReceiveTimeoutSec).PreferenceChange += async (sender, e) =>
            {
                await interviewerSettings.SetGpsResponseTimeoutAsync(ParseIntegerSettingsValue(e.NewValue, interviewerSettings.GpsReceiveTimeoutSec));
                this.UpdateSettings();
            };

            this.UpdateSettings();
        }

        private void UpdateSettings()
        {
            var interviewerSettings = Mvx.Resolve<IInterviewerSettings>();

            this.SetPreferenceTitleAndSummary(SettingsNames.Endpoint, InterviewerUIResources.Prefs_EndpointTitle,
                interviewerSettings.Endpoint, interviewerSettings.Endpoint);
            this.SetPreferenceTitleAndSummary(SettingsNames.HttpResponseTimeout,
                InterviewerUIResources.Prefs_HttpResponseTimeoutTitle,
                InterviewerUIResources.Prefs_HttpResponseTimeoutSummary, interviewerSettings.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            this.SetPreferenceTitleAndSummary(SettingsNames.BufferSize, InterviewerUIResources.Prefs_BufferSizeTitle,
                InterviewerUIResources.Prefs_BufferSizeSummary, interviewerSettings.BufferSize.ToString());
            this.SetPreferenceTitleAndSummary(SettingsNames.GpsReceiveTimeoutSec,
                InterviewerUIResources.Prefs_GpsReceiveTimeoutSecTitle,
                InterviewerUIResources.Prefs_GpsReceiveTimeoutSecSummary,
                interviewerSettings.GpsReceiveTimeoutSec.ToString());
        }

        private static int ParseIntegerSettingsValue(object settingsValue, int defaultValue)
        {
            var intValue = -1;
            if (int.TryParse(settingsValue.ToString(), out intValue) && intValue > 0)
                return intValue;

            return defaultValue;
        }

        private void SetPreferenceTitleAndSummary(string preferenceKey, string title, string summary, string defaultValue = "")
        {
            var preference = this.FindPreference(preferenceKey);

            preference.Title = title;
            preference.Summary = summary;

            var editPreference = preference as EditTextPreference;
            if (editPreference != null)
            {
                editPreference.Text = defaultValue;
            }
        }
    }
}