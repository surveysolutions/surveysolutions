using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
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

        private QuestionnaireDownloadViewModel QuestionnaireDownloader { get; }

        public InterviewViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
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
            IJsonAllTypesSerializer jsonSerializer,
            VibrationViewModel vibrationViewModel,
            IEnumeratorSettings enumeratorSettings,
            QuestionnaireDownloadViewModel questionnaireDownloader)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState, principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, jsonSerializer, vibrationViewModel, enumeratorSettings)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.QuestionnaireDownloader = questionnaireDownloader;
        }

        public override IMvxCommand ReloadCommand => new MvxCommand(() => this.viewModelNavigationService.NavigateToInterview(this.interviewId, this.navigationState.CurrentNavigationIdentity));

        public IMvxCommand NavigateToDashboardCommand => new MvxCommand(this.NavigateToDashboard);
        public IMvxCommand ReloadQuestionnaireCommand => new MvxCommand(async () => await this.ReloadQuestionnaire());

        public IMvxCommand NavigateToSettingsCommand => new MvxCommand(this.viewModelNavigationService.NavigateToSettings);
        public IMvxCommand SignOutCommand => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);

        public override void NavigateBack()
        {
            if (this.HasPrefilledQuestions)
            {
                this.viewModelNavigationService.NavigateToPrefilledQuestions(this.interviewId);
            }
            else
            {
                this.NavigateToDashboard();
            }
        }

        private void NavigateToDashboard()
        {
            this.viewModelNavigationService.NavigateToDashboard();
            this.Dispose();
        }

        private async Task ReloadQuestionnaire()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            string questionnaireId = interview.QuestionnaireIdentity.QuestionnaireId.FormatGuid();

            await this.QuestionnaireDownloader.LoadQuestionnaireAsync(
                questionnaireId, this.QuestionnaireTitle, new Progress<string>(), CancellationToken.None);

            this.Dispose();
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
                case ScreenType.Group:
                    var activeStageViewModel = this.interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                    activeStageViewModel.Init(this.interviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                    return activeStageViewModel;
                default:
                    return null;
            }
        }
    }
}