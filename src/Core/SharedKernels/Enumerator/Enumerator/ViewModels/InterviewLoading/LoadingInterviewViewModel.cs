using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading
{
    public class LoadingInterviewViewModel : ProgressViewModel, IMvxViewModel<LoadingViewModelArg>
    {
        private readonly IPlainStorage<InterviewView> interviewsRepository;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly IUserInteractionService interactionService;
        private CancellationTokenSource loadingCancellationTokenSource;
        protected IAuditLogService auditLogService;
        private readonly IViewModelEventRegistry viewModelEventRegistry;

        public LoadingInterviewViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IStatefulInterviewRepository interviewRepository,
            ICommandService commandService,
            ILogger logger,
            IUserInteractionService interactionService,
            IPlainStorage<InterviewView> interviewsRepository,
            IJsonAllTypesSerializer serializer,
            IAuditLogService auditLogService,
            IViewModelEventRegistry viewModelEventRegistry)
            : base(principal, viewModelNavigationService)
        {
            this.interviewRepository = interviewRepository;
            this.commandService = commandService;
            this.logger = logger;
            this.interactionService = interactionService;
            this.interviewsRepository = interviewsRepository;
            this.serializer = serializer;
            this.auditLogService = auditLogService;
            this.viewModelEventRegistry = viewModelEventRegistry;
        }

        protected Guid InterviewId { get; set; }
        private bool ShouldReopen { get; set; }


        public void Prepare(LoadingViewModelArg arg)
        {
            this.InterviewId = arg.InterviewId;
            this.ShouldReopen = arg.ShouldReopen;

            this.IsIndeterminate = false;
        }

        public override Task Initialize()
        {
            if (InterviewId == Guid.Empty) throw new ArgumentException(nameof(InterviewId));

            var interview = this.interviewsRepository.GetById(this.InterviewId.FormatGuid());
            this.QuestionnaireTitle = interview?.QuestionnaireTitle;

            return Task.CompletedTask;
        }

        public override void ViewAppeared()
        {
            Task.Run(() => LoadAndNavigateToInterviewAsync(this.InterviewId));
        }

        public async Task LoadAndNavigateToInterviewAsync(Guid interviewId)
        {
            viewModelEventRegistry.WriteToLogInfoBySubscribers();
            viewModelEventRegistry.Reset();
            
            var interview = await LoadInterviewAsync(interviewId);
            if (interview == null)
            {
                await this.ViewModelNavigationService.NavigateToDashboardAsync()
                    .ConfigureAwait(false);
                return;
            }

            if (interview.Status == InterviewStatus.Completed && this.ShouldReopen)
            {
                this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();
                var restartInterviewCommand = new RestartInterviewCommand(interviewId,
                    this.Principal.CurrentUserIdentity.UserId, "", DateTime.UtcNow);
                await this.commandService.ExecuteAsync(restartInterviewCommand)
                    .ConfigureAwait(false);

                auditLogService.Write(new RestartInterviewAuditLogEntity(interviewId, interview.GetInterviewKey().ToString()));
            }

            else if (interview.Mode == InterviewMode.CAWI && this.ShouldReopen)
            {
                this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();
                var requestCapimodeCommand = new ChangeInterviewModeCommand(interviewId,
                    this.Principal.CurrentUserIdentity.UserId, InterviewMode.CAPI);
                
                await this.commandService.ExecuteAsync(requestCapimodeCommand)
                    .ConfigureAwait(false);

                auditLogService.Write(new SwitchInterviewModeAuditLogEntity(interviewId, interview.GetInterviewKey().ToString(), InterviewMode.CAPI));
            }

            var interviewView = this.interviewsRepository.GetById(interviewId.FormatGuid());

            if (interviewView != null && 
                !string.IsNullOrEmpty(interviewView.LastVisitedSectionId))
            {
                var targetIdentity = this.serializer.Deserialize<Identity>(interviewView.LastVisitedSectionId);
                    
                await this.ViewModelNavigationService.NavigateToInterviewAsync(interviewId.FormatGuid(), 
                    navigationIdentity: new NavigationIdentity
                    {
                        TargetScreen = interviewView.LastVisitedScreenType.GetValueOrDefault(ScreenType.Group),
                        TargetGroup = targetIdentity
                    }).ConfigureAwait(false);
                return;
            }
             
            if (interview.HasEditableIdentifyingQuestions)
            {
                await this.ViewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewId.FormatGuid());
            }
            else
            {
                await this.ViewModelNavigationService.NavigateToInterviewAsync(interviewId.FormatGuid(), navigationIdentity: null);
            }
        }

        protected async Task<IStatefulInterview> LoadInterviewAsync(Guid interviewId)
        {
            this.ProgressDescription = EnumeratorUIResources.Interview_Loading;
            this.OperationDescription = EnumeratorUIResources.Interview_Loading_Description;
            IsIndeterminate = false;

            this.loadingCancellationTokenSource = new CancellationTokenSource();
            var interviewIdString = interviewId.FormatGuid();

            var progress = new Progress<EventReadingProgress>(e =>
            {
                var percent = e.Current.PercentOf(e.Maximum);
                this.ProgressDescription = string.Format(EnumeratorUIResources.Interview_Loading_With_Percents, percent);
                this.Progress = percent;
            });

            try
            {
                this.loadingCancellationTokenSource.Token.ThrowIfCancellationRequested();

                IStatefulInterview interview = await this.interviewRepository.GetAsync(interviewIdString, progress, this.loadingCancellationTokenSource.Token)
                    .ConfigureAwait(false);

                //DB has no events
                //state is broken
                //remove from dashboard storage and return to dashboard
                if (interview == null)
                {
                    this.logger.Error($"Failed to load interview {interviewId}. Stream is empty. Removing interview.");
                    this.interviewsRepository.Remove(interviewId.FormatGuid());
                }

                return interview;
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception exception)
            {
                this.logger.Error($"Failed to load interview {interviewId}. {exception.ToString()}", exception);
                await this.interactionService.AlertAsync(EnumeratorUIResources.FailedToLoadInterviewDescription, EnumeratorUIResources.FailedToLoadInterview);
            }

            return null;
        }


        public override void CancelLoading()
        {
            if (this.loadingCancellationTokenSource != null && !this.loadingCancellationTokenSource.IsCancellationRequested)
            {
                this.loadingCancellationTokenSource.Cancel();
            }
        }

        public override void Dispose()
        {
            loadingCancellationTokenSource?.Dispose();
            base.Dispose();
        }
    }
}
