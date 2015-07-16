using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "Preferences activity", NoHistory = false)]
    public class PrefsActivity : PreferenceActivity
    {
        private static int tapTimes = 0;
        private Preference devSettingsCategory;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.AddPreferencesFromResource(Resource.Xml.preferences);

            this.devSettingsCategory = this.FindPreference("dev_settings_category");
            this.PreferenceScreen.RemovePreference(this.devSettingsCategory);

            this.SetupVersionPreference();
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
            string format = this.Resources.GetString(Resource.String.Prefs_VersionSummaryFormat);
            var summary = string.Format(format, pInfo.VersionName, System.Environment.NewLine);
            versionPreference.Summary = summary;
        }
    }
}