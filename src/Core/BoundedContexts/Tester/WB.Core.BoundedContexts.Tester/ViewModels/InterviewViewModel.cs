using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class InterviewViewModel : BaseInterviewViewModel
    {
        private readonly ITesterPrincipal testerPrincipal;
        private QuestionnaireDownloadViewModel QuestionnaireDownloader { get; }

        public InterviewViewModel(
            ITesterPrincipal testerPrincipal,
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
            VibrationViewModel vibrationViewModel,
            IEnumeratorSettings enumeratorSettings,
            QuestionnaireDownloadViewModel questionnaireDownloader)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState, testerPrincipal, viewModelNavigationService,
                interviewViewModelFactory, commandService, vibrationViewModel, enumeratorSettings)
        {
            this.testerPrincipal = testerPrincipal;
            this.QuestionnaireDownloader = questionnaireDownloader;
            
            if (testerPrincipal.CurrentUserIdentity == null)
                testerPrincipal.UseFakeIdentity();
        }

        public bool IsRealUserAuthenticated => testerPrincipal.IsAuthenticated && !testerPrincipal.IsFakeIdentity;
        
        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToInterviewAsync(this.InterviewId, this.NavigationState.CurrentNavigationIdentity));

        public IMvxAsyncCommand ReloadQuestionnaireCommand => new MvxAsyncCommand(this.ReloadQuestionnaire, () => !this.IsInProgress);

        public IMvxAsyncCommand NavigateToAnonymousQuestionnairesCommand => new MvxAsyncCommand(async () =>
        {
            await this.ViewModelNavigationService.NavigateToAsync<AnonymousQuestionnairesViewModel>();
            this.Dispose();
        });

        public override async Task NavigateBack()
        {
            await this.ViewModelNavigationService.NavigateToDashboardAsync();
            this.Dispose();
        }

        private async Task ReloadQuestionnaire()
        {
            if (this.IsInProgress) return;

            this.IsInProgress = true;
            try
            {
                var interview = this.interviewRepository.Get(this.InterviewId);
                string questionnaireId = interview.QuestionnaireIdentity.QuestionnaireId.FormatGuid();

                bool succeeded = await this.QuestionnaireDownloader.ReloadQuestionnaireAsync(
                    questionnaireId, this.QuestionnaireTitle, interview, this.NavigationState.CurrentNavigationIdentity,
                    new Progress<string>(), CancellationToken.None);

                if (!succeeded) return;

                this.Dispose();
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
