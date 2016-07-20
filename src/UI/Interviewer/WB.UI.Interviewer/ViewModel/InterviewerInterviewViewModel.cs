using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewerInterviewViewModel : EnumeratorInterviewViewModel
    {
        readonly IStatefulInterviewRepository interviewRepository;
        readonly IViewModelNavigationService viewModelNavigationService;

        public InterviewerInterviewViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            GroupStateViewModel groupState,
            InterviewStateViewModel interviewState,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICommandService commandService,
            IJsonAllTypesSerializer jsonSerializer)
            : base(questionnaireRepository, interviewRepository, answerToStringService, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, jsonSerializer)
        {
            this.interviewRepository = interviewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
        }
        
        public IMvxCommand NavigateToDashboardCommand => new MvxCommand(this.viewModelNavigationService.NavigateToDashboard);

        public IMvxCommand SignOutCommand => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);

        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxCommand(() => this.viewModelNavigationService.NavigateTo<DiagnosticsViewModel>());

        public void NavigateToPreviousViewModel(Action navigateToIfHistoryIsEmpty)
            => this.navigationState.NavigateBack(navigateToIfHistoryIsEmpty);

        public void NavigateBack()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            if (this.PrefilledQuestions != null && this.PrefilledQuestions.Any() && interview.CreatedOnClient)
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
            if (this.navigationState.CurrentScreenType == ScreenType.Complete)
            {
                var completeInterviewViewModel = interviewViewModelFactory.GetNew<InterviewerCompleteInterviewViewModel>();
                completeInterviewViewModel.Init(this.interviewId, this.navigationState);
                return completeInterviewViewModel;
            }
            else
            {
                var activeStageViewModel = interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                activeStageViewModel.Init(interviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                return activeStageViewModel;
            }
        }
    }
}