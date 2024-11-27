using System;
using System.Globalization;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using MvvmCross;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Settings;
using WB.UI.Supervisor.SharedPreferences;

namespace WB.UI.Supervisor.Activities
{
    [Activity(Label = "Preferences activity", 
        NoHistory = false, 
        Theme = "@style/GrayAppTheme",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
    public class PrefsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SupportFragmentManager
                .BeginTransaction()
                .Replace(Android.Resource.Id.Content, new PrefsFragment())
                .Commit();
        }

        public class PrefsFragment : PreferenceFragmentCompat
        {
            private static int tapTimes;
            public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
            {
                this.AddPreferencesFromResource(Resource.Xml.preferences);
                this.SetupPreferences();
            }

            private void SetupPreferences()
            {
                var settings = Mvx.IoCProvider.Resolve<ISupervisorSettings>();

                this.SetPreferenceTitleAndSummary("interview_settings_category",
                    EnumeratorUIResources.Prefs_InterviewSettings, string.Empty);
                this.SetPreferenceTitleAndSummary("about_category", EnumeratorUIResources.Prefs_AboutApplication,
                    string.Empty);
                this.SetPreferenceTitleAndSummary("connection_settings_category",
                    EnumeratorUIResources.Prefs_ConnectionSettings, string.Empty);

                this.SetPreferenceTitleAndSummary("version", EnumeratorUIResources.Prefs_ApplicationVersionTitle,
                    settings.GetApplicationVersionName());
                this.SetPreferenceTitleAndSummary("deviceid", EnumeratorUIResources.Prefs_DeviceIdTitle,
                    settings.GetDeviceId());

                this.FindPreference(SettingsNames.Endpoint).PreferenceChange += (sender, e) =>
                {
                    settings.SetEndpoint(e.NewValue.ToString());
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.EventChunkSize)
                    .SetEditTextNumericMode()
                    .PreferenceChange += (sender, e) =>
                {
                    settings.SetEventChunkSize(ParseIntegerSettingsValue(e.NewValue, settings.EventChunkSize));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.HttpResponseTimeout)
                    .SetEditTextNumericMode()
                    .PreferenceChange += (sender, e) =>
                {
                    settings.SetHttpResponseTimeout(ParseIntegerSettingsValue(e.NewValue,
                        (int) settings.Timeout.TotalSeconds));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.BufferSize)
                    .SetEditTextNumericMode()
                    .PreferenceChange += (sender, e) =>
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
                this.SetupVersionPreference();
            }

            private void UpdateSettings()
            {
                var settings = Mvx.IoCProvider.Resolve<ISupervisorSettings>();

                this.SetPreferenceTitleAndSummary(SettingsNames.Endpoint, EnumeratorUIResources.Prefs_EndpointTitle,
                    settings.Endpoint, settings.Endpoint);
                this.SetPreferenceTitleAndSummary(SettingsNames.HttpResponseTimeout,
                    EnumeratorUIResources.Prefs_HttpResponseTimeoutTitle,
                    EnumeratorUIResources.Prefs_HttpResponseTimeoutSummary,
                    settings.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                this.SetPreferenceTitleAndSummary(SettingsNames.BufferSize,
                    EnumeratorUIResources.Prefs_BufferSizeTitle,
                    EnumeratorUIResources.Prefs_BufferSizeSummary, settings.BufferSize.ToString());


                this.SetPreferenceTitleAndSummary(SettingsNames.EventChunkSize,
                    EnumeratorUIResources.Prefs_EventChunkSizeTitle,
                    EnumeratorUIResources.Prefs_EventChunkSizeSummary, settings.EventChunkSize.ToString());

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
                if (int.TryParse(settingsValue.ToString(), out var intValue) && intValue > 0)
                    return intValue;

                return defaultValue;
            }

            private void SetPreferenceTitleAndSummary(string preferenceKey, string title, string summary,
                string defaultValue = "")
            {
                var preference = this.FindPreference(preferenceKey);

                preference.Title = title;
                preference.Summary = summary;

                if (preference is EditTextPreference editPreference)
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
                if (preference is CheckBoxPreference checkBoxPreference)
                {
                    checkBoxPreference.Checked = defaultValue;
                }
            }
            
            private void SetupVersionPreference()
            {
                Preference versionPreference = this.FindPreference("version");
                versionPreference.PreferenceClick += (sender, args) =>
                {
                    Interlocked.Increment(ref tapTimes);
                    if (tapTimes > 7)
                    {
                        throw new InvalidOperationException("Test Exception" + " Should not have clicked this!");
                    }
                }
            }
        }
    }
}
