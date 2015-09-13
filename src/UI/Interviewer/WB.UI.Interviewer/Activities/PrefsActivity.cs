using System;
using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "Preferences activity", NoHistory = false, Theme = "@style/GrayAppTheme")]
    public class PrefsActivity : PreferenceActivity
    {
        private static int tapTimes = 0;
        private Preference devSettingsCategory;

        private const string EndpointKey = "Endpoint";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.AddPreferencesFromResource(Resource.Xml.preferences);

            this.devSettingsCategory = this.FindPreference("dev_settings_category");

            Preference endpointPreference = this.FindPreference(EndpointKey);
            endpointPreference.PreferenceChange += this.DevSettingsCategoryOnPreferenceChange;

            this.SetupReadOnlyPreferences();

            this.PreferenceScreen.RemovePreference(this.devSettingsCategory);
        }

        void DevSettingsCategoryOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs preferenceChangeEventArgs)
        {
            string uri = preferenceChangeEventArgs.NewValue.ToString();
            string trimmedUri = (uri ?? string.Empty).Trim().Replace(" ", string.Empty);
            if (uri != trimmedUri)
            {
                preferenceChangeEventArgs.Handled = false;
            }
        }

        private void SetupReadOnlyPreferences()
        {
            Preference versionPreference = this.FindPreference("version");
            versionPreference.PreferenceClick += (sender, args) =>
            {
                Interlocked.Increment(ref tapTimes);
                if (tapTimes > 7)
                {
                    this.PreferenceScreen.AddPreference(this.devSettingsCategory);
                    tapTimes = 0;
                }
            };

            var interviewerSettings = Mvx.Resolve<IInterviewerSettings>();


            this.SetPreferenceTitleAndSummary("common_category", InterviewerUIResources.Prefs_CommonSettings, string.Empty);
            this.SetPreferenceTitleAndSummary("about_category", InterviewerUIResources.Prefs_AboutApplication, string.Empty);
            this.SetPreferenceTitleAndSummary("dev_settings_category", InterviewerUIResources.Prefs_ConnectionSettings, string.Empty);
            this.SetPreferenceTitleAndSummary("version", InterviewerUIResources.Prefs_ApplicationVersionTitle, interviewerSettings.GetApplicationVersionName());
            this.SetPreferenceTitleAndSummary("deviceid", InterviewerUIResources.Prefs_DeviceIdTitle, interviewerSettings.GetDeviceId());
            this.SetPreferenceTitleAndSummary("installationid", InterviewerUIResources.Prefs_InstallationIdTitle, interviewerSettings.GetInstallationId().ToString());
            this.SetPreferenceTitleAndSummary("clientregistrationid",
                InterviewerUIResources.Prefs_ClientRegistrationIdTitle,
                interviewerSettings.GetClientRegistrationId().HasValue
                    ? interviewerSettings.GetClientRegistrationId().ToString()
                    : " ");

            this.SetPreferenceTitleAndSummary("Endpoint", InterviewerUIResources.Prefs_EndpointTitle, InterviewerUIResources.Prefs_EndpointSummary);
            this.SetPreferenceTitleAndSummary("HttpResponseTimeout", InterviewerUIResources.Prefs_HttpResponseTimeoutTitle, InterviewerUIResources.Prefs_HttpResponseTimeoutSummary);
            this.SetPreferenceTitleAndSummary("BufferSize", InterviewerUIResources.Prefs_BufferSizeTitle, InterviewerUIResources.Prefs_BufferSizeSummary);
            this.SetPreferenceTitleAndSummary("GpsReceiveTimeoutSec", InterviewerUIResources.Prefs_GpsReceiveTimeoutSecTitle, InterviewerUIResources.Prefs_GpsReceiveTimeoutSecSummary);
            
        }

        private void SetPreferenceTitleAndSummary(string preferenceKey, string title, string summary)
        {
            var preference = this.FindPreference(preferenceKey);

            preference.Title = title;
            preference.Summary = summary;
        }
    }
}