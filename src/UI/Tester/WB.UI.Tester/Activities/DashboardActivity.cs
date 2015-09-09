using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar = Android.Support.V7.Widget.Toolbar;
//using SearchView = Android.Support.V7.Widget.SearchView;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden, 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>
    {
        //private SearchView searchView;
        protected override int ViewResourceId
        {
            get { return Resource.Layout.dashboard; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var questionnairesList = this.FindViewById<MvxListView>(Resource.Id.questionnairesList);
            questionnairesList.EmptyView = this.FindViewById<LinearLayout>(Resource.Id.emptyView);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            /*var searchItem = menu.FindItem(Resource.Id.dashboard_search);
            var searchViewControl = MenuItemCompat.GetActionView(searchItem);
            this.searchView = searchViewControl.JavaCast<SearchView>();

            this.searchView.QueryTextChange += searchView_QueryTextChange;

            MenuItemCompat.SetOnActionExpandListener(searchItem, new SearchViewExpandListener(ViewModel));
            */
            return base.OnCreateOptionsMenu(menu);
        }

        //private class SearchViewExpandListener
        //   : Java.Lang.Object, MenuItemCompat.IOnActionExpandListener
        //{
        //    private readonly DashboardViewModel viewModel;

        //    public SearchViewExpandListener(DashboardViewModel viewModel)
        //    {
        //        this.viewModel = viewModel;
        //    }

        //    public bool OnMenuItemActionCollapse(IMenuItem item)
        //    {
        //        return true;
        //    }

        //    public bool OnMenuItemActionExpand(IMenuItem item)
        //    {
        //        return true;
        //    }
        //}

        //void searchView_QueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        //{
        //    ViewModel.SearchText = e.NewText;
        //    e.Handled = true;
        //}

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                //case Resource.Id.dashboard_search:
                //    //this.ViewModel.RefreshQuestionnairesCommand.Execute();
                //    break;
                case Resource.Id.dashboard_refresh:
                    this.ViewModel.RefreshQuestionnairesCommand.Execute();
                    break;
                case Resource.Id.dashboard_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.dashboard_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }
    }
}