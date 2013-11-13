using Android.Content.PM;
using WB.UI.QuestionnaireTester.Adapters;
using Android.App;
using Android.OS;

namespace WB.UI.QuestionnaireTester
{
    [Activity(Label = "CAPI",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class QuestionnaireListActivity : ListActivity 
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.ListAdapter = new QuestionnaireListAdapter(this);
        }
    }
}