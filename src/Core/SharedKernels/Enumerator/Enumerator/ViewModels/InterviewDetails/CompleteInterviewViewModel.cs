using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
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
    public class TabViewModel : BaseViewModel
    {
        private const int ShowItemsCount = 10;
        public bool IsEnabled => Items.Count > 0;
        public bool ShowMore => Items.Count > ShowItemsCount;
        public string MoreCount => string.Format(UIResources.Interview_Complete_MoreCountString, Items.Count - ShowItemsCount);
        public string Title { get; set; }
        public CompleteTabContent TabContent { get; set; }
        public List<EntityWithErrorsViewModel> Items { get; set; }
        public List<EntityWithErrorsViewModel> ShortItems => Items.Take(ShowItemsCount).ToList();
        
        public string Count => Items.Count > 0 ? $"{Items.Count}" : "No";
    }
    
    public enum CompleteTabContent
    {
        Unknown,
        CriticalError,
        Error,
        Unanswered,
    }

    
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
            
            this.InterviewState.Init(interviewId, null);
            this.CompleteStatus = InterviewState.Status;
            this.Name.InitAsStatic(UIResources.Interview_Complete_Screen_Title);

            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.CompleteScreenTitle = string.Format(UIResources.Interview_Complete_Title, interviewKey);


            var questionsCount = InterviewState.QuestionsCount;
            this.AnsweredCount = InterviewState.AnsweredQuestionsCount;

            this.UnansweredCount = questionsCount - this.AnsweredCount;
            var unansweredQuestions = 
                this.entitiesListViewModelFactory.GetTopUnansweredQuestions(interviewId, navigationState, forSupervisor).ToList();

            this.ErrorsCount = InterviewState.InvalidAnswersCount;
            var entitiesWithErrors = this.entitiesListViewModelFactory.GetTopEntitiesWithErrors(interviewId, navigationState).ToList();
            this.EntitiesWithErrorsDescription = UIResources.Interview_Complete_Entities_With_Errors + " " + MoreThan(this.ErrorsCount);

            this.Tabs = new CompositeCollection<TabViewModel>();

            Tabs.Add(new TabViewModel()
            {
                Title  = "Critical\r\nerrors",
                Items = new List<EntityWithErrorsViewModel>(),// entitiesWithErrors,
                TabContent = CompleteTabContent.CriticalError
            });
            Tabs.Add(new TabViewModel()
            {
                Title  = "Questions\r\nwith errors",
                Items = entitiesWithErrors,
                TabContent = CompleteTabContent.Error
            });
            Tabs.Add(new TabViewModel()
            {
                Title  = "Unanswered\r\nquestions",
                Items = unansweredQuestions,
                TabContent = CompleteTabContent.Unanswered
            });
            

            this.Comment = lastCompletionComments.Get(this.InterviewId);
            this.CommentLabel = UIResources.Interview_Complete_Note_For_Supervisor;
            this.CompleteButtonComment = UIResources.Interview_Complete_Consequences_Instrunction;
        }
        
        public CompositeCollection<TabViewModel> Tabs { get; set; }
        
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
                var tabViewModel = Tabs.First(t => t.TabContent == CompleteTabContent.CriticalError);
                tabViewModel.Items.AddRange(TopFailedCriticalRules);
            }
            
            this.TopUnansweredCriticalQuestions = this.entitiesListViewModelFactory.GetTopUnansweredCriticalQuestions(interviewId, navigationState).ToList();
            if (TopUnansweredCriticalQuestions.Count > 0)
            {
                var tabViewModel = Tabs.First(t => t.TabContent == CompleteTabContent.CriticalError);
                tabViewModel.Items.AddRange(TopUnansweredCriticalQuestions);
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
            
            Name?.Dispose();
            Tabs?.Dispose();
            InterviewState?.DisposeIfDisposable();
            
            base.Dispose();
        }

        protected string MoreThan(int count)
            => count >= this.entitiesListViewModelFactory.MaxNumberOfEntities 
                ? this.entitiesListViewModelFactory.MaxNumberOfEntities + "+" 
                : count.ToString();
    }
}
