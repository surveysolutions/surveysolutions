using MvvmCross;
using MvvmCross.Platforms.Android.Views;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseSplashScreenActivity : MvxSplashScreenActivity
    {
        protected BaseSplashScreenActivity(int resourceId = 0) : base(resourceId)
        {
        }

        protected override void OnResume()
        {
            CrashReporting.Init(this);
            base.OnResume();
        }

        public override void InitializationComplete()
        {
            base.InitializationComplete();
            var principal = Mvx.Resolve<IPrincipal>();
            var navigationService = Mvx.Resolve<IViewModelNavigationService>();
            if (principal.IsAuthenticated)
            {
                navigationService.NavigateToDashboardAsync().WaitAndUnwrapException();
            }
            else
            {
                navigationService.NavigateToLoginAsync().WaitAndUnwrapException();
            }
        }
    }
}
