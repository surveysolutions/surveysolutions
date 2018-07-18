using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Tester
{
    public class TesterAppStart : MvxAppStart
    {
        public TesterAppStart(IMvxApplication application, IMvxNavigationService navigationService) : base(application, navigationService)
        {
        }

        protected override void Startup(object hint = null)
        {
            ClearAttachmentStorage();
            ClearPlainInterviewStorage();

            Mvx.Resolve<IPlainStorage<TranslationInstance>>().RemoveAll();

            base.Startup(hint);
        }

        protected override void NavigateToFirstViewModel(object hint = null)
        {
            IPrincipal principal = Mvx.Resolve<IPrincipal>();
            IViewModelNavigationService viewModelNavigationService = Mvx.Resolve<IViewModelNavigationService>();

            if (principal.IsAuthenticated)
            {
                viewModelNavigationService.NavigateToDashboardAsync().WaitAndUnwrapException();
            }
            else
            {
                viewModelNavigationService.NavigateToLoginAsync().WaitAndUnwrapException();
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
