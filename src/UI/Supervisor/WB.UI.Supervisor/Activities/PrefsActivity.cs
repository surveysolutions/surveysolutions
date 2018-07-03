using System.Globalization;
using Android.App;
using Android.OS;
using Android.Preferences;
using MvvmCross;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Supervisor.SharedPreferences;

namespace WB.UI.Supervisor.Activities
{
    [Activity(Label = "Preferences activity", NoHistory = false, Theme = "@style/GrayAppTheme")]
    public class PrefsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            FragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, new PrefsFragment()).Commit();
        }

        protected override bool IsValidFragment(string fragmentName)
        {
            return false;
        }

        public class PrefsFragment : PreferenceFragment
        {
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                this.AddPreferencesFromResource(Resource.Xml.preferences);
                this.SetupPreferences();
            }

            private void SetupPreferences()
            {
                var interviewerSettings = Mvx.Resolve<ISupervisorSettings>();

                this.SetPreferenceTitleAndSummary("about_category", InterviewerUIResources.Prefs_AboutApplication, string.Empty);
                this.SetPreferenceTitleAndSummary("connection_settings_category", InterviewerUIResources.Prefs_ConnectionSettings, string.Empty);

                this.SetPreferenceTitleAndSummary("version", InterviewerUIResources.Prefs_ApplicationVersionTitle, interviewerSettings.GetApplicationVersionName());
                this.SetPreferenceTitleAndSummary("deviceid", InterviewerUIResources.Prefs_DeviceIdTitle, interviewerSettings.GetDeviceId());

                this.FindPreference(SettingsNames.Endpoint).PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetEndpoint(e.NewValue.ToString());
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.EventChunkSize).PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetEventChunkSize(ParseIntegerSettingsValue(e.NewValue, interviewerSettings.EventChunkSize));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.HttpResponseTimeout).PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetHttpResponseTimeout(ParseIntegerSettingsValue(e.NewValue, (int)interviewerSettings.Timeout.TotalSeconds));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.BufferSize).PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetCommunicationBufferSize(ParseIntegerSettingsValue(e.NewValue, interviewerSettings.BufferSize));
                    this.UpdateSettings();
                };

                this.UpdateSettings();
            }

            private void UpdateSettings()
            {
                var interviewerSettings = Mvx.Resolve<ISupervisorSettings>();

                this.SetPreferenceTitleAndSummary(SettingsNames.Endpoint, InterviewerUIResources.Prefs_EndpointTitle,
                    interviewerSettings.Endpoint, interviewerSettings.Endpoint);
                this.SetPreferenceTitleAndSummary(SettingsNames.HttpResponseTimeout,
                    InterviewerUIResources.Prefs_HttpResponseTimeoutTitle,
                    InterviewerUIResources.Prefs_HttpResponseTimeoutSummary, interviewerSettings.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                this.SetPreferenceTitleAndSummary(SettingsNames.BufferSize, InterviewerUIResources.Prefs_BufferSizeTitle,
                    InterviewerUIResources.Prefs_BufferSizeSummary, interviewerSettings.BufferSize.ToString());

                
                this.SetPreferenceTitleAndSummary(SettingsNames.EventChunkSize, InterviewerUIResources.Prefs_EventChunkSizeTitle,
                    InterviewerUIResources.Prefs_EventChunkSizeSummary, interviewerSettings.EventChunkSize.ToString());
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
}
