using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
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
using WB.Core.SharedKernels.Enumerator.Utils;
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
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IUserInteractionService userInteractionService;
        private readonly ICalendarEventStorage calendarEventStorage;

        public SupervisorResolveInterviewViewModel(
            ICommandService commandService, 
            IPrincipal principal, 
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
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
                entitiesListViewModelFactory,
                lastCompletionComments,
                interviewState,
                dynamicTextViewModel,
                interviewRepository,
                questionnaireRepository,
                logger)
        {
            this.commandService = commandService;
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
            RunConfiguration(interviewId, navigationState, true);

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
            
            this.TopFailedCriticalRules = this.entitiesListViewModelFactory.GetTopFailedCriticalRulesFromState(interviewId, navigationState).ToList();
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
            
            IsLoading = false;
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

                    var command = new ApproveInterviewCommand(InterviewId, this.principal.CurrentUserIdentity.UserId,
                        Comment);
                    await this.commandService.ExecuteAsync(command);
                    auditLogService.Write(new ApproveInterviewAuditLogEntity(this.InterviewId, interview.GetInterviewKey().ToString()));

                    CompleteCalendarEventIfExists();
                }
            }
            catch (InterviewException e)
            {
                Logger.Warn($"Error on Interview Approve. Interview: {InterviewId}", e);
            }

            await viewModelNavigationService.NavigateFromInterviewAsync(InterviewId.FormatGuid());
        }, () => this.status == InterviewStatus.Completed || 
                 this.status == InterviewStatus.RejectedByHeadquarters ||
                 this.status == InterviewStatus.RejectedBySupervisor);

        public IMvxAsyncCommand Reject => new MvxAsyncCommand(async () =>
        {
            try
            {
                if (this.interview.Status != InterviewStatus.RejectedBySupervisor)
                {
                    var command = new RejectInterviewCommand(InterviewId, this.principal.CurrentUserIdentity.UserId,
                        Comment);
                    await this.commandService.ExecuteAsync(command);
                    auditLogService.Write(new RejectInterviewAuditLogEntity(this.InterviewId,
                        interview.GetInterviewKey().ToString()));

                    CompleteCalendarEventIfExists();
                }
            }
            catch (InterviewException e)
            {
                Logger.Warn($"Error on Interview Reject. Interview: {InterviewId}", e);
            }

            await viewModelNavigationService.NavigateFromInterviewAsync(InterviewId.FormatGuid());
        }, () => this.status == InterviewStatus.Completed || 
                 this.status == InterviewStatus.RejectedByHeadquarters);

        private void CompleteCalendarEventIfExists()
        {
            var calendarEvent = calendarEventStorage.GetCalendarEventForInterview(InterviewId);
            if (calendarEvent == null)
                return;

            var command = new CompleteCalendarEventCommand(calendarEvent.Id, this.principal.CurrentUserIdentity.UserId, 
                new QuestionnaireIdentity() //dummy
                );
            this.commandService.Execute(command);

            Logger.Info($"Calendar event {calendarEvent.Id} completed after approve/reject interview {interview.GetInterviewKey()?.ToString()} ({InterviewId})");
        }

        public IMvxAsyncCommand Assign => new MvxAsyncCommand(SelectInterviewer, () => 
            this.status == InterviewStatus.RejectedBySupervisor || 
            this.status == InterviewStatus.SupervisorAssigned || 
            this.status == InterviewStatus.InterviewerAssigned);

        private Task SelectInterviewer() =>
            viewModelNavigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
                    new SelectResponsibleForAssignmentArgs(this.InterviewId));
        
        public override void Dispose()
        {
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
