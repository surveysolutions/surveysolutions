using Android.OS;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class AuthorizedUserActivity<TViewModel> : BaseActivity<TViewModel> where TViewModel : MvxViewModel
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (Mvx.Resolve<IPrincipal>()?.CurrentUserIdentity?.Name == null) // happens when application is rested after crash
            {
                Mvx.Resolve<IViewModelNavigationService>().NavigateToSplashScreen();
            }
        }
    }
}