using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Tester.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenActivity
    {
        public SplashActivity()
            : base(Resource.Layout.splash)
        {
        }

        protected override async void TriggerFirstNavigate()
        {
            await ClearAttachmentStorage();

            IPrincipal principal = Mvx.Resolve<IPrincipal>();
            IViewModelNavigationService viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            if (principal.IsAuthenticated)
            {
                await viewModelNavigationService.NavigateToAsync<DashboardViewModel>();
            }
            else
            {
                await viewModelNavigationService.NavigateToAsync<LoginViewModel>();
            }
        }

        private async Task ClearAttachmentStorage()
        {
            var attachmentContentRemover = Mvx.Resolve<IAsyncPlainStorageRemover<AttachmentContent>>();
            await attachmentContentRemover.DeleteAllAsync();
        }
    }
}