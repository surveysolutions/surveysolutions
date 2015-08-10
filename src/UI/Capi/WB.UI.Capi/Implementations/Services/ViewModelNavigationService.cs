using Android.Content;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.BoundedContexts.Tester.Services;

namespace WB.UI.Capi.Implementations.Services
{
    internal class ViewModelNavigationService : MvxNavigatingObject, IViewModelNavigationService
    {
        public void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>();
        }

        public void NavigateTo<TViewModel>(object perameterValuesObject) where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>(perameterValuesObject);
        }

        public void NavigateToDashboard()
        {
            var mvxAndroidCurrentTopActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var intent = new Intent(mvxAndroidCurrentTopActivity.Activity, typeof(DashboardActivity));
            intent.AddFlags(ActivityFlags.NoHistory);
            mvxAndroidCurrentTopActivity.Activity.StartActivity(intent);
        }
    }
}