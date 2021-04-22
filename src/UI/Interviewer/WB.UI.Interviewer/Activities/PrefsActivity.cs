using System.Globalization;
using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using MvvmCross;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.SharedPreferences;
using WB.UI.Shared.Enumerator.Settings;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "Preferences activity", 
        NoHistory = false, 
        Theme = "@style/GrayAppTheme",
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
            public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
            {
                this.AddPreferencesFromResource(Resource.Xml.preferences);
                this.SetupPreferences();
            }

            private void SetupPreferences()
            {
                var interviewerSettings = Mvx.IoCProvider.Resolve<IInterviewerSettings>();
                var principal = Mvx.IoCProvider.Resolve<IPrincipal>();

                this.SetPreferenceTitleAndSummary("interview_settings_category", EnumeratorUIResources.Prefs_InterviewSettings, string.Empty);
                this.SetPreferenceTitleAndSummary("about_category", EnumeratorUIResources.Prefs_AboutApplication, string.Empty);
                this.SetPreferenceTitleAndSummary("connection_settings_category", EnumeratorUIResources.Prefs_ConnectionSettings, string.Empty);

                this.SetPreferenceTitleAndSummary("version", EnumeratorUIResources.Prefs_ApplicationVersionTitle, interviewerSettings.GetApplicationVersionName());
                this.SetPreferenceTitleAndSummary("deviceid", EnumeratorUIResources.Prefs_DeviceIdTitle, interviewerSettings.GetDeviceId());

                this.FindPreference(SettingsNames.GpsDesiredAccuracy)
                    .SetEditTextDecimalMode()
                    .PreferenceChange += (sender, e) =>
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
                this.FindPreference(SettingsNames.EventChunkSize)
                    .SetEditTextNumericMode()
                    .PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetEventChunkSize(ParseIntegerSettingsValue(e.NewValue, interviewerSettings.EventChunkSize));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.HttpResponseTimeout)
                    .SetEditTextNumericMode()
                    .PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetHttpResponseTimeout(ParseIntegerSettingsValue(e.NewValue, (int)interviewerSettings.Timeout.TotalSeconds));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.BufferSize)
                    .SetEditTextNumericMode()
                    .PreferenceChange += (sender, e) =>
                {
                    interviewerSettings.SetCommunicationBufferSize(ParseIntegerSettingsValue(e.NewValue, interviewerSettings.BufferSize));
                    this.UpdateSettings();
                };
                this.FindPreference(SettingsNames.GpsReceiveTimeoutSec)
                    .SetEditTextNumericMode()
                    .PreferenceChange += (sender, e) =>
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
                allowSyncWithHq.Enabled = principal.CurrentUserIdentity?.Workspace != null;
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
                var interviewerSettings = Mvx.IoCProvider.Resolve<IInterviewerSettings>();

                this.SetPreferenceTitleAndSummary(SettingsNames.Endpoint, EnumeratorUIResources.Prefs_EndpointTitle,
                    interviewerSettings.Endpoint, interviewerSettings.Endpoint);
                this.SetPreferenceTitleAndSummary(SettingsNames.HttpResponseTimeout,
                    EnumeratorUIResources.Prefs_HttpResponseTimeoutTitle,
                    EnumeratorUIResources.Prefs_HttpResponseTimeoutSummary, interviewerSettings.Timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                this.SetPreferenceTitleAndSummary(SettingsNames.BufferSize, EnumeratorUIResources.Prefs_BufferSizeTitle,
                    EnumeratorUIResources.Prefs_BufferSizeSummary, interviewerSettings.BufferSize.ToString());

                this.SetPreferenceTitleAndSummary(SettingsNames.GpsReceiveTimeoutSec,
                    EnumeratorUIResources.Prefs_GpsReceiveTimeoutSecTitle,
                    EnumeratorUIResources.Prefs_GpsReceiveTimeoutSecSummary,
                    interviewerSettings.GpsReceiveTimeoutSec.ToString());

                this.SetPreferenceTitleAndSummary(SettingsNames.GpsDesiredAccuracy,
                    UIResources.Prefs_GpsDesiredAccuracyTitle,
                    UIResources.Prefs_GpsDesiredAccuracySubTitle,
                    interviewerSettings.GpsDesiredAccuracy.ToString());
                this.SetPreferenceTitleAndSummary(SettingsNames.EventChunkSize, EnumeratorUIResources.Prefs_EventChunkSizeTitle,
                    EnumeratorUIResources.Prefs_EventChunkSizeSummary, interviewerSettings.EventChunkSize.ToString());

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.VibrateOnError, UIResources.Prefs_VibrateOnErrorTitle,
                    UIResources.Prefs_VibrateOnErrorSummary, interviewerSettings.VibrateOnError);

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.ShowLocationOnMap, 
                    UIResources.Prefs_ShowLocationOnMap,
                    UIResources.Prefs_ShowLocationOnMapSummary,
                    interviewerSettings.ShowLocationOnMap);

                this.SetBooleanPreferenceTitleAndSummary(SettingsNames.AllowSyncWithHq,
                    EnumeratorUIResources.Prefs_AllowSyncWithHq,
                    interviewerSettings.IsOfflineSynchronizationDone 
                        ? EnumeratorUIResources.Prefs_AllowSyncWithHq_Summary_Disabled
                        : EnumeratorUIResources.Prefs_AllowSyncWithHq_Summary,
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
