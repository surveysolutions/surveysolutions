using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CompleteInterviewViewModel : BaseViewModel
    {
        protected readonly IViewModelNavigationService viewModelNavigationService;
        
        private readonly ICommandService commandService;
        private readonly IEntitiesListViewModelFactory entitiesListViewModelFactory;
        private readonly ILastCompletionComments lastCompletionComments;
        protected readonly IPrincipal principal;

        protected readonly IMvxMessenger Messenger;
        
        public InterviewStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }
        public string CompleteScreenTitle { get; set; }

        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            ILastCompletionComments lastCompletionComments,
            InterviewStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage)
        {
            Messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.lastCompletionComments = lastCompletionComments;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;
        }

        protected readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;

        protected Guid interviewId;

        public virtual void Configure(string interviewId, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.interviewId = Guid.Parse(interviewId);

            this.InterviewState.Init(interviewId, null);
            this.Name.InitAsStatic(UIResources.Interview_Complete_Screen_Title);

            this.CompleteScreenTitle = UIResources.Interview_Complete_Screen_Description;

            var questionsCount = InterviewState.QuestionsCount;
            this.AnsweredCount = InterviewState.AnsweredQuestionsCount;

            this.UnansweredCount = questionsCount - this.AnsweredCount;
            var unansweredQuestions = this.entitiesListViewModelFactory.GetTopUnansweredQuestions(interviewId, navigationState).ToList();
            var unansweredGroup = new CompleteGroup(unansweredQuestions)
            {
                AllCount = this.UnansweredCount,
                Title = UIResources.Interview_Complete_Unanswered + ": " + UnansweredCount,
                GroupContent = CompleteGroupContent.Unanswered,
            };

            this.ErrorsCount = InterviewState.InvalidAnswersCount;
            this.EntitiesWithErrors = this.entitiesListViewModelFactory.GetTopEntitiesWithErrors(interviewId, navigationState).ToList();
            this.EntitiesWithErrorsDescription = EntitiesWithErrors.Count < this.ErrorsCount
                ? string.Format(UIResources.Interview_Complete_First_n_Entities_With_Errors,
                    this.entitiesListViewModelFactory.MaxNumberOfEntities)
                : UIResources.Interview_Complete_Entities_With_Errors + " " + this.ErrorsCount;
            var errorsGroup = new CompleteGroup(EntitiesWithErrors)
            {
                AllCount = this.ErrorsCount,
                Title = this.EntitiesWithErrorsDescription,
                GroupContent = CompleteGroupContent.Error,
            };
            
            this.CompleteGroups = new MvxObservableCollection<CompleteGroup>()
            {
                //unansweredCriticalQuestionsGroup,
                //failedCriticalRulesGroup,
                unansweredGroup,
                errorsGroup,
            };

            this.Comment = lastCompletionComments.Get(this.interviewId);
            this.CommentLabel = UIResources.Interview_Complete_Note_For_Supervisor;
            this.CompleteButtonComment = UIResources.Interview_Complete_Consequences_Instrunction;

            Task.Run(() => CollectCriticalityInfo(interviewId, navigationState));
        }

        public enum CriticalityLevel
        {
            Ignore,
            Warning,
            Error
        }
        
        CriticalityLevel criticalityLevel = CriticalityLevel.Warning;
        
        
        private Task CollectCriticalityInfo(string interviewId, NavigationState navigationState)
        {
            if (!this.HasCriticalFeature(interviewId))
            {
                IsCompletionAllowed = true;
                return Task.CompletedTask;
            }

            if (criticalityLevel == CriticalityLevel.Ignore)
            {
                IsCompletionAllowed = true;
                return Task.CompletedTask;
            }
            
            this.UnansweredCriticalQuestions = this.entitiesListViewModelFactory.GetTopUnansweredCriticalQuestions(interviewId, navigationState).ToList();
            var unansweredCriticalQuestionsGroup = new CompleteGroup(UnansweredCriticalQuestions)
            {
                AllCount = this.UnansweredCriticalQuestions.Count,
                Title= string.Format(UIResources.Interview_Complete_CriticalUnanswered, this.UnansweredCriticalQuestions.Count),
                GroupContent = CompleteGroupContent.Error,
            };
            CompleteGroups.Insert(0, unansweredCriticalQuestionsGroup);
            
            this.TopFailedCriticalRules = this.entitiesListViewModelFactory.GetTopFailedCriticalRules(interviewId, navigationState).ToList();
            var results = this.TopFailedCriticalRules.Select(i =>
                EntityWithErrorsViewModel.InitError(i.EntityTitle)).ToArray();
            var failedCriticalRulesGroup = new CompleteGroup(results)
            {
                AllCount = this.TopFailedCriticalRules.Count,
                Title = string.Format(UIResources.Interview_Complete_FailCriticalConditions, this.TopFailedCriticalRules.Count),
                GroupContent = CompleteGroupContent.Error,
            };
            CompleteGroups.Insert(1, failedCriticalRulesGroup);

            HasCriticalIssues = UnansweredCriticalQuestionsCount > 0 || FailedCriticalRulesCount > 0;
            IsCompletionAllowed = criticalityLevel != CriticalityLevel.Error || !HasCriticalIssues;

            if (criticalityLevel == CriticalityLevel.Warning)
            {
                this.IsCompletionAllowed = !HasCriticalIssues || !string.IsNullOrWhiteSpace(Comment);
                this.CompleteButtonComment = UIResources.Interview_Complete_Note_For_Supervisor_with_Criticality;
            }
            else
            {
                this.CompleteButtonComment = UIResources.Interview_Complete_Consequences_Instrunction;
                this.IsCompletionAllowed = false;
            }
            
            return Task.CompletedTask;
        }
        
        private bool HasCriticalFeature(string interviewId)
        {
            IStatefulInterview interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, null);
            return questionnaire.DoesSupportCriticality();
        }
        
        public MvxObservableCollection<CompleteGroup> CompleteGroups { get; set; }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }
        public int UnansweredCriticalQuestionsCount => UnansweredCriticalQuestions.Count;
        public int FailedCriticalRulesCount => TopFailedCriticalRules.Count;

        public string EntitiesWithErrorsDescription { get; private set; }

        public bool CanSwitchToWebMode
        {
            get => canSwitchToWebMode;
            set => this.RaiseAndSetIfChanged(ref this.canSwitchToWebMode, value);
        }

        public bool RequestWebInterview
        {
            get => requestWebInterview;
            set => this.RaiseAndSetIfChanged(ref this.requestWebInterview, value);
        }

        public string WebInterviewUrl { get; set; }

        public IList<EntityWithErrorsViewModel> EntitiesWithErrors { get; private set; }
        public IList<EntityWithErrorsViewModel> UnansweredCriticalQuestions { get; private set; }
        public IList<FailedCriticalRuleViewModel> TopFailedCriticalRules { get; private set; }

        private IMvxAsyncCommand completeInterviewCommand;
        public IMvxAsyncCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ??= new MvxAsyncCommand(async () =>
                    await this.CompleteInterviewAsync(), () => !WasThisInterviewCompleted && IsCompletionAllowed);
            }
        }

        public string Comment
        {
            get => comment;
            set
            {
                comment = value;
                this.lastCompletionComments.Store(this.interviewId, value);

                if (HasCriticalIssues && criticalityLevel == CriticalityLevel.Warning)
                {
                    IsCompletionAllowed = !string.IsNullOrWhiteSpace(Comment);
                }
            }
        }

        public string CommentLabel { get; protected set; }
        public string CompleteButtonComment { get; protected set; }

        private bool wasThisInterviewCompleted = false;
        public bool WasThisInterviewCompleted
        {
            get => this.wasThisInterviewCompleted;
            set => this.RaiseAndSetIfChanged(ref this.wasThisInterviewCompleted, value);
        }

        public bool IsCompletionAllowed
        {
            get => isCompletionAllowed;
            set
            {
                if (value == isCompletionAllowed) 
                    return;
                isCompletionAllowed = value;
                RaisePropertyChanged(() => IsCompletionAllowed);
                RaisePropertyChanged(() => CompleteInterviewCommand);
            }
        }

        public bool HasCriticalIssues
        {
            get => hasCriticalIssues;
            set => SetProperty(ref hasCriticalIssues, value);
        }


        private string comment;
        private bool requestWebInterview;
        private bool canSwitchToWebMode;
        private bool isDisposed;
        private bool isCompletionAllowed;
        private bool hasCriticalIssues;

        private async Task CompleteInterviewAsync()
        {
            if (!this.IsCompletionAllowed)
                return;
            
            if (this.WasThisInterviewCompleted)
                return;

            if (HasCriticalIssues)
            {
                var confirmResult = await userInteractionService.ConfirmAsync(UIResources.Interview_Complete_WithWarningCriticality,
                    okButton: UIResources.Yes,
                    cancelButton: UIResources.No);
                
                if (confirmResult == false)
                    return;
            }

            this.WasThisInterviewCompleted = true;
            await this.commandService.WaitPendingCommandsAsync();

            ICommand completeInterview = this.RequestWebInterview
            ? new ChangeInterviewModeCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                InterviewMode.CAWI,
                comment: this.Comment)
            : new CompleteInterviewCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                comment: this.Comment);

            try
            {
                await this.commandService.ExecuteAsync(completeInterview);
            }
            catch (InterviewException e)
            {
                logger.Warn("Interview has unexpected status", e);
            }

            await this.CloseInterviewAfterComplete(this.RequestWebInterview);
        }

        protected virtual async Task CloseInterviewAfterComplete(bool switchInterviewToCawiMode)
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.interviewId.ToString());
            Dispose();
            Messenger.Publish(new InterviewCompletedMessage(this));
        }

        public override void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            
            this.Name?.Dispose();

            if (EntitiesWithErrors != null)
            {
                var entitiesWithErrors = EntitiesWithErrors.ToArray();
                foreach (var entityWithErrorsViewModel in entitiesWithErrors)
                {
                    entityWithErrorsViewModel?.DisposeIfDisposable();
                }
            }

            if (UnansweredCriticalQuestions != null)
            {
                var viewModels = UnansweredCriticalQuestions.ToArray();
                foreach (var viewModel in viewModels)
                {
                    viewModel?.DisposeIfDisposable();
                }
            }

            if (TopFailedCriticalRules != null)
            {
                var errors = TopFailedCriticalRules.ToArray();
                foreach (var errorsViewModel in errors)
                {
                    errorsViewModel?.DisposeIfDisposable();
                }
            }

            this.InterviewState?.DisposeIfDisposable();
            
            base.Dispose();
        }
    }
}
