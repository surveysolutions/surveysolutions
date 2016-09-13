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

        protected override void TriggerFirstNavigate()
        {
            ClearAttachmentStorage();

            Mvx.Resolve<IPlainStorage<TranslationInstance>>().RemoveAll();

            IPrincipal principal = Mvx.Resolve<IPrincipal>();
            IViewModelNavigationService viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            if (principal.IsAuthenticated)
            {
                viewModelNavigationService.NavigateTo<DashboardViewModel>();
            }
            else
            {
                viewModelNavigationService.NavigateTo<LoginViewModel>();
            }
        }

        private void ClearAttachmentStorage()
        {
            var attachmentContentMetadataStorage = Mvx.Resolve<IPlainStorage<AttachmentContentMetadata>>();
            attachmentContentMetadataStorage.RemoveAll();

            var attachmentContentDataStorage = Mvx.Resolve<IPlainStorage<AttachmentContentData>>();
            attachmentContentDataStorage.RemoveAll();
        }
    }
}