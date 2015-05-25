using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Label = "Preferences activity")]
    public class PrefsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            AddPreferencesFromResource(Resource.Xml.preferences);
        }
    }
}