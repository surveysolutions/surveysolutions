using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class InterviewViewModel : BaseInterviewViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;

        public InterviewViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            GroupStateViewModel groupState,
            InterviewStateViewModel interviewState,
            CoverStateViewModel coverState,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICommandService commandService,
            IJsonAllTypesSerializer jsonSerializer)
            : base(questionnaireRepository, interviewRepository, answerToStringService, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState, principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, jsonSerializer)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public override IMvxCommand ReloadCommand => new MvxCommand(() => this.viewModelNavigationService.NavigateToInterview(this.interviewId, this.navigationState.CurrentNavigationIdentity));

        public IMvxCommand NavigateToDashboardCommand => new MvxCommand(this.viewModelNavigationService.NavigateToDashboard);
        public IMvxCommand NavigateToSettingsCommand => new MvxCommand(this.viewModelNavigationService.NavigateToSettings);
        public IMvxCommand SignOutCommand => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);

        public override void NavigateBack()
        {
            if (this.PrefilledQuestions.Any())
            {
                this.viewModelNavigationService.NavigateToPrefilledQuestions(this.interviewId);
            }
            else
            {
                this.viewModelNavigationService.NavigateToDashboard();
            }
        }

        protected override MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            switch (this.navigationState.CurrentScreenType)
            {
                case ScreenType.Complete:
                    var completeInterviewViewModel = this.interviewViewModelFactory.GetNew<CompleteInterviewViewModel>();
                    completeInterviewViewModel.Init(this.interviewId, this.navigationState);
                    return completeInterviewViewModel;
                case ScreenType.Cover:
                    var coverInterviewViewModel = this.interviewViewModelFactory.GetNew<CoverInterviewViewModel>();
                    coverInterviewViewModel.Init(this.interviewId, this.navigationState);
                    return coverInterviewViewModel;
                default:
                    var activeStageViewModel = this.interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                    activeStageViewModel.Init(this.interviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                    return activeStageViewModel;
            }
        }
    }
}