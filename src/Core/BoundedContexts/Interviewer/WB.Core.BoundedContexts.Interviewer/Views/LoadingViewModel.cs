using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoadingViewModel : BaseViewModel, IDisposable
    {
        protected Guid interviewId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private CancellationTokenSource loadingCancellationTokenSource;

        public LoadingViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IStatefulInterviewRepository interviewRepository,
            ICommandService commandService,
            IPlainQuestionnaireRepository questionnaireRepository)
            : base(principal, viewModelNavigationService)
        {
            this.interviewRepository = interviewRepository;
            this.commandService = commandService;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.questionnaireRepository = questionnaireRepository;
        }

        public IMvxCommand CancelLoadingCommand => new MvxCommand(this.CancelLoading);

        public void Dispose()
        {
        }

        public void Init(Guid interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.interviewId = interviewId;
            this.ProgressDescription = InterviewerUIResources.Interview_Loading;
        }

        public async Task RestoreInterviewAndNavigateThereAsync()
        {
            this.loadingCancellationTokenSource = new CancellationTokenSource();
            var interviewIdString = this.interviewId.FormatGuid();

            var progress = new Progress<EventReadingProgress>();
            progress.ProgressChanged += Progress_ProgressChanged;
            try
            {
                this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();
                
                IStatefulInterview interview =
                    await this.interviewRepository.GetAsync(interviewIdString, progress, this.loadingCancellationTokenSource.Token).ConfigureAwait(false);
         
                await Task.Run(() => this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity));

                if (interview.Status == InterviewStatus.Completed)
                {
                    this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();
                    var restartInterviewCommand = new RestartInterviewCommand(this.interviewId, this.principal.CurrentUserIdentity.UserId, "", DateTime.UtcNow);
                    await this.commandService.ExecuteAsync(restartInterviewCommand).ConfigureAwait(false);
                }

                this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();

                if (interview.CreatedOnClient)
                {
                    this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewIdString);
                }
                else
                {
                    this.viewModelNavigationService.NavigateToInterview(interviewIdString);
                }
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                progress.ProgressChanged -= Progress_ProgressChanged;
            }
        }

        private string progressDescription;
        public string ProgressDescription
        {
            get { return this.progressDescription; }
            set { this.RaiseAndSetIfChanged(ref this.progressDescription, value); }
        }

        private void Progress_ProgressChanged(object sender, EventReadingProgress e)
        {
            var percent = e.Current.PercentOf(e.Maximum);
            this.ProgressDescription = string.Format(InterviewerUIResources.Interview_Loading_With_Percents, percent);
        }

        public void CancelLoading()
        {
            if (this.loadingCancellationTokenSource != null && !this.loadingCancellationTokenSource.IsCancellationRequested)
            {
                this.loadingCancellationTokenSource.Cancel();
            }
        }

        public IMvxCommand NavigateToDashboardCommand => new MvxCommand(this.viewModelNavigationService.NavigateToDashboard);

        public IMvxCommand SignOutCommand => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);
    }
}