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
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, LaunchMode = LaunchMode.SingleTop, Label = "questionnairelist")]
    public class QuestionnaireListActivity : Activity
    {
        protected ListView listView;
        protected QuestionnaireListAdapter adapter; 
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (this.FinishIfNotLoggedIn())
                return;
            this.Title = string.Format("List of {0}'s questionnaires", CapiTesterApplication.Membership.RemoteUser.UserName);
            
            this.listView = new ListView(this);
            this.listView.Adapter = adapter = new QuestionnaireListAdapter(this);
            this.listView.ChoiceMode = ChoiceMode.Single;
            this.listView.ItemClick += listView_ItemClick;
            this.AddContentView(listView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
        }

        void searchView_QueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            adapter.Query(e.NewText);
        }

        private void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var intent = new Intent(this, typeof (LoadingActivity));
            intent.PutExtra("publicKey", e.View.GetTag(Resource.Id.QuestionnaireId).ToString());
            this.StartActivity(intent);
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            svQuery.SetQuery(string.Empty, true);
            adapter.Update();
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateSearchebleActionBar();

            svQuery.QueryTextChange += searchView_QueryTextChange;
        }

        protected SearchView svQuery
        {
            get { return this.ActionBar.CustomView.FindViewById<SearchView>(Resource.Id.svQuery); }
        }
    }
}