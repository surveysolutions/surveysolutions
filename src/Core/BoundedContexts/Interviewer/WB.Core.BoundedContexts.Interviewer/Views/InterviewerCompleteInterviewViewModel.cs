using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MvvmCross.Plugin.Messenger;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerCompleteInterviewViewModel : CompleteInterviewViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IPlainStorage<QuestionnaireView> interviewViewRepository;
        private readonly IUserInteractionService userInteractionService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireSettings questionnaireSettings;

        public InterviewerCompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            ICommandService commandService,
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            InterviewStateViewModel interviewState,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            DynamicTextViewModel dynamicTextViewModel,
            ILastCompletionComments lastCompletionComments,
            IAuditLogService auditLogService,
            IInterviewerSettings interviewerSettings,
            ILogger logger,
            IPlainStorage<QuestionnaireView> interviewViewRepository,
            IUserInteractionService userInteractionService,
            IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireSettings questionnaireSettings)
            : base(viewModelNavigationService, commandService, principal, 
                entitiesListViewModelFactory, lastCompletionComments,interviewState, dynamicTextViewModel, logger)
        {
            this.interviewRepository = interviewRepository;
            this.auditLogService = auditLogService;
            this.interviewerSettings = interviewerSettings;
            this.interviewViewRepository = interviewViewRepository;
            this.userInteractionService = userInteractionService;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireSettings = questionnaireSettings;

            TopUnansweredCriticalQuestions = new List<EntityWithErrorsViewModel>();
            TopFailedCriticalRules = new List<FailedCriticalRuleViewModel>();
        }

        public IList<EntityWithErrorsViewModel> TopUnansweredCriticalQuestions { get; private set; } 
        public IList<FailedCriticalRuleViewModel> TopFailedCriticalRules { get; private set; }
        public int UnansweredCriticalQuestionsCount => TopUnansweredCriticalQuestions.Count;
        public int FailedCriticalRulesCount => TopFailedCriticalRules.Count;


        public override void Configure(string interviewUid, NavigationState navigationState)
        {
            if (interviewUid == null) throw new ArgumentNullException(nameof(interviewUid));
            
            var interview = this.interviewRepository.GetOrThrow(interviewUid);
            this.criticalityLevel = questionnaireSettings.GetCriticalityLevel(interview.QuestionnaireIdentity);

            base.Configure(interviewUid, navigationState);
            
            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.CompleteScreenTitle = string.IsNullOrEmpty(interviewKey)
                ? UIResources.Interview_Complete_Screen_Description
                : string.Format(UIResources.Interview_Complete_Screen_DescriptionWithInterviewKey, interviewKey);

            var questionnaireView = interviewViewRepository.GetById(interview.QuestionnaireIdentity.ToString());
            if (questionnaireView.WebModeAllowed && interviewerSettings.WebInterviewUriTemplate != null && interview.GetAssignmentId() != null)
            {
                this.CanSwitchToWebMode = true;

                this.WebInterviewUrl = interviewerSettings.RenderWebInterviewUri(
                    interview.GetAssignmentId()!.Value,
                    interview.Id
                );
            }
            
            Task.Run(() => CollectCriticalityInfo(interviewUid, navigationState));
        }
        
        CriticalityLevel? criticalityLevel = null;
        
        private Task CollectCriticalityInfo(string interviewId, NavigationState navigationState)
        {
            if (!this.HasCriticalFeature(interviewId) || criticalityLevel == CriticalityLevel.Ignore)
            {
                IsCompletionAllowed = true;
                IsLoading = false;
                return Task.CompletedTask;
            }
            
            this.TopUnansweredCriticalQuestions = this.entitiesListViewModelFactory.GetTopUnansweredCriticalQuestions(interviewId, navigationState).ToList();
            if (TopUnansweredCriticalQuestions.Count > 0)
            {
                var unansweredCriticalQuestionsGroup = new CompleteGroup(TopUnansweredCriticalQuestions)
                {
                    AllCount = this.TopUnansweredCriticalQuestions.Count,
                    Title= string.Format(UIResources.Interview_Complete_CriticalUnanswered, this.TopUnansweredCriticalQuestions.Count),
                    GroupContent = CompleteGroupContent.Error,
                };
                CompleteGroups.Insert(0, unansweredCriticalQuestionsGroup);
            }
            
            this.TopFailedCriticalRules = this.entitiesListViewModelFactory.GetTopFailedCriticalRules(interviewId, navigationState).ToList();
            if (TopFailedCriticalRules.Count > 0)
            {
                var results = this.TopFailedCriticalRules.Select(i =>
                    EntityWithErrorsViewModel.InitError(i.EntityTitle)).ToArray();
                var failedCriticalRulesGroup = new CompleteGroup(results)
                {
                    AllCount = this.TopFailedCriticalRules.Count,
                    Title = string.Format(UIResources.Interview_Complete_FailCriticalConditions, this.TopFailedCriticalRules.Count),
                    GroupContent = CompleteGroupContent.Error,
                };
                CompleteGroups.Insert(1, failedCriticalRulesGroup);
            }

            HasCriticalIssues = UnansweredCriticalQuestionsCount > 0 || FailedCriticalRulesCount > 0;
            IsCompletionAllowed = criticalityLevel != CriticalityLevel.Block || !HasCriticalIssues;

            if (HasCriticalIssues)
                InterviewStatus = GroupStatus.CompletedInvalid; 

            if (criticalityLevel == CriticalityLevel.Warn)
            {
                this.IsCompletionAllowed = !HasCriticalIssues || !string.IsNullOrWhiteSpace(Comment);
                this.CompleteButtonComment = UIResources.Interview_Complete_Note_For_Supervisor_with_Criticality;
            }
            else
            {
                this.CompleteButtonComment = UIResources.Interview_Complete_CriticalIssues_Instrunction;
                this.IsCompletionAllowed = false;
            }
            
            IsLoading = false;
            return Task.CompletedTask;
        }
        
        private bool HasCriticalFeature(string interviewId)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, null);
            return questionnaire.DoesSupportCriticality();
        }
        
        public override string Comment
        {
            get => base.Comment;
            set
            {
                base.Comment = value;

                if (HasCriticalIssues && criticalityLevel == CriticalityLevel.Warn)
                {
                    IsCompletionAllowed = !string.IsNullOrWhiteSpace(Comment);
                }
            }
        }

        private bool hasCriticalIssues;
        public bool HasCriticalIssues
        {
            get => hasCriticalIssues;
            set => SetProperty(ref hasCriticalIssues, value);
        }

        protected override async Task CompleteInterviewAsync()
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

            await base.CompleteInterviewAsync();
        }
        
        protected override Task CloseInterviewAfterComplete(bool switchInterviewToCawiMode)
        {
            var statefulInterview = this.interviewRepository.GetOrThrow(this.interviewId.FormatGuid());

            if(switchInterviewToCawiMode)            
                auditLogService.Write(new SwitchInterviewModeAuditLogEntity(this.interviewId, statefulInterview.GetInterviewKey().ToString(), InterviewMode.CAWI));
            else
                auditLogService.Write(new CompleteInterviewAuditLogEntity(this.interviewId, statefulInterview.GetInterviewKey().ToString()));
            
            return base.CloseInterviewAfterComplete(switchInterviewToCawiMode);
        }

        public override void Dispose()
        {
            if (TopUnansweredCriticalQuestions != null)
            {
                var viewModels = TopUnansweredCriticalQuestions.ToArray();
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
            
            base.Dispose();
        }
    }
}
