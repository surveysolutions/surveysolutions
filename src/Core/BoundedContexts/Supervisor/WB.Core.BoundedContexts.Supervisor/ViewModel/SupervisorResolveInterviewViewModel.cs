using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class SupervisorResolveInterviewViewModel : CompleteInterviewViewModel
    {
        private readonly IAuditLogService auditLogService;
        private readonly ICommandService commandService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IUserInteractionService userInteractionService;
        private readonly ICalendarEventStorage calendarEventStorage;

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
            IAuditLogService auditLogService,
            IPlainStorage<InterviewView> interviews,
            IUserInteractionService userInteractionService,
            ICalendarEventStorage calendarEventStorage) : 
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
            this.interviewRepository = interviewRepository;
            this.auditLogService = auditLogService;
            this.interviews = interviews;
            this.userInteractionService = userInteractionService;
            this.calendarEventStorage = calendarEventStorage;
        }

        private InterviewStatus status;
        private IStatefulInterview interview;
        private DateTime? receivedByInterviewerTabletAt;

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

            var interviewView = this.interviews.GetById(interviewId);
            this.receivedByInterviewerTabletAt = interviewView.ReceivedByInterviewerAtUtc;
        }

        public IMvxAsyncCommand Approve => new MvxAsyncCommand(async () =>
        {
            try
            {
                if (this.interview.Status != InterviewStatus.ApprovedBySupervisor)
                {
                    if (receivedByInterviewerTabletAt != null)
                    {
                        var approveConfirmed = await userInteractionService.ConfirmAsync(
                            SupervisorUIResources.Confirm_Approve_Synchronized_Interview_Message,
                            okButton: UIResources.Yes,
                            cancelButton: UIResources.No);

                        if (!approveConfirmed)
                        {
                            return;
                        }
                    }

                    var command = new ApproveInterviewCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                        Comment);
                    await this.commandService.ExecuteAsync(command);
                    auditLogService.Write(new ApproveInterviewAuditLogEntity(this.interviewId, interview.GetInterviewKey().ToString()));

                    CompleteCalendarEventIfExists();
                }
            }
            catch (InterviewException e)
            {
                logger.Warn($"Error on Interview Approve. Interview: {interviewId}", e);
            }

            await viewModelNavigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }, () => this.status == InterviewStatus.Completed || 
                 this.status == InterviewStatus.RejectedByHeadquarters ||
                 this.status == InterviewStatus.RejectedBySupervisor);

        public IMvxAsyncCommand Reject => new MvxAsyncCommand(async () =>
        {
            try
            {
                if (this.interview.Status != InterviewStatus.RejectedBySupervisor)
                {
                    var command = new RejectInterviewCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                        Comment);
                    await this.commandService.ExecuteAsync(command);
                    auditLogService.Write(new RejectInterviewAuditLogEntity(this.interviewId,
                        interview.GetInterviewKey().ToString()));

                    CompleteCalendarEventIfExists();
                }
            }
            catch (InterviewException e)
            {
                logger.Warn($"Error on Interview Reject. Interview: {interviewId}", e);
            }

            await viewModelNavigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }, () => this.status == InterviewStatus.Completed || 
                 this.status == InterviewStatus.RejectedByHeadquarters);

        private void CompleteCalendarEventIfExists()
        {
            var calendarEvent = calendarEventStorage.GetCalendarEventForInterview(interviewId);
            if (calendarEvent == null)
                return;

            var command = new CompleteCalendarEventCommand(calendarEvent.Id, this.principal.CurrentUserIdentity.UserId, 
                new QuestionnaireIdentity() //dummy
                );
            this.commandService.Execute(command);

            logger.Info($"Calendar event {calendarEvent.Id} completed after approve/reject interview {interview.GetInterviewKey()?.ToString()} ({interviewId})");
        }

        public IMvxAsyncCommand Assign => new MvxAsyncCommand(SelectInterviewer, () => 
            this.status == InterviewStatus.RejectedBySupervisor || 
            this.status == InterviewStatus.SupervisorAssigned || 
            this.status == InterviewStatus.InterviewerAssigned);

        private Task SelectInterviewer() =>
            viewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
                    new SelectResponsibleForAssignmentArgs(this.interviewId));
    }
}
