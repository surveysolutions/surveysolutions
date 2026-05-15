using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Views.CreateInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class InterviewerAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel, IDashboardViewItem
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;

        public InterviewerAssignmentDashboardItemViewModel(IServiceLocator serviceLocator,
            IViewModelNavigationService viewModelNavigationService,
            IMapInteractionService mapInteractionService,
            IUserInteractionService userInteractionService) 
            : base(serviceLocator, mapInteractionService, userInteractionService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
        }

        private bool AllowInterviewerChangeAssignmentStatus =>
            serviceLocator.GetInstance<IEnumeratorSettings>().AllowInterviewerChangeAssignmentStatus;

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment.LocationLatitude, Assignment.LocationLongitude);

            switch (Assignment.Status)
            {
                case AssignmentStatus.Open:
                    Actions.Add(new ActionDefinition
                    {
                        Command = new MvxAsyncCommand(
                            async () => { await CreateInterviewAsync(); },
                            () => !Assignment.Quantity.HasValue ||
                                  Math.Max(val1: 0, val2: InterviewsLeftByAssignmentCount) > 0),
                        Label = EnumeratorUIResources.Dashboard_StartNewInterview
                    });

                    Actions.Add(new ActionDefinition
                    {
                        ActionType = ActionType.Context,
                        Command = new MvxAsyncCommand(this.SetCalendarEventAsync),
                        Label = Assignment.CalendarEvent.HasValue 
                            ? EnumeratorUIResources.Dashboard_EditCalendarEvent
                            : EnumeratorUIResources.Dashboard_AddCalendarEvent
                    });

                    Actions.Add(new ActionDefinition
                    {
                        ActionType = ActionType.Context,
                        Command = new MvxCommand(this.RemoveCalendarEvent, () => Assignment.CalendarEvent.HasValue),
                        Label = EnumeratorUIResources.Dashboard_RemoveCalendarEvent
                    });

                    if (AllowInterviewerChangeAssignmentStatus)
                    {
                        Actions.Add(new ActionDefinition
                        {
                            ActionType = ActionType.Context,
                            Command = new MvxAsyncCommand(this.CompleteAssignmentAsync),
                            Label = EnumeratorUIResources.Dashboard_CompleteAssignment
                        });
                    }
                    break;

                case AssignmentStatus.Completed:
                    if (AllowInterviewerChangeAssignmentStatus)
                    {
                        Actions.Add(new ActionDefinition
                        {
                            ActionType = ActionType.Primary,
                            Command = new MvxAsyncCommand(this.ReopenAssignmentAsync),
                            Label = EnumeratorUIResources.Dashboard_Reopen
                        });
                    }
                    break;

                case AssignmentStatus.Approved:
                    // No actions available for approved assignments
                    break;
            }

            BindTargetAreaAction(Assignment.Id, Assignment.TargetArea);
        }

        private async Task CompleteAssignmentAsync()
        {
            // Single dialog: shows warning message and optional comment field together
            await ChangeAssignmentStatusAsync(
                AssignmentStatus.Completed,
                EnumeratorUIResources.Dashboard_CompleteAssignment_Message,
                EnumeratorUIResources.Dashboard_CompleteAssignment_Title,
                EnumeratorUIResources.Dashboard_CompleteAssignment);
        }

        private async Task ReopenAssignmentAsync()
        {
            await ChangeAssignmentStatusAsync(
                AssignmentStatus.Open,
                string.Empty,
                EnumeratorUIResources.Dashboard_ReopenAssignment_Title,
                EnumeratorUIResources.Dashboard_Reopen);
        }

        private async Task ChangeAssignmentStatusAsync(AssignmentStatus newStatus, string message, string title, string okButton)
        {
            var comment = await userInteractionService.AssignmentStatusChangeAsync(
                message,
                title: title,
                okButton: okButton,
                cancelButton: UIResources.Cancel,
                commentHint: EnumeratorUIResources.Dashboard_Assignment_Comment);

            if (comment == null) // user cancelled
                return;

            Assignment.Status = newStatus;
            var trimmedComment = comment.Trim();
            Assignment.StatusComment = trimmedComment.Length > 0 ? trimmedComment : null;
            // Track the timestamp of the local change — used as pending-upload indicator
            Assignment.StatusChangedAtUtc = DateTime.UtcNow;
            AssignmentsRepository.Store(Assignment);

            RaiseOnItemUpdated();
        }

        private async Task CreateInterviewAsync()
        {
            if (!string.IsNullOrWhiteSpace(Assignment.TargetArea))
            {
                var confirmResult = await userInteractionService.ConfirmAsync(
                    EnumeratorUIResources.Dashboard_CreateInterview_TargetArea_Warning);
                if (!confirmResult)
                    return;
            }
            
            await viewModelNavigationService
                .NavigateToAsync<CreateAndLoadInterviewViewModel, CreateInterviewViewModelArg>(
                    new CreateInterviewViewModelArg()
                    {
                        AssignmentId = Assignment.Id,
                        InterviewId = Guid.NewGuid()
                    }, true);
        }
    }
}
