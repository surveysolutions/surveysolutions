using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.Views.Fragments;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(NoHistory = true, WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme", LaunchMode = LaunchMode.SingleTask)]
    public class LoginView : BaseActivityView<LoginViewModel>
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.login, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.login_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    StartActivity(intent);
                    break;
                default: break;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override int ViewResourceId
        {
            get { return Resource.Layout.login; }
        }
    }
}