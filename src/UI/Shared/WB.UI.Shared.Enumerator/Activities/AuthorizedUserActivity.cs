using Android.OS;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class AuthorizedUserActivity<TViewModel> : BaseActivity<TViewModel> where TViewModel : MvxViewModel
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.CheckPrincipalAndNavigateToSplash();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.CheckPrincipalAndNavigateToSplash();
        }

        /*protected override void OnRestart()
        {
            base.OnRestart();
            this.CheckPrincipalAndNavigateToSplash();
        }*/

        private void CheckPrincipalAndNavigateToSplash()
        {
            if (Mvx.Resolve<IPrincipal>()?.CurrentUserIdentity?.Name == null) // happens when application is restarted after crash
            {
                Mvx.Resolve<IViewModelNavigationService>().NavigateToSplashScreen();
            }
        }
    }
}