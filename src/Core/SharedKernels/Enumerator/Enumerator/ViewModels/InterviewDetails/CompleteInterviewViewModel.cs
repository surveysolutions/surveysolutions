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
            IUserInteractionService userInteractionService)
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
        }

        protected readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;

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
            var unansweredQuestions = this.entitiesListViewModelFactory.GetUnansweredQuestions(interviewId, navigationState).ToList();
            var unansweredGroup = new CompleteGroup(unansweredQuestions)
            {
                AllCount = this.UnansweredCount,
                Title = UIResources.Interview_Complete_Unanswered + ": " + UnansweredCount,
                GroupContent = CompleteGroupContent.Unanswered,
            };

            this.ErrorsCount = InterviewState.InvalidAnswersCount;
            this.EntitiesWithErrors = this.entitiesListViewModelFactory.GetEntitiesWithErrors(interviewId, navigationState).ToList();
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
                //failCriticalityConditionsGroup,
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
            if (!this.entitiesListViewModelFactory.HasCriticalFeature(interviewId))
            {
                IsAllowToCompleteInterview = true;
                return Task.CompletedTask;
            }

            if (criticalityLevel == CriticalityLevel.Ignore)
            {
                IsAllowToCompleteInterview = true;
                return Task.CompletedTask;
            }
            
            this.UnansweredCriticalQuestions = this.entitiesListViewModelFactory.GetUnansweredCriticalQuestions(interviewId, navigationState).ToList();
            var unansweredCriticalQuestionsGroup = new CompleteGroup(UnansweredCriticalQuestions)
            {
                AllCount = this.UnansweredCriticalQuestions.Count,
                Title= string.Format(UIResources.Interview_Complete_CriticalUnanswered, this.UnansweredCriticalQuestions.Count),
                GroupContent = CompleteGroupContent.Error,
            };
            CompleteGroups.Insert(0, unansweredCriticalQuestionsGroup);
            
            this.FailedCriticalityRules = this.entitiesListViewModelFactory.RunAndGetFailCriticalityConditions(interviewId, navigationState).ToList();
            var results = this.FailedCriticalityRules.Select(i =>
                EntityWithErrorsViewModel.InitError(i.EntityTitle)).ToArray();
            var failCriticalityConditionsGroup = new CompleteGroup(results)
            {
                AllCount = this.FailedCriticalityRules.Count,
                Title = string.Format(UIResources.Interview_Complete_FailCriticalConditions, this.FailedCriticalityRules.Count),
                GroupContent = CompleteGroupContent.Error,
            };
            CompleteGroups.Insert(1, failCriticalityConditionsGroup);

            IsExistsCriticalityProblems = UnansweredCriticalQuestionsCount > 0 || FailCriticalityConditionsCount > 0;
            IsAllowToCompleteInterview = criticalityLevel != CriticalityLevel.Error || !IsExistsCriticalityProblems;

            if (criticalityLevel == CriticalityLevel.Warning)
            {
                this.IsAllowToCompleteInterview = !IsExistsCriticalityProblems || !string.IsNullOrWhiteSpace(Comment);
                this.CommentLabel = UIResources.Interview_Complete_Note_For_Supervisor_with_Criticality;
            }
            else
            {
                this.CompleteButtonComment = UIResources.Interview_Complete_Consequences_Instrunction;
                this.IsAllowToCompleteInterview = false;
            }
            
            return Task.CompletedTask;
        }
        
        public MvxObservableCollection<CompleteGroup> CompleteGroups { get; set; }

        public enum CompleteGroupContent
        {
            Unknown,
            Error,
            Answered,
            Unanswered,
        }
        
        public class CompleteGroup : MvxObservableCollection<EntityWithErrorsViewModel>
        {
            public CompleteGroup()
            {
            }

            public CompleteGroup(IEnumerable<EntityWithErrorsViewModel> items) : base(items)
            {
            }

            public int AllCount { get; set; }
            public CompleteGroupContent GroupContent { get; set; }
            
            public string Title { get; set; }
            public bool IsError => GroupContent == CompleteGroupContent.Error && AllCount > 0;
            public bool IsAnswered => GroupContent == CompleteGroupContent.Answered && AllCount > 0;
            public bool IsUnanswered => GroupContent == CompleteGroupContent.Unanswered && AllCount > 0;
        }
        
        public class CompleteItem: MvxViewModel
        {
            public string Title { get; set; }
        }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }
        public int UnansweredCriticalQuestionsCount => UnansweredCriticalQuestions.Count;
        public int FailCriticalityConditionsCount => FailedCriticalityRules.Count;

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
        public IList<FailCriticalityConditionViewModel> FailedCriticalityRules { get; private set; }

        private IMvxAsyncCommand completeInterviewCommand;
        public IMvxAsyncCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ??= new MvxAsyncCommand(async () =>
                    await this.CompleteInterviewAsync(), () => !WasThisInterviewCompleted && IsAllowToCompleteInterview);
            }
        }

        public string Comment
        {
            get => comment;
            set
            {
                comment = value;
                this.lastCompletionComments.Store(this.interviewId, value);

                if (IsExistsCriticalityProblems && criticalityLevel == CriticalityLevel.Warning)
                {
                    IsAllowToCompleteInterview = !string.IsNullOrWhiteSpace(Comment);
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

        public bool IsAllowToCompleteInterview
        {
            get => isAllowToCompleteInterview;
            set
            {
                if (value == isAllowToCompleteInterview) 
                    return;
                isAllowToCompleteInterview = value;
                RaisePropertyChanged(() => IsAllowToCompleteInterview);
                RaisePropertyChanged(() => CompleteInterviewCommand);
            }
        }

        public bool IsExistsCriticalityProblems
        {
            get => isExistsCriticalityProblems;
            set => SetProperty(ref isExistsCriticalityProblems, value);
        }


        private string comment;
        private bool requestWebInterview;
        private bool canSwitchToWebMode;
        private bool isDisposed;
        private bool isAllowToCompleteInterview;
        private bool isExistsCriticalityProblems;

        private async Task CompleteInterviewAsync()
        {
            if (!this.IsAllowToCompleteInterview)
                return;
            
            if (this.WasThisInterviewCompleted)
                return;

            if (IsExistsCriticalityProblems)
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

            if (FailedCriticalityRules != null)
            {
                var errors = FailedCriticalityRules.ToArray();
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
