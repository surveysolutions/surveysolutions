using System;
using MvvmCross.Plugin.Messenger;
using System.Threading.Tasks;
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

        public InterviewerCompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            ICommandService commandService,
            IPrincipal principal,
            IMvxMessenger messenger, 
            IStatefulInterviewRepository interviewRepository,
            InterviewStateViewModel interviewState,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            DynamicTextViewModel dynamicTextViewModel,
            ILastCompletionComments lastCompletionComments,
            IAuditLogService auditLogService,
            IInterviewerSettings interviewerSettings,
            ILogger logger,
            IPlainStorage<QuestionnaireView> interviewViewRepository)
            : base(viewModelNavigationService, commandService, principal, messenger, 
                entitiesListViewModelFactory, lastCompletionComments,interviewState, dynamicTextViewModel, logger)
        {
            this.interviewRepository = interviewRepository;
            this.auditLogService = auditLogService;
            this.interviewerSettings = interviewerSettings;
            this.interviewViewRepository = interviewViewRepository;
        }

        public override void Configure(string interviewUid, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewUid));
            base.Configure(interviewUid, navigationState);

            var interview = this.interviewRepository.GetOrThrow(interviewUid);
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

            if (string.IsNullOrEmpty(this.Comment))
            {
                this.Comment = interview.InterviewerCompleteComment;
            }
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
    }
}
