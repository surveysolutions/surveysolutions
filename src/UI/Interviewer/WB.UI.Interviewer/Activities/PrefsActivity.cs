using System.Globalization;
using Android.App;
using Android.OS;
using Android.Preferences;
using MvvmCross;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Interviewer.SharedPreferences;

namespace WB.UI.Interviewer.Activities
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
                var interviewerSettings = Mvx.Resolve<IInterviewerSettings>();

                this.SetPreferenceTitleAndSummary("interview_settings_category", InterviewerUIResources.Prefs_InterviewSettings, string.Empty);
                this.SetPreferenceTitleAndSummary("about_category", InterviewerUIResources.Prefs_AboutApplication, string.Empty);
                this.SetPreferenceTitleAndSummary("connection_settings_category", InterviewerUIResources.Prefs_ConnectionSettings, string.Empty);

                this.SetPreferenceTitleAndSummary("version", InterviewerUIResources.Prefs_ApplicationVersionTitle, interviewerSettings.GetApplicationVersionName());
                this.SetPreferenceTitleAndSummary("deviceid", InterviewerUIResources.Prefs_DeviceIdTitle, interviewerSettings.GetDeviceId());

                this.FindPreference(SettingsNames.GpsDesiredAccuracy).PreferenceChange += (sender, e) =>
                {
                    if (double.TryParse(e.NewValue.ToString(), out var newValue))
                    {
                        interviewerSettings.SetGpsDesiredAccuracy(newValue);
                    }

                    this.UpdateSettings();
                };

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
                this.FindPreference(SettingsNames.GpsReceiveTimeoutSec).PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetGpsResponseTimeout(ParseIntegerSettingsValue(e.NewValue, interviewerSettings.GpsReceiveTimeoutSec));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.VibrateOnError).PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetVibrateOnError(ParseBooleanSettingsValue(e.NewValue, interviewerSettings.VibrateOnError));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.ShowLocationOnMap).PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetShowLocationOnMap(ParseBooleanSettingsValue(e.NewValue, interviewerSettings.ShowLocationOnMap));
                    this.UpdateSettings();
                };

                var allowSyncWithHq = this.FindPreference(SettingsNames.AllowSyncWithHq);
                
                allowSyncWithHq.PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetAllowSyncWithHq(ParseBooleanSettingsValue(e.NewValue, interviewerSettings.AllowSyncWithHq));
                    this.UpdateSettings();
                };

                if (interviewerSettings.IsOfflineSynchronizationDone)
                {
                    allowSyncWithHq.Enabled = false;
                }

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

                this.SetPreferenceTitleAndSummary(SettingsNames.GpsDesiredAccuracy,
                    UIResources.Prefs_GpsDesiredAccuracyTitle,
                    UIResources.Prefs_GpsDesiredAccuracySubTitle,
                    interviewerSettings.GpsDesiredAccuracy.ToString());
                this.SetPreferenceTitleAndSummary(SettingsNames.EventChunkSize, InterviewerUIResources.Prefs_EventChunkSizeTitle,
                    InterviewerUIResources.Prefs_EventChunkSizeSummary, interviewerSettings.EventChunkSize.ToString());

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.VibrateOnError, UIResources.Prefs_VibrateOnErrorTitle,
                    UIResources.Prefs_VibrateOnErrorSummary, interviewerSettings.VibrateOnError);

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.ShowLocationOnMap, 
                    UIResources.Prefs_ShowLocationOnMap,
                    UIResources.Prefs_ShowLocationOnMapSummary,
                    interviewerSettings.ShowLocationOnMap);

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.AllowSyncWithHq, 
                    InterviewerUIResources.Prefs_AllowSyncWithHq,
                    interviewerSettings.IsOfflineSynchronizationDone 
                        ? InterviewerUIResources.Prefs_AllowSyncWithHq_Summary_Disabled
                        : InterviewerUIResources.Prefs_AllowSyncWithHq_Summary,
                    interviewerSettings.AllowSyncWithHq);
            }

            private static bool ParseBooleanSettingsValue(object settingsValue, bool defaultValue)
            {
                if (bool.TryParse(settingsValue.ToString(), out var intValue))
                    return intValue;

                return defaultValue;
            }

            private static int ParseIntegerSettingsValue(object settingsValue, int defaultValue)
            {
                if (int.TryParse(settingsValue.ToString(), out var intValue) && intValue > 0)
                    return intValue;

                return defaultValue;
            }

            private void SetPreferenceTitleAndSummary(string preferenceKey, string title, string summary, string defaultValue = "")
            {
                var preference = this.FindPreference(preferenceKey);

                preference.Title = title;
                preference.Summary = summary;

                if (preference is EditTextPreference editPreference)
                {
                    editPreference.Text = defaultValue;
                }
            }

            private void SetBooleanPreferenceTitleAndSummary(string preferenceKey, string title, string summary, bool defaultValue)
            {
                var preference = this.FindPreference(preferenceKey);

                preference.Title = title;
                preference.Summary = summary;
                if (preference is CheckBoxPreference checkBoxPreference)
                {
                    checkBoxPreference.Checked = defaultValue;
                }
            }
        }
    }
}
