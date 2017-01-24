using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
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
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            ILiteEventRegistry eventRegistry,
            IEnumeratorSettings settings,
            IVirbationService virbationService)
            : base(
                interviewViewModelFactory,
                questionnaireRepository,
                interviewRepository,
                viewModelNavigationService,
                logger,
                principal,
                commandService,
                compositeCollectionInflationService,
                eventRegistry,
                settings,
                virbationService) {}

        public override IMvxCommand ReloadCommand => new MvxCommand(() => this.viewModelNavigationService.NavigateToPrefilledQuestions(this.interviewId));

        public IMvxCommand NavigateToDashboardCommand => new MvxCommand(() =>
        {
            this.viewModelNavigationService.NavigateToDashboard();
            this.Dispose();
        });
        public IMvxCommand NavigateToSettingsCommand => new MvxCommand(this.viewModelNavigationService.NavigateToSettings);
        public IMvxCommand SignOutCommand => new MvxCommand(() =>
        {
            this.viewModelNavigationService.SignOutAndNavigateToLogin();
            this.Dispose();
        });
    }
}