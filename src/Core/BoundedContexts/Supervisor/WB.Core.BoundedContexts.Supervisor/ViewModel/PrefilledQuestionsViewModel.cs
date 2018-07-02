using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class PrefilledQuestionsViewModel : BasePrefilledQuestionsViewModel
    {
        public PrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            ILogger logger,
            IPrincipal principal,
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            VibrationViewModel vibrationViewModel)
            : base(
                interviewViewModelFactory,
                questionnaireRepository,
                interviewRepository,
                viewModelNavigationService,
                logger,
                principal,
                commandService,
                compositeCollectionInflationService,
                vibrationViewModel)
        {
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId));

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () =>
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
            this.Dispose();
        });

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);
        public IMvxCommand SignOutCommand => new MvxAsyncCommand(async () =>
        {
            await this.viewModelNavigationService.SignOutAndNavigateToLoginAsync();
            this.Dispose();
        });

        //public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<MapsViewModel>);
    }
}
