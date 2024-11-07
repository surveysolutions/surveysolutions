using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAuditLogService auditLogService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IUserInteractionService userInteractionService;
        private readonly IQuestionnaireSettings questionnaireSettings;

        public InterviewerCompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            ICommandService commandService,
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            InterviewStateViewModel interviewState,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            DynamicTextViewModel dynamicTextViewModel,
            ILastCompletionComments lastCompletionComments,
            IAuditLogService auditLogService,
            IInterviewerSettings interviewerSettings,
            ILogger logger,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IUserInteractionService userInteractionService,
            IQuestionnaireSettings questionnaireSettings)
            : base(viewModelNavigationService, commandService, principal, 
                entitiesListViewModelFactory, lastCompletionComments,interviewState, dynamicTextViewModel,
                interviewRepository,
                questionnaireRepository,
                logger)
        {
            this.auditLogService = auditLogService;
            this.interviewerSettings = interviewerSettings;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.userInteractionService = userInteractionService;
            this.questionnaireSettings = questionnaireSettings;

            TopUnansweredCriticalQuestions = new List<EntityWithErrorsViewModel>();
            TopFailedCriticalRules = new List<EntityWithErrorsViewModel>();
        }
        public override void Configure(string interviewUid, NavigationState navigationState)
        {
            if (interviewUid == null) throw new ArgumentNullException(nameof(interviewUid));
            
            var interview = this.interviewRepository.GetOrThrow(interviewUid);
            this.CriticalityLevel = questionnaireSettings.GetCriticalityLevel(interview.QuestionnaireIdentity);

            RunConfiguration(interviewUid, navigationState);
            
            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.CompleteScreenTitle = string.IsNullOrEmpty(interviewKey)
                ? UIResources.Interview_Complete_Screen_Description
                : string.Format(UIResources.Interview_Complete_Screen_DescriptionWithInterviewKey, interviewKey);

            var questionnaireView = questionnaireViewRepository.GetById(interview.QuestionnaireIdentity.ToString());
            if (questionnaireView.WebModeAllowed && interviewerSettings.WebInterviewUriTemplate != null && interview.GetAssignmentId() != null)
            {
                this.CanSwitchToWebMode = true;

                this.WebInterviewUrl = interviewerSettings.RenderWebInterviewUri(
                    interview.GetAssignmentId()!.Value,
                    interview.Id
                );
            }
            
            if (!this.HasCriticalFeature(interviewUid) 
                || CriticalityLevel == SharedKernels.DataCollection.ValueObjects.Interview.CriticalityLevel.Ignore)
            {
                IsCompletionAllowed = true;
                IsLoading = false;
            }
            else
                Task.Run(() => CollectCriticalityInfo(interviewUid, navigationState));
        }
        
        public override string Comment
        {
            get => base.Comment;
            set
            {
                base.Comment = value;
                IsCompletionAllowed = CalculateIsCompletionAllowed();
            }
        }
        
        protected override async Task CompleteInterviewAsync()
        {
            if (!this.IsCompletionAllowed)
                return;
            
            if (this.WasThisInterviewCompleted)
                return;

            if (HasCriticalIssues && !this.RequestWebInterview)
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
            var statefulInterview = this.interviewRepository.GetOrThrow(this.InterviewId.FormatGuid());

            if(switchInterviewToCawiMode)            
                auditLogService.Write(new SwitchInterviewModeAuditLogEntity(this.InterviewId, statefulInterview.GetInterviewKey().ToString(), InterviewMode.CAWI));
            else
                auditLogService.Write(new CompleteInterviewAuditLogEntity(this.InterviewId, statefulInterview.GetInterviewKey().ToString()));
            
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
        
        public override bool RequestWebInterview
        {
            get => base.RequestWebInterview;
            set
            {
                base.RequestWebInterview = value;
                IsCompletionAllowed = CalculateIsCompletionAllowed();
            }
        }

        protected override bool CalculateIsCompletionAllowed()
        {
            if (this.RequestWebInterview)
                return true;

            if (!this.HasCriticalFeature(InterviewId.FormatGuid()) 
                || CriticalityLevel == SharedKernels.DataCollection.ValueObjects.Interview.CriticalityLevel.Ignore)
                return true;
                
            if (HasCriticalIssues)
            {
                if (CriticalityLevel == SharedKernels.DataCollection.ValueObjects.Interview.CriticalityLevel.Warn)
                {
                    return !string.IsNullOrWhiteSpace(Comment);
                }
                else
                {
                    return false;
                }
            } 
                
            return true;
        }
    }
}
