using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
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

        protected override async Task<object> ApplicationStartup(object hint = null)
        {
            ClearAttachmentStorage();
            ClearPlainInterviewStorage();

            Mvx.IoCProvider.Resolve<IPlainStorage<TranslationInstance>>().RemoveAll();

            return await base.ApplicationStartup(hint);
        }

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            IPrincipal principal = Mvx.IoCProvider.Resolve<IPrincipal>();
            IViewModelNavigationService viewModelNavigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();

            if (principal.IsAuthenticated)
            {
                await viewModelNavigationService.NavigateToDashboardAsync();
            }
            else
            {
                await viewModelNavigationService.NavigateToLoginAsync();
            }
        }

        private void ClearAttachmentStorage()
        {
            var attachmentContentMetadataStorage = Mvx.IoCProvider.Resolve<IPlainStorage<AttachmentContentMetadata>>();
            attachmentContentMetadataStorage.RemoveAll();

            var attachmentContentDataStorage = Mvx.IoCProvider.Resolve<IPlainStorage<AttachmentContentData>>();
            attachmentContentDataStorage.RemoveAll();
        }

        private void ClearPlainInterviewStorage()
        {
            var imageFileStorage = Mvx.IoCProvider.Resolve<IImageFileStorage>();
            (imageFileStorage as IPlainFileCleaner)?.Clear();

            var audioFileStorage = Mvx.IoCProvider.Resolve<IAudioFileStorage>();
            (audioFileStorage as IPlainFileCleaner)?.Clear();
        }
    }
}
