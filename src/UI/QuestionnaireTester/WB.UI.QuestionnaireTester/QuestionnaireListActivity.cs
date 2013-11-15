using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
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

            if (this.FinishIfNotLoggedIn())
                return;

            this.listView = new ListView(this);
            this.listView.Adapter = new QuestionnaireListAdapter(this);
            this.listView.ChoiceMode = ChoiceMode.Single;
            this.listView.ItemClick += listView_ItemClick;
            this.AddContentView(listView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
        }

        private void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var intent = new Intent(this, typeof (LoadingActivity));
            intent.PutExtra("publicKey", e.View.GetTag(Resource.Id.QuestionnaireId).ToString());
            this.StartActivity(intent);
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();
        }
    }
}