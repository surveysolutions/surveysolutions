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
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CompleteInterviewViewModel : BaseViewModel
    {
        protected readonly IViewModelNavigationService viewModelNavigationService;
        
        private readonly ICommandService commandService;
        protected readonly IEntitiesListViewModelFactory entitiesListViewModelFactory;
        private readonly ILastCompletionComments lastCompletionComments;
        protected readonly IPrincipal principal;

        protected readonly IMvxMessenger Messenger;
        
        public InterviewStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }
        public string CompleteScreenTitle { get; set; }
        
        protected CriticalityLevel? CriticalityLevel = null;
        
        public IList<EntityWithErrorsViewModel> TopUnansweredCriticalQuestions { get; protected set; } 
        public IList<EntityWithErrorsViewModel> TopFailedCriticalRules { get; protected set; }

        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            ILastCompletionComments lastCompletionComments,
            InterviewStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel,
            ILogger logger)
        {
            Messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.lastCompletionComments = lastCompletionComments;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.Logger = logger;
        }

        protected readonly ILogger Logger;
        protected Guid InterviewId { set; get; }

        public virtual void Configure(string interviewId, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.InterviewId = Guid.Parse(interviewId);
            
            this.InterviewState.Init(interviewId, null);
            this.CompleteStatus = InterviewState.Status;
            this.Name.InitAsStatic(UIResources.Interview_Complete_Screen_Title);

            this.CompleteScreenTitle = UIResources.Interview_Complete_Screen_Description;

            var questionsCount = InterviewState.QuestionsCount;
            this.AnsweredCount = InterviewState.AnsweredQuestionsCount;

            this.UnansweredCount = questionsCount - this.AnsweredCount;
            var unansweredQuestions = this.entitiesListViewModelFactory.GetTopUnansweredQuestions(interviewId, navigationState).ToList();
            var unansweredGroup = new CompleteGroup(unansweredQuestions)
            {
                AllCount = this.UnansweredCount,
                Title = UIResources.Interview_Complete_Unanswered + ": " + MoreThan(UnansweredCount),
                GroupContent = CompleteGroupContent.Unanswered,
            };

            this.ErrorsCount = InterviewState.InvalidAnswersCount;
            this.EntitiesWithErrors = this.entitiesListViewModelFactory.GetTopEntitiesWithErrors(interviewId, navigationState).ToList();
            this.EntitiesWithErrorsDescription = UIResources.Interview_Complete_Entities_With_Errors + " " + MoreThan(this.ErrorsCount);
            var errorsGroup = new CompleteGroup(EntitiesWithErrors)
            {
                AllCount = this.ErrorsCount,
                Title = this.EntitiesWithErrorsDescription,
                GroupContent = CompleteGroupContent.Error,
            };

            this.CompleteGroups = new CompositeCollection<MvxViewModel>();
            if (UnansweredCount > 0)
            {
                CompleteGroups.AddCollection(new CovariantObservableCollection<MvxViewModel>() { unansweredGroup });
                CompleteGroups.AddCollection(unansweredGroup.Items);
            }

            if (ErrorsCount > 0)
            {
                CompleteGroups.AddCollection(new CovariantObservableCollection<MvxViewModel>() { errorsGroup });
                CompleteGroups.AddCollection(errorsGroup.Items);
            }

            this.Comment = lastCompletionComments.Get(this.InterviewId);
            this.CommentLabel = UIResources.Interview_Complete_Note_For_Supervisor;
            this.CompleteButtonComment = UIResources.Interview_Complete_Consequences_Instrunction;
        }

        public bool HasCompleteGroups => CompleteGroups.Count > 0;
        public CompositeCollection<MvxViewModel> CompleteGroups { get; set; }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }

        public string EntitiesWithErrorsDescription { get; private set; }

        public bool CanSwitchToWebMode
        {
            get => canSwitchToWebMode;
            set => this.RaiseAndSetIfChanged(ref this.canSwitchToWebMode, value);
        }

        public virtual bool RequestWebInterview
        {
            get => requestWebInterview;
            set => this.RaiseAndSetIfChanged(ref this.requestWebInterview, value);
        }

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        private GroupStatus completeStatus;
        public GroupStatus CompleteStatus
        {
            get => completeStatus;
            set => this.RaiseAndSetIfChanged(ref this.completeStatus, value);
        }

        public string WebInterviewUrl { get; set; }

        public IList<EntityWithErrorsViewModel> EntitiesWithErrors { get; private set; }

        private IMvxAsyncCommand completeInterviewCommand;
        public IMvxAsyncCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ??= new MvxAsyncCommand(async () =>
                    await this.CompleteInterviewAsync(), () => !WasThisInterviewCompleted && IsCompletionAllowed);
            }
        }

        public virtual string Comment
        {
            get => comment;
            set
            {
                comment = value;
                this.lastCompletionComments.Store(this.InterviewId, value);
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
                RaisePropertyChanged(() => CompleteStatus);
            }
        }

        private string comment;
        private bool requestWebInterview;
        private bool canSwitchToWebMode;
        private bool isDisposed;
        private bool isCompletionAllowed;
        
        private bool hasCriticalIssues;
        public bool HasCriticalIssues
        {
            get => hasCriticalIssues;
            set => SetProperty(ref hasCriticalIssues, value);
        }
        
        public int UnansweredCriticalQuestionsCount => TopUnansweredCriticalQuestions.Count;
        public int FailedCriticalRulesCount => TopFailedCriticalRules.Count;

        protected virtual bool CalculateIsCompletionAllowed()
        {
            return true;
        }

        protected Task CollectCriticalityInfo(string interviewId, NavigationState navigationState)
        {
            this.TopFailedCriticalRules = this.entitiesListViewModelFactory.GetTopFailedCriticalRules(interviewId, navigationState).ToList();
            if (TopFailedCriticalRules.Count > 0)
            {
                var failedCriticalRulesGroup = new CompleteGroup(TopFailedCriticalRules)
                {
                    AllCount = this.TopFailedCriticalRules.Count,
                    Title = string.Format(UIResources.Interview_Complete_FailCriticalConditions, MoreThan(this.TopFailedCriticalRules.Count)),
                    GroupContent = CompleteGroupContent.Error,
                };
                CompleteGroups.InsertCollection(0, failedCriticalRulesGroup.Items);
                CompleteGroups.InsertCollection(0, new CovariantObservableCollection<MvxViewModel>() { failedCriticalRulesGroup });
            }
            
            this.TopUnansweredCriticalQuestions = this.entitiesListViewModelFactory.GetTopUnansweredCriticalQuestions(interviewId, navigationState).ToList();
            if (TopUnansweredCriticalQuestions.Count > 0)
            {
                var unansweredCriticalQuestionsGroup = new CompleteGroup(TopUnansweredCriticalQuestions)
                {
                    AllCount = this.TopUnansweredCriticalQuestions.Count,
                    Title= string.Format(UIResources.Interview_Complete_CriticalUnanswered, MoreThan(this.TopUnansweredCriticalQuestions.Count)),
                    GroupContent = CompleteGroupContent.Error,
                };
                CompleteGroups.InsertCollection(0, unansweredCriticalQuestionsGroup.Items);
                CompleteGroups.InsertCollection(0, new CovariantObservableCollection<MvxViewModel>() { unansweredCriticalQuestionsGroup });
            }
            
            HasCriticalIssues = UnansweredCriticalQuestionsCount > 0 || FailedCriticalRulesCount > 0;

            if (HasCriticalIssues)
            {
                CompleteStatus = GroupStatus.CompletedInvalid;

                if (CriticalityLevel == SharedKernels.DataCollection.ValueObjects.Interview.CriticalityLevel.Warn)
                {
                    this.CompleteButtonComment = UIResources.Interview_Complete_Note_For_Supervisor_with_Criticality;
                }
                else
                {
                    this.CompleteButtonComment = UIResources.Interview_Complete_CriticalIssues_Instrunction;
                }
            }

            IsCompletionAllowed = CalculateIsCompletionAllowed();
            IsLoading = false;
            return Task.CompletedTask;
        }
        
        protected virtual async Task CompleteInterviewAsync()
        {
            if (!this.IsCompletionAllowed)
                return;
            
            if (this.WasThisInterviewCompleted)
                return;
            
            this.WasThisInterviewCompleted = true;
            await this.commandService.WaitPendingCommandsAsync();

            ICommand completeInterview = this.RequestWebInterview
            ? new ChangeInterviewModeCommand(
                interviewId: this.InterviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                InterviewMode.CAWI,
                comment: this.Comment)
            : new CompleteInterviewCommand(
                interviewId: this.InterviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                comment: this.Comment,
                criticalityLevel: CriticalityLevel);

            try
            {
                await this.commandService.ExecuteAsync(completeInterview);
            }
            catch (InterviewException e)
            {
                Logger.Warn("Interview has unexpected status", e);
            }

            await this.CloseInterviewAfterComplete(this.RequestWebInterview);
        }

        protected virtual async Task CloseInterviewAfterComplete(bool switchInterviewToCawiMode)
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId.ToString());
            Dispose();
            Messenger.Publish(new InterviewCompletedMessage(this));
        }

        public override void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            
            this.Name?.Dispose();
            
            CompleteGroups.Dispose();

            this.InterviewState?.DisposeIfDisposable();
            
            base.Dispose();
        }

        protected string MoreThan(int count)
            => count >= this.entitiesListViewModelFactory.MaxNumberOfEntities 
                ? this.entitiesListViewModelFactory.MaxNumberOfEntities + "+" 
                : count.ToString();
    }
}
