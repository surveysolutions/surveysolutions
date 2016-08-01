using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Tester.ViewModels
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
            ICommandService commandService)
            : base(
                interviewViewModelFactory,
                questionnaireRepository,
                interviewRepository,
                viewModelNavigationService,
                logger,
                principal,
                commandService) {}

        public override IMvxCommand ReloadCommand => new MvxCommand(() => this.viewModelNavigationService.NavigateToPrefilledQuestions(this.interviewId));

        public IMvxCommand NavigateToDashboardCommand => new MvxCommand(this.viewModelNavigationService.NavigateToDashboard);
        public IMvxCommand NavigateToSettingsCommand => new MvxCommand(this.viewModelNavigationService.NavigateToSettings);
        public IMvxCommand SignOutCommand => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);
    }
}