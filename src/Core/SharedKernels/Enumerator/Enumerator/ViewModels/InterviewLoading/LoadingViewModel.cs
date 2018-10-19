using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading
{
    public class LoadingViewModel : BaseViewModel<LoadingViewModelArg>, IDisposable
    {
        protected Guid interviewId;
        private readonly IInterviewerInterviewAccessor interviewFactory;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly IUserInteractionService interactionService;
        private CancellationTokenSource loadingCancellationTokenSource;

        public LoadingViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IStatefulInterviewRepository interviewRepository,
            ICommandService commandService,
            ILogger logger,
            IUserInteractionService interactionService,
            IQuestionnaireStorage questionnaireRepository,
            IInterviewerInterviewAccessor interviewFactory)
            : base(principal, viewModelNavigationService)
        {
            this.interviewRepository = interviewRepository;
            this.commandService = commandService;
            this.logger = logger;
            this.interactionService = interactionService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewFactory = interviewFactory;
        }

        public IMvxCommand CancelLoadingCommand => new MvxCommand(this.CancelLoading);

        public void Dispose()
        {
        }

        public override void Prepare(LoadingViewModelArg arg)
        {
            this.interviewId = arg.InterviewId;
            this.shouldReopen = arg.ShouldReopen;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            if (interviewId == Guid.Empty) throw new ArgumentException(nameof(interviewId));
            this.ProgressDescription = InterviewerUIResources.Interview_Loading;
        }

        public override void ViewAppeared()
        {
            Task.Run(RestoreInterviewAndNavigateThereAsync);
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

                IStatefulInterview interview = await this.interviewRepository.GetAsync(interviewIdString, progress, this.loadingCancellationTokenSource.Token)
                                                                             .ConfigureAwait(false);

                //DB has no events
                //state is broken
                //remove from storage and return to dashboard
                if (interview == null)
                {
                    await this.interactionService.AlertAsync(InterviewerUIResources.FailedToLoadInterviewDescription, InterviewerUIResources.FailedToLoadInterview);

                    this.logger.Error($"Failed to load interview {this.interviewId}. Stream is empty. Removing interview." );
                    this.interviewFactory.RemoveInterview(this.interviewId);

                    await this.viewModelNavigationService.NavigateToDashboardAsync();
                }
                else
                {
                    this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

                    if (interview.Status == InterviewStatus.Completed && this.shouldReopen)
                    {
                        this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();
                        var restartInterviewCommand = new RestartInterviewCommand(this.interviewId,
                            this.Principal.CurrentUserIdentity.UserId, "", DateTime.UtcNow);
                        await this.commandService.ExecuteAsync(restartInterviewCommand).ConfigureAwait(false);
                    }

                    this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    if (interview.HasEditableIdentifyingQuestions)
                    {
                        await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewIdString);
                    }
                    else
                    {
                        await this.viewModelNavigationService.NavigateToInterviewAsync(interviewIdString,
                            navigationIdentity: null);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception exception)
            {
                await this.interactionService.AlertAsync(exception.Message, InterviewerUIResources.FailedToLoadInterview);
                this.logger.Error($"Failed to load interview {this.interviewId}. {exception.ToString()}", exception);
                await this.viewModelNavigationService.NavigateToDashboardAsync();
            }
            finally
            {
                progress.ProgressChanged -= Progress_ProgressChanged;
            }
        }

        private string progressDescription;
        private bool shouldReopen;

        public string ProgressDescription
        {
            get => this.progressDescription;
            set => SetProperty(ref this.progressDescription, value);
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

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToDashboardAsync());

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.viewModelNavigationService.SignOutAndNavigateToLoginAsync);
    }
}