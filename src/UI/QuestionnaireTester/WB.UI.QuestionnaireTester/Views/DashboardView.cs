using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(Label = "", Theme = "@style/Theme.Gray.Light", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateHidden)]
    public class DashboardView : BaseActivityView<DashboardViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.dashboard; }
        }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.dashboard, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.dashboard_refresh:
                    this.ViewModel.RefreshQuestionnairesCommand.Execute();
                    break;
                case Resource.Id.dashboard_search:
                    this.ViewModel.SearchQuestionnairesCommand.Execute();
                    break;
                case Resource.Id.dashboard_settings:
                    this.ViewModel.ShowSettingsCommand.Execute();
                    break;
                case Resource.Id.dashboard_about:
                    this.ViewModel.ShowAboutCommand.Execute();
                    break;
                case Resource.Id.dashboard_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }
    }
}