using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(Theme = "@style/Theme.Tester", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.AdjustPan)]
    public class DashboardView : BaseActivityView<DashboardViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Dashboard; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var searchView = this.FindViewById<SearchView>(Resource.Id.search);

            int searchSrcTextId = Resources.GetIdentifier("android:id/search_src_text", null, null);
            EditText searchEditText = (EditText)searchView.FindViewById(searchSrcTextId);
            searchEditText.SetTextAppearance(this, Resource.Style.dashboardSearchEditText);

            this.FindViewById<ImageView>(Resource.Id.logo).Click += (s, e) =>
            {
                var drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer);
                if (drawer.IsDrawerOpen((int)GravityFlags.Right))
                    drawer.CloseDrawer((int)GravityFlags.Right);
                else
                    drawer.OpenDrawer((int)GravityFlags.Right);
            };
            
            this.FindViewById<SwipeRefreshLayout>(Resource.Id.questionnairelistrefresher)
                .SetColorScheme(Resource.Color.progressIndicatorFirst, Resource.Color.progressIndicatorSecond,
                    Resource.Color.progressIndicatorThird, Resource.Color.progressIndicatorFourth);

            this.FindViewById<SwipeRefreshLayout>(Resource.Id.emptyquestionnairelistrefresher)
                .SetColorScheme(Resource.Color.progressIndicatorFirst, Resource.Color.progressIndicatorSecond,
                    Resource.Color.progressIndicatorThird, Resource.Color.progressIndicatorFourth);
        }
    }
}