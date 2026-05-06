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

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment.LocationLatitude, Assignment.LocationLongitude);

            switch (Assignment.Status)
            {
                case AssignmentStatus.Active:
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

                    Actions.Add(new ActionDefinition
                    {
                        ActionType = ActionType.Context,
                        Command = new MvxAsyncCommand(this.FinishAssignmentAsync),
                        Label = EnumeratorUIResources.Dashboard_FinishAssignment
                    });
                    break;

                case AssignmentStatus.Finished:
                    Actions.Add(new ActionDefinition
                    {
                        ActionType = ActionType.Primary,
                        Command = new MvxAsyncCommand(this.ReopenAssignmentAsync),
                        Label = EnumeratorUIResources.Dashboard_Reopen
                    });
                    break;

                case AssignmentStatus.Completed:
                    // No actions available for completed assignments
                    break;
            }

            BindTargetAreaAction(Assignment.Id, Assignment.TargetArea);
        }

        private async Task FinishAssignmentAsync()
        {
            // Single dialog: shows warning message and optional comment field together
            var comment = await userInteractionService.ConfirmWithTextInputAsync(
                EnumeratorUIResources.Dashboard_FinishAssignment_Message,
                title: EnumeratorUIResources.Dashboard_FinishAssignment_Title,
                okButton: EnumeratorUIResources.Dashboard_FinishAssignment,
                cancelButton: UIResources.Cancel);

            if (comment == null) // user cancelled
                return;

            Assignment.Status = AssignmentStatus.Finished;
            // Use empty string (not null) to signal a pending upload with no comment
            Assignment.StatusComment = comment.Trim().Length > 0 ? comment.Trim() : string.Empty;
            AssignmentsRepository.Store(Assignment);

            RaiseOnItemUpdated();
        }

        private async Task ReopenAssignmentAsync()
        {
            var comment = await userInteractionService.ConfirmWithTextInputAsync(
                string.Empty,
                title: EnumeratorUIResources.Dashboard_ReopenAssignment_Title,
                okButton: EnumeratorUIResources.Dashboard_Reopen,
                cancelButton: UIResources.Cancel);

            if (comment == null) // user cancelled
                return;

            Assignment.Status = AssignmentStatus.Active;
            // Use empty string (not null) to signal a pending upload with no comment
            Assignment.StatusComment = comment.Trim().Length > 0 ? comment.Trim() : string.Empty;
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
