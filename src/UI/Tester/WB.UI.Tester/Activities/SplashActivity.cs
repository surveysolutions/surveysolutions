using Android.App;
using Android.Content.PM;
using HockeyApp.Android;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Tester.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : BaseSplashScreenActivity
    {
        public SplashActivity()
            : base(Resource.Layout.splash)
        {
        }

        protected override void TriggerFirstNavigate()
        {
            ClearAttachmentStorage();
            ClearPlainInterviewStorage();

            Mvx.Resolve<IPlainStorage<TranslationInstance>>().RemoveAll();

            IPrincipal principal = Mvx.Resolve<IPrincipal>();
            IViewModelNavigationService viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            if (principal.IsAuthenticated)
            {
                viewModelNavigationService.NavigateToAsync<DashboardViewModel>().WaitAndUnwrapException();
            }
            else
            {
                viewModelNavigationService.NavigateToAsync<LoginViewModel>().WaitAndUnwrapException();
            }
        }

        private void ClearAttachmentStorage()
        {
            var attachmentContentMetadataStorage = Mvx.Resolve<IPlainStorage<AttachmentContentMetadata>>();
            attachmentContentMetadataStorage.RemoveAll();

            var attachmentContentDataStorage = Mvx.Resolve<IPlainStorage<AttachmentContentData>>();
            attachmentContentDataStorage.RemoveAll();
        }

        private void ClearPlainInterviewStorage()
        {
            var imageFileStorage = Mvx.Resolve<IImageFileStorage>();
            (imageFileStorage as IPlainFileCleaner)?.Clear();

            var audioFileStorage = Mvx.Resolve<IAudioFileStorage>();
            (audioFileStorage as IPlainFileCleaner)?.Clear();
        }
    }
}
