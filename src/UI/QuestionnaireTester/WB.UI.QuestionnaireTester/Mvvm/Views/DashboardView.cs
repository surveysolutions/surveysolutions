using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Widget;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(Theme = "@style/Theme.Tester", HardwareAccelerated = true)]
    public class DashboardView : BaseActivityView<DashboardViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Dashboard; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.FindViewById<ImageView>(Resource.Id.logo).Click += (s, e) =>
            {
                var drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer);
                var sidebar = this.FindViewById<LinearLayout>(Resource.Id.sidebar);
                if (drawer.IsDrawerOpen(sidebar))
                    drawer.CloseDrawer(sidebar);
                else
                    drawer.OpenDrawer(sidebar);
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