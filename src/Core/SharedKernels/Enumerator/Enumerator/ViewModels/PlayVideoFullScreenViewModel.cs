#nullable enable
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public class PlayVideoFullScreenViewModel : PlayVideoViewModel
{
    public PlayVideoFullScreenViewModel(IPrincipal principal,
        IViewModelNavigationService mvxNavigationService,
        IAttachmentContentStorage attachmentContentStorage,
        IQuestionnaireStorage questionnaireRepository,
        IStatefulInterviewRepository interviewRepository)
        : base(principal, mvxNavigationService, attachmentContentStorage, questionnaireRepository, interviewRepository)
    {
    }

    // Shadow the base command to prevent recursive fullscreen navigation when already in full screen view
    public new IMvxAsyncCommand OpenFullScreenCommand =>
        new MvxAsyncCommand(() => System.Threading.Tasks.Task.CompletedTask, () => false);
}
