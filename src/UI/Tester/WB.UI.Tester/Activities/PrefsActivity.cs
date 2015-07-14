using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "Preferences activity", NoHistory = false)]
    public class PrefsActivity : PreferenceActivity
    {
        private readonly IExpressionsEngineVersionService engineVersionService;
        private static int tapTimes = 0;
        private Preference devSettingsCategory;
        private const string DevSettingName = "pref_dev";

        public PrefsActivity()
            : this(ServiceLocator.Current.GetInstance<IExpressionsEngineVersionService>()) // needed for android
        {
        }

        public PrefsActivity(IExpressionsEngineVersionService engineVersionService)
        {
            this.engineVersionService = engineVersionService;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.AddPreferencesFromResource(Resource.Xml.preferences);

            var settings = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var devSettingsEnabled = settings.GetBoolean(DevSettingName, false);

            if (!devSettingsEnabled)
            {
                this.devSettingsCategory = this.FindPreference("dev_settings_category");
                this.PreferenceScreen.RemovePreference(this.devSettingsCategory);
            }

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
                    var defaultSharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                    var sharedPreferencesEditor = defaultSharedPreferences.Edit();
                    sharedPreferencesEditor.PutBoolean(DevSettingName, true);
                    sharedPreferencesEditor.Commit();

                    this.PreferenceScreen.AddPreference(this.devSettingsCategory);

                    tapTimes = 0;
                }
            };

            PackageInfo pInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
            string format = this.Resources.GetString(Resource.String.Prefs_VersionSummaryFormat);
            var summary = string.Format(format, pInfo.VersionName, System.Environment.NewLine, this.engineVersionService.GetExpressionsEngineSupportedVersion());
            versionPreference.Summary = summary;
        }
    }
}