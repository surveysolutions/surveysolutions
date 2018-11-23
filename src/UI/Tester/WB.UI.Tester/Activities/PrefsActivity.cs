using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using MvvmCross;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Tester.Infrastructure.Internals.Settings;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "Preferences activity", 
        NoHistory = false, 
        Name = "org.worldbank.solutions.Vtester.PrefsActivity",
        Exported = false)]
    public class PrefsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            FragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, new PrefsFragment()).Commit();
        }

        protected override bool IsValidFragment(string fragmentName)
        {
            return typeof(PrefsFragment).Name.Equals(fragmentName);
        }

        public class PrefsFragment : PreferenceFragment
        {
            private static int tapTimes;
            private Preference devSettingsCategory;

            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(Resource.Xml.preferences);

                var settings = Mvx.Resolve<TesterSettings>();

                this.devSettingsCategory = this.FindPreference("dev_settings_category");

                Preference designerEndpointPreference = this.FindPreference(TesterSettings.DesignerEndpointParameterName);
                designerEndpointPreference.PreferenceChange += DevSettingsCategoryOnPreferenceChange;

                this.SetPreferenceTitleAndSummary("HttpResponseTimeout", TesterUIResources.Prefs_HttpResponseTimeoutTitle, TesterUIResources.Prefs_HttpResponseTimeoutSummary);
                this.SetPreferenceTitleAndSummary("GpsReceiveTimeoutSec", TesterUIResources.Prefs_GpsReceiveTimeoutSecTitle, TesterUIResources.Prefs_GpsReceiveTimeoutSecSummary);
                this.SetPreferenceTitleAndSummary("version", TesterUIResources.Prefs_VersionTitle, string.Empty);
                this.SetPreferenceTitleAndSummary("dev_settings_category", TesterUIResources.Prefs_ConnectionSettings, string.Empty);
                this.SetPreferenceTitleAndSummary(TesterSettings.DesignerEndpointParameterName, TesterUIResources.Prefs_DesignerEndPointTitle, settings.Endpoint);
                this.SetPreferenceTitleAndSummary("AcceptUnsignedSslCertificate", TesterUIResources.Prefs_AcceptUnsignedTitle, TesterUIResources.Prefs_AcceptUnsignedSummary);

                this.SetBooleanPreferenceTitleAndSummary(TesterSettings.ShowVariablesParamterName, TesterUIResources.Prefs_ShowVariables,
                    settings.ShowVariables ? TesterUIResources.Prefs_ShowVariablesSummary_Checked : TesterUIResources.Prefs_ShowVariablesSummary_UnChecked, settings.ShowVariables);
                this.FindPreference(TesterSettings.ShowVariablesParamterName).PreferenceChange += (sender, args) =>
                {
                    var checkBoxPreference = args.Preference as CheckBoxPreference;
                    var summary = !checkBoxPreference.Checked ? // this wonderful api returns value BEFORE change
                        TesterUIResources.Prefs_ShowVariablesSummary_Checked
                        : TesterUIResources.Prefs_ShowVariablesSummary_UnChecked;
                    checkBoxPreference.Summary = summary;
                };

                this.SetBooleanPreferenceTitleAndSummary(TesterSettings.ShowAnsweringTimeName, TesterUIResources.Prefs_ShowAnswerTime, TesterUIResources.Prefs_ShowAnswerTimeSummary, settings.ShowAnswerTime);

                this.SetBooleanPreferenceTitleAndSummary(TesterSettings.ShowLocationOnMapParamName, 
                    UIResources.Prefs_ShowLocationOnMap, 
                    UIResources.Prefs_ShowLocationOnMapSummary,
                    settings.ShowLocationOnMap);

                this.SetPreferenceTitleAndSummary("GpsDesiredAccuracy", UIResources.Prefs_GpsDesiredAccuracyTitle,
                    string.Format(UIResources.Prefs_GpsDesiredAccuracySubTitle, settings.GpsDesiredAccuracy));

                this.SetBooleanPreferenceTitleAndSummary(TesterSettings.VibrateOnErrorParameterName, UIResources.Prefs_VibrateOnErrorTitle, UIResources.Prefs_VibrateOnErrorSummary, settings.VibrateOnError);
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

            private void SetBooleanPreferenceTitleAndSummary(string preferenceKey, string title, string summary, bool value)
            {
                var preference = this.FindPreference(preferenceKey);

                preference.Title = title;
                preference.Summary = summary;
                var checkBoxPreference = preference as CheckBoxPreference;
                if (checkBoxPreference != null)
                {
                    checkBoxPreference.Checked = value;
                }
            }


            void DevSettingsCategoryOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs preferenceChangeEventArgs)
            {
                string uri = preferenceChangeEventArgs.NewValue.ToString();
                string trimmedUri = uri.Trim().Replace(" ", string.Empty);
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

                PackageInfo pInfo = this.Activity.PackageManager.GetPackageInfo(this.Activity.PackageName, 0);
                string format = TesterUIResources.Prefs_VersionSummaryFormat;
                var summary = string.Format(format, pInfo.VersionName, System.Environment.NewLine);
                versionPreference.Summary = summary;
            }
        }
    }
}
