using Android.Content.PM;
using Android.Views;
using Android.Widget;
using WB.UI.QuestionnaireTester.Adapters;
using Android.App;
using Android.OS;
using WB.UI.QuestionnaireTester.Extensions;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.QuestionnaireTester
{
    [Activity(Label = "CAPI",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class QuestionnaireListActivity : Activity
    {
        protected ListView listView;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.listView=new ListView(this);
            this.listView.Adapter = new QuestionnaireListAdapter(this);
            this.AddContentView(listView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }
    }
}