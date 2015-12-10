using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using WB.Core.BoundedContexts.Tester.Properties;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "Preferences activity", NoHistory = false)]
    public class PrefsActivity : PreferenceActivity
    {
        private static int tapTimes = 0;
        private Preference devSettingsCategory;

        private const string designerEndpointKey = "DesignerEndpointV11";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.AddPreferencesFromResource(Resource.Xml.preferences);

            this.devSettingsCategory = this.FindPreference("dev_settings_category");

            Preference designerEndpointPreference = this.FindPreference(designerEndpointKey);
            designerEndpointPreference.PreferenceChange += DevSettingsCategoryOnPreferenceChange;

            this.SetPreferenceTitleAndSummary("HttpResponseTimeout", TesterUIResources.Prefs_HttpResponseTimeoutTitle, TesterUIResources.Prefs_HttpResponseTimeoutSummary);
            this.SetPreferenceTitleAndSummary("GpsReceiveTimeoutSec", TesterUIResources.Prefs_GpsReceiveTimeoutSecTitle, TesterUIResources.Prefs_GpsReceiveTimeoutSecSummary);
            this.SetPreferenceTitleAndSummary("version", TesterUIResources.Prefs_VersionTitle, string.Empty);
            this.SetPreferenceTitleAndSummary("dev_settings_category", TesterUIResources.Prefs_ConnectionSettings, string.Empty);
            this.SetPreferenceTitleAndSummary("DesignerEndpointV11", TesterUIResources.Prefs_DesignerEndPointTitle, TesterUIResources.Prefs_DesignerEndPointSummary);
            this.SetPreferenceTitleAndSummary("AcceptUnsignedSslCertificate", TesterUIResources.Prefs_AcceptUnsignedTitle, TesterUIResources.Prefs_AcceptUnsignedSummary);


            this.PreferenceScreen.RemovePreference(this.devSettingsCategory);
          
            this.SetupVersionPreference();
        }

        private void SetPreferenceTitleAndSummary(string preferenceKey, string title, string summary)
        {
            var preference = this.FindPreference(preferenceKey);

            if (preference != null)
            {
                preference.Title = title;
                preference.Summary = summary;
            }
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

        private void SetupVersionPreference()
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

            PackageInfo pInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
            string format = TesterUIResources.Prefs_VersionSummaryFormat;
            var summary = string.Format(format, pInfo.VersionName, System.Environment.NewLine);
            versionPreference.Summary = summary;
        }
    }
}