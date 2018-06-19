using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class PrefilledQuestionsViewModel : BasePrefilledQuestionsViewModel
    {
        private readonly QuestionnaireDownloadViewModel questionnaireDownloader;

        public PrefilledQuestionsViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelNavigationService viewModelNavigationService,
            ILogger logger,
            IPrincipal principal,
            ICommandService commandService,
            ICompositeCollectionInflationService compositeCollectionInflationService,
            VibrationViewModel vibrationViewModel,
            QuestionnaireDownloadViewModel questionnaireDownloader)
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
            this.questionnaireDownloader = questionnaireDownloader;
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId));

        public IMvxAsyncCommand ReloadQuestionnaireCommand => new MvxAsyncCommand(this.ReloadQuestionnaire, () => !this.IsInProgress);

        private async Task ReloadQuestionnaire()
        {
            if (this.IsInProgress) return;

            this.IsInProgress = true;
            try
            {
                var interview = this.interviewRepository.Get(this.InterviewId);
                string questionnaireId = interview.QuestionnaireIdentity.QuestionnaireId.FormatGuid();

                bool succeeded = await this.questionnaireDownloader.ReloadQuestionnaireAsync(
                    questionnaireId, this.QuestionnaireTitle, interview, new NavigationIdentity() { TargetScreen = ScreenType.Identifying}, 
                    new Progress<string>(), CancellationToken.None);

                if (!succeeded) return;

                this.Dispose();
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () =>
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync();
            this.Dispose();
        });
        public IMvxCommand NavigateToSettingsCommand => new MvxCommand(this.viewModelNavigationService.NavigateToSettings);
        public IMvxCommand SignOutCommand => new MvxAsyncCommand(async () =>
        {
            await this.viewModelNavigationService.SignOutAndNavigateToLoginAsync();
            this.Dispose();
        });
    }
}
