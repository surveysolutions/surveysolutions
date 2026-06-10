using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
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
        protected readonly IEntitiesListViewModelFactory entitiesListViewModelFactory;
        private readonly ILastCompletionComments lastCompletionComments;
        protected readonly IPrincipal principal;
        
        protected readonly IStatefulInterviewRepository interviewRepository;
        protected readonly IQuestionnaireStorage questionnaireRepository;

        protected readonly IMvxMessenger Messenger;
        
        public InterviewStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }
        public string CompleteScreenTitle { get; set; }
        
        protected CriticalityLevel? CriticalityLevel = null;
        
        public bool IsAllOk => Tabs.All(t => t.Items.Count == 0);
        
        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            ILastCompletionComments lastCompletionComments,
            InterviewStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel,
            IStatefulInterviewRepository interviewRepository, 
            IQuestionnaireStorage questionnaireRepository,
            ILogger logger)
        {
            Messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.lastCompletionComments = lastCompletionComments;

            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.Logger = logger;
        }

        protected readonly ILogger Logger;
        protected Guid InterviewId { set; get; }

        public virtual void Configure(string interviewId, NavigationState navigationState)
        {
            RunConfiguration(interviewId, navigationState);
        }

        protected void RunConfiguration(string interviewId, NavigationState navigationState, bool forSupervisor = false)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.InterviewId = Guid.Parse(interviewId);

            // Fast synchronous setup: just enough to show the screen immediately.
            // All expensive interview traversals are deferred to the background task below.
            this.Name.InitAsStatic(UIResources.Interview_Complete_Screen_Title);

            var interview = this.interviewRepository.Get(interviewId);
            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.CompleteScreenTitle = string.Format(UIResources.Interview_Complete_Title, interviewKey);

            this.Tabs = new List<TabViewModel>
            {
                new TabViewModel
                {
                    Title  = UIResources.Interview_Complete_Tab_Title_Critical,
                    Items = new(),
                    TabContent = CompleteTabContent.CriticalError,
                    Total = 0,
                },
                new TabViewModel
                {
                    Title  = UIResources.Interview_Complete_Tab_Title_WithErrors,
                    Items = new(),
                    TabContent = CompleteTabContent.Error,
                    Total = 0,
                },
                new TabViewModel
                {
                    Title  = UIResources.Interview_Complete_Tab_Title_Unanswered,
                    Items = new(),
                    TabContent = CompleteTabContent.Unanswered,
                    Total = 0,
                },
            };

            this.Comment = lastCompletionComments.Get(this.InterviewId);
            this.CommentLabel = UIResources.Interview_Complete_Note_For_Supervisor;

            // IsLoading is true by default — the screen opens immediately with a loading indicator.
            // Load counts and entity lists on a background thread.
            _loadingCts?.Cancel();
            _loadingCts?.Dispose();
            _loadingCts = new CancellationTokenSource();
            var cancellationToken = _loadingCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await LoadDataForDisplayAsync(interviewId, navigationState, forSupervisor, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // User navigated away before loading finished — expected, nothing to do.
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to load complete screen data", ex);
                    await InvokeOnMainThreadAsync(() =>
                    {
                        if (isDisposed) return;
                        IsLoading = false;
                    });
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Loads all expensive interview data (state counts, entity lists) on the calling (background) thread,
        /// then pushes UI updates onto the main thread.
        /// Subclasses can override to add extra loading steps (e.g. supervisor counts, criticality).
        /// </summary>
        protected virtual async Task LoadDataForDisplayAsync(string interviewId, NavigationState navigationState, bool forSupervisor = false, CancellationToken cancellationToken = default)
        {
            // --- Heavy work on background thread ---
            if (isDisposed) return;
            cancellationToken.ThrowIfCancellationRequested();
            this.InterviewState.Init(interviewId, null);
            var status = InterviewState.Status;
            var questionsCount = InterviewState.QuestionsCount;
            var answeredCount = InterviewState.AnsweredQuestionsCount;
            var unansweredCount = questionsCount - answeredCount;
            var errorsCount = InterviewState.InvalidAnswersCount;

            cancellationToken.ThrowIfCancellationRequested();
            var topUnansweredResult = this.entitiesListViewModelFactory.GetTopUnansweredQuestions(interviewId, navigationState, forSupervisor);
            var unansweredQuestions = topUnansweredResult.Entities.ToList();

            cancellationToken.ThrowIfCancellationRequested();
            var topErrorsResult = this.entitiesListViewModelFactory.GetTopEntitiesWithErrors(interviewId, navigationState);
            var entitiesWithErrors = topErrorsResult.Entities.ToList();

            var errorsDescription = UIResources.Interview_Complete_Entities_With_Errors + " " + MoreThan(errorsCount);
            cancellationToken.ThrowIfCancellationRequested();

            // --- Marshal UI updates to main thread ---
            await InvokeOnMainThreadAsync(() =>
            {
                if (isDisposed || cancellationToken.IsCancellationRequested)
                {
                    // Dispose ViewModels that were created but won't be added to any tab.
                    entitiesWithErrors.ForEach(vm => vm.DisposeIfDisposable());
                    unansweredQuestions.ForEach(vm => vm.DisposeIfDisposable());
                    return;
                }

                this.CompleteStatus = status;
                this.AnsweredCount = answeredCount;
                this.UnansweredCount = unansweredCount;
                this.ErrorsCount = errorsCount;
                this.EntitiesWithErrorsDescription = errorsDescription;

                var errorsTab = this.Tabs.First(t => t.TabContent == CompleteTabContent.Error);
                errorsTab.Items.AddRange(entitiesWithErrors);
                errorsTab.Total = topErrorsResult.Total;

                var unansweredTab = this.Tabs.First(t => t.TabContent == CompleteTabContent.Unanswered);
                unansweredTab.Items.AddRange(unansweredQuestions);
                unansweredTab.Total = topUnansweredResult.Total;

                RaisePropertyChanged(nameof(AnsweredCount));
                RaisePropertyChanged(nameof(UnansweredCount));
                RaisePropertyChanged(nameof(ErrorsCount));
                RaisePropertyChanged(nameof(EntitiesWithErrorsDescription));
                RaisePropertyChanged(nameof(Tabs));
                RaisePropertyChanged(nameof(IsAllOk));
            });

            cancellationToken.ThrowIfCancellationRequested();
            if (isDisposed) return;
            await OnTabDataLoadedAsync(interviewId, navigationState);
        }

        /// <summary>
        /// Called after the base tab data (counts, entity lists) has been loaded and pushed to the UI.
        /// Base implementation marks loading complete and computes completion eligibility.
        /// Subclasses override to add criticality, supervisor-specific counts, etc.
        /// </summary>
        protected virtual async Task OnTabDataLoadedAsync(string interviewId, NavigationState navigationState)
        {
            await InvokeOnMainThreadAsync(() =>
            {
                if (isDisposed) return;

                IsCompletionAllowed = CalculateIsCompletionAllowed();
                IsLoading = false;
                RaisePropertyChanged(nameof(IsAllOk));
            });
        }

        public List<TabViewModel> Tabs { get; set; } = new();
        
        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }

        public string EntitiesWithErrorsDescription { get; protected set; }

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

        public string CompleteButtonComment
        {
            get => completeButtonComment;
            protected set
            {
                if (value == completeButtonComment) return;
                completeButtonComment = value;
                RaisePropertyChanged(() => CompleteButtonComment);
            }
        }

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
        private CancellationTokenSource _loadingCts;
        protected bool isDisposed;
        private bool isCompletionAllowed;
        
        private bool hasCriticalIssues;
        private string completeButtonComment;

        public bool HasCriticalIssues
        {
            get => hasCriticalIssues;
            set => SetProperty(ref hasCriticalIssues, value);
        }
        
        protected virtual bool CalculateIsCompletionAllowed()
        {
            return true;
        }

        protected async Task CollectCriticalityInfo(string interviewId, NavigationState navigationState)
        {
            var topFailedCriticalRulesInfo = this.entitiesListViewModelFactory.GetTopFailedCriticalRules(interviewId, navigationState);
            var topFailedCriticalRules = topFailedCriticalRulesInfo.Entities.ToList();

            var topUnansweredCriticalQuestionsInfo = this.entitiesListViewModelFactory.GetTopUnansweredCriticalQuestions(interviewId, navigationState);
            var topUnansweredCriticalQuestions = topUnansweredCriticalQuestionsInfo.Entities.ToList();
            
            await InvokeOnMainThreadAsync(() =>
            {
                if (isDisposed)
                {
                    topFailedCriticalRules.ForEach(vm => vm.DisposeIfDisposable());
                    topUnansweredCriticalQuestions.ForEach(vm => vm.DisposeIfDisposable());
                    return;
                }

                var tabViewModel = Tabs.First(t => t.TabContent == CompleteTabContent.CriticalError);
                if (topFailedCriticalRules.Count > 0)
                {
                    var takeCount = Math.Max(0, entitiesListViewModelFactory.MaxNumberOfEntities - tabViewModel.Items.Count);
                    tabViewModel.Items.AddRange(topFailedCriticalRules.Take(takeCount));
                    tabViewModel.Total += topFailedCriticalRulesInfo.Total;
                }

                if (topUnansweredCriticalQuestions.Count > 0)
                {
                    var takeCount = Math.Max(0, entitiesListViewModelFactory.MaxNumberOfEntities - tabViewModel.Items.Count);
                    tabViewModel.Items.AddRange(topUnansweredCriticalQuestions.Take(takeCount));
                    tabViewModel.Total += topUnansweredCriticalQuestionsInfo.Total;
                }

                HasCriticalIssues = topUnansweredCriticalQuestions.Count > 0 || topFailedCriticalRules.Count > 0;
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
            });
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
        
        public bool HasCriticalFeature(string interviewId)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, null);
            return questionnaire.DoesSupportCriticality();
        }

        protected virtual async Task CloseInterviewAfterComplete(bool switchInterviewToCawiMode)
        {
            await this.viewModelNavigationService.NavigateFromInterviewAsync(this.InterviewId.ToString());
            Dispose();
            Messenger.Publish(new InterviewCompletedMessage(this));
        }

        public override void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            _loadingCts?.Cancel();
            _loadingCts?.Dispose();
            _loadingCts = null;
            
            Name?.Dispose();
            InterviewState?.DisposeIfDisposable();
            Tabs?.ForEach(t => t.Dispose());
            Tabs?.DisposeIfDisposable();
            
            base.Dispose();
        }

        protected string MoreThan(int count)
            => count >= this.entitiesListViewModelFactory.MaxNumberOfEntities 
                ? this.entitiesListViewModelFactory.MaxNumberOfEntities + "+" 
                : count.ToString();
    }
}
