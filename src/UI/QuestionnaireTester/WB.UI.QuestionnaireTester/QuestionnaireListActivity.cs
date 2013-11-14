using Android.Content.PM;
using WB.UI.QuestionnaireTester.Adapters;
using Android.App;
using Android.OS;
using WB.UI.QuestionnaireTester.Extensions;
using WB.UI.Shared.Android.Extensions;

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

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }
    }
}