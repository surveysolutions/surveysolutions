using Android.App;
using Android.Content.PM;
using MvvmCross;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Tester.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : BaseSplashScreenActivity
    {
        public SplashActivity()
            : base(Resource.Layout.splash)
        {
        }

        public override void InitializationComplete()
        {
            ClearAttachmentStorage();
            ClearPlainInterviewStorage();

            Mvx.Resolve<IPlainStorage<TranslationInstance>>().RemoveAll();
            base.InitializationComplete();
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
