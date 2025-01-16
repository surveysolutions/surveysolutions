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
            IEnumeratorSettings enumeratorSettings,
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            VibrationViewModel vibrationViewModel)
            : base(
                interviewViewModelFactory,
                questionnaireRepository,
                interviewRepository,
                viewModelNavigationService,
                enumeratorSettings,
                logger,
                principal,
                commandService,
                compositeCollectionInflationService,
                vibrationViewModel)
        {
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId));

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () =>
        {
            await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
            this.Dispose();
        });

        public IMvxCommand NavigateToDiagnosticsPageCommand => 
            new MvxAsyncCommand(() => this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>());
        public IMvxCommand SignOutCommand => new MvxAsyncCommand(async () =>
        {
            await this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync();
            this.Dispose();
        });

        //public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<MapsViewModel>);
    }
}
