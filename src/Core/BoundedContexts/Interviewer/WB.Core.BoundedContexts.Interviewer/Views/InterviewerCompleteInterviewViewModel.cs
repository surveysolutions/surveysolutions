using System;
using MvvmCross.Plugin.Messenger;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerCompleteInterviewViewModel : CompleteInterviewViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAuditLogService auditLogService;

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
            ILogger logger)
            : base(viewModelNavigationService, commandService, principal, messenger, 
                entitiesListViewModelFactory, lastCompletionComments,interviewState, dynamicTextViewModel, logger)
        {
            this.interviewRepository = interviewRepository;
            this.auditLogService = auditLogService;
        }

        public override void Configure(string interviewId, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            base.Configure(interviewId, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.CompleteScreenTitle = string.IsNullOrEmpty(interviewKey)
                ? UIResources.Interview_Complete_Screen_Description
                : string.Format(UIResources.Interview_Complete_Screen_DescriptionWithInterviewKey, interviewKey);


            var statefulInterview = this.interviewRepository.Get(interviewId);
            if (string.IsNullOrEmpty(this.CompleteComment))
            {
                this.CompleteComment = statefulInterview.InterviewerCompleteComment;
            }
        }

        protected override Task CloseInterviewAfterComplete()
        {
            var statefulInterview = this.interviewRepository.Get(this.interviewId.FormatGuid());
            auditLogService.Write(new CompleteInterviewAuditLogEntity(this.interviewId, statefulInterview.GetInterviewKey().ToString()));
            return base.CloseInterviewAfterComplete();
        }
    }
}
