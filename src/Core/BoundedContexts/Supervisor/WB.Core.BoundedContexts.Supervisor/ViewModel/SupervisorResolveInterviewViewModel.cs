using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class SupervisorResolveInterviewViewModel : CompleteInterviewViewModel
    {
        private readonly IAuditLogService auditLogService;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelNavigationService navigationService;
        
        public SupervisorResolveInterviewViewModel(
            ICommandService commandService, 
            IPrincipal principal, 
            IMvxMessenger messenger, 
            IStatefulInterviewRepository interviewRepository,
            IEntitiesListViewModelFactory entitiesListViewModelFactory, 
            ILastCompletionComments lastCompletionComments, 
            InterviewStateViewModel interviewState, 
            DynamicTextViewModel dynamicTextViewModel, 
            IViewModelNavigationService navigationService,
            ILogger logger,
            IAuditLogService auditLogService) : 
                base(navigationService,
                commandService,
                principal,
                messenger,
                entitiesListViewModelFactory,
                lastCompletionComments,
                interviewState,
                dynamicTextViewModel,
                logger)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.navigationService = navigationService;
            this.auditLogService = auditLogService;
        }

        private InterviewStatus status;
        private IStatefulInterview interview;

        public override void Configure(string interviewId, NavigationState navigationState)
        {
            base.Configure(interviewId, navigationState);

            this.Name.InitAsStatic(InterviewDetails.Resolve);

            this.CommentLabel = InterviewDetails.ResolveComment;

            interview = this.interviewRepository.Get(interviewId);
            this.status = interview.Status;

            var interviewKey = interview.GetInterviewKey()?.ToString();
            this.CompleteScreenTitle = string.IsNullOrEmpty(interviewKey)
                ? UIResources.Interview_Complete_Screen_Description
                : string.Format(UIResources.Interview_Complete_Screen_DescriptionWithInterviewKey, interviewKey);

            base.AnsweredCount = interview.CountActiveAnsweredQuestionsInInterviewForSupervisor();
            base.ErrorsCount = interview.CountInvalidEntitiesInInterviewForSupervisor();
            base.UnansweredCount = interview.CountActiveQuestionsInInterviewForSupervisor() - base.AnsweredCount;
        }

        public IMvxAsyncCommand Approve => new MvxAsyncCommand(async () =>
        {
            var command = new ApproveInterviewCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                Comment);
            await this.commandService.ExecuteAsync(command);
            auditLogService.Write(new ApproveInterviewAuditLogEntity(this.interviewId, interview.GetInterviewKey().ToString()));
            await this.navigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }, () => this.status == InterviewStatus.Completed || 
                 this.status == InterviewStatus.RejectedByHeadquarters);

        public IMvxAsyncCommand Reject => new MvxAsyncCommand(async () =>
        {
            var command = new RejectInterviewCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                Comment);
            await this.commandService.ExecuteAsync(command);
            auditLogService.Write(new RejectInterviewAuditLogEntity(this.interviewId, interview.GetInterviewKey().ToString()));
            await this.navigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }, () => this.status == InterviewStatus.Completed || 
                 this.status == InterviewStatus.RejectedByHeadquarters);

        public IMvxAsyncCommand Assign => new MvxAsyncCommand(SelectInterviewer, () => 
            this.status == InterviewStatus.RejectedBySupervisor || 
            this.status == InterviewStatus.SupervisorAssigned || 
            this.status == InterviewStatus.InterviewerAssigned);

        private Task SelectInterviewer() =>
            navigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
                    new SelectResponsibleForAssignmentArgs(this.interviewId));
    }
}
