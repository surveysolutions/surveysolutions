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
    [Activity(Label = "Preferences activity", 
        NoHistory = false, 
        Theme = "@style/GrayAppTheme",
        Exported = false)]
    public class PrefsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            FragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, new PrefsFragment()).Commit();
        }

        protected override bool IsValidFragment(string fragmentName)
        {
            return typeof(PrefsFragment).Name.Equals(fragmentName);
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
                var settings = Mvx.Resolve<ISupervisorSettings>();

                this.SetPreferenceTitleAndSummary("interview_settings_category",
                    InterviewerUIResources.Prefs_InterviewSettings, string.Empty);
                this.SetPreferenceTitleAndSummary("about_category", InterviewerUIResources.Prefs_AboutApplication,
                    string.Empty);
                this.SetPreferenceTitleAndSummary("connection_settings_category",
                    InterviewerUIResources.Prefs_ConnectionSettings, string.Empty);

                this.SetPreferenceTitleAndSummary("version", InterviewerUIResources.Prefs_ApplicationVersionTitle,
                    settings.GetApplicationVersionName());
                this.SetPreferenceTitleAndSummary("deviceid", InterviewerUIResources.Prefs_DeviceIdTitle,
                    settings.GetDeviceId());

                this.FindPreference(SettingsNames.Endpoint).PreferenceChange += (sender, e) =>
                {
                    settings.SetEndpoint(e.NewValue.ToString());
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.EventChunkSize).PreferenceChange += (sender, e) =>
                {
                    settings.SetEventChunkSize(ParseIntegerSettingsValue(e.NewValue, settings.EventChunkSize));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.HttpResponseTimeout).PreferenceChange += (sender, e) =>
                {
                    settings.SetHttpResponseTimeout(ParseIntegerSettingsValue(e.NewValue,
                        (int) settings.Timeout.TotalSeconds));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.BufferSize).PreferenceChange += (sender, e) =>
                {
                    settings.SetCommunicationBufferSize(ParseIntegerSettingsValue(e.NewValue, settings.BufferSize));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.ShowLocationOnMap).PreferenceChange += (sender, e) =>
                {
                    settings.SetShowLocationOnMap(ParseBooleanSettingsValue(e.NewValue, settings.ShowLocationOnMap));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.DownloadUpdatesForInterviewerApp).PreferenceChange += (sender, e) =>
                {
                    settings.SetDownloadUpdatesForInterviewerApp(ParseBooleanSettingsValue(e.NewValue,
                        settings.DownloadUpdatesForInterviewerApp));
                    this.UpdateSettings();
                };

                this.UpdateSettings();
            }

            private void UpdateSettings()
            {
                var settings = Mvx.Resolve<ISupervisorSettings>();

                this.SetPreferenceTitleAndSummary(SettingsNames.Endpoint, InterviewerUIResources.Prefs_EndpointTitle,
                    settings.Endpoint, settings.Endpoint);
                this.SetPreferenceTitleAndSummary(SettingsNames.HttpResponseTimeout,
                    InterviewerUIResources.Prefs_HttpResponseTimeoutTitle,
                    InterviewerUIResources.Prefs_HttpResponseTimeoutSummary,
                    settings.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                this.SetPreferenceTitleAndSummary(SettingsNames.BufferSize,
                    InterviewerUIResources.Prefs_BufferSizeTitle,
                    InterviewerUIResources.Prefs_BufferSizeSummary, settings.BufferSize.ToString());


                this.SetPreferenceTitleAndSummary(SettingsNames.EventChunkSize,
                    InterviewerUIResources.Prefs_EventChunkSizeTitle,
                    InterviewerUIResources.Prefs_EventChunkSizeSummary, settings.EventChunkSize.ToString());

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.ShowLocationOnMap,
                    UIResources.Prefs_ShowLocationOnMap,
                    UIResources.Prefs_ShowLocationOnMapSummary,
                    settings.ShowLocationOnMap);

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.DownloadUpdatesForInterviewerApp,
                    UIResources.Prefs_DownloadUpdatesForInterviewerApp,
                    UIResources.Prefs_DownloadUpdatesForInterviewerAppSummary,
                    settings.DownloadUpdatesForInterviewerApp);
            }

            private static bool ParseBooleanSettingsValue(object settingsValue, bool defaultValue)
            {
                bool intValue;
                if (bool.TryParse(settingsValue.ToString(), out intValue))
                    return intValue;

                return defaultValue;
            }

            private static int ParseIntegerSettingsValue(object settingsValue, int defaultValue)
            {
                var intValue = -1;
                if (int.TryParse(settingsValue.ToString(), out intValue) && intValue > 0)
                    return intValue;

                return defaultValue;
            }

            private void SetPreferenceTitleAndSummary(string preferenceKey, string title, string summary,
                string defaultValue = "")
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

            private void SetBooleanPreferenceTitleAndSummary(string preferenceKey, string title, string summary,
                bool defaultValue)
            {
                var preference = this.FindPreference(preferenceKey);

                preference.Title = title;
                preference.Summary = summary;
                var checkBoxPreference = preference as CheckBoxPreference;
                if (checkBoxPreference != null)
                {
                    checkBoxPreference.Checked = defaultValue;
                }
            }
        }
    }
}
