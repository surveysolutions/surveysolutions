using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items
{
    public class SupervisorAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel
    {
        private readonly IViewModelNavigationService navigationService;
        private readonly IUserInteractionService userInteractionService;

        public SupervisorAssignmentDashboardItemViewModel(IServiceLocator serviceLocator, 
            IMapInteractionService mapInteractionService,
            IViewModelNavigationService navigationService,
            IUserInteractionService userInteractionService) 
            : base(serviceLocator, mapInteractionService, userInteractionService)
        {
            this.navigationService = navigationService;
            this.userInteractionService = userInteractionService;
        }

        protected override void BindTitles()
        {
            base.BindTitles();
            Responsible =  string.Format(EnumeratorUIResources.DashboardItem_Responsible,  Assignment.ResponsibleName);
        }

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment?.LocationLatitude, Assignment?.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Primary,
                Command = new MvxAsyncCommand(this.SelectInterviewerAsync),
                Label = EnumeratorUIResources.Dashboard_Assign
            });

            // Active: supervisor can Complete via context menu
            if (Assignment.Status == AssignmentStatus.Active)
            {
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Context,
                    Command = new MvxAsyncCommand(this.CompleteAssignmentAsync),
                    Label = EnumeratorUIResources.Dashboard_CompleteAssignment
                });
            }

            // Finished: supervisor can Complete or Reopen — both visible on card as primary actions
            if (Assignment.Status == AssignmentStatus.Finished)
            {
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Primary,
                    Command = new MvxAsyncCommand(this.CompleteAssignmentAsync),
                    Label = EnumeratorUIResources.Dashboard_CompleteAssignment
                });
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Primary,
                    Command = new MvxAsyncCommand(this.ReopenAssignmentAsync),
                    Label = EnumeratorUIResources.Dashboard_Reopen
                });
            }

            // Completed: supervisor can Reopen via context menu
            if (Assignment.Status == AssignmentStatus.Completed)
            {
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Context,
                    Command = new MvxAsyncCommand(this.ReopenAssignmentAsync),
                    Label = EnumeratorUIResources.Dashboard_Reopen
                });
            }
            
            BindTargetAreaAction(Assignment.Id, Assignment.TargetArea);
        }

        private Task SelectInterviewerAsync() =>
            navigationService.NavigateToAsync<AssignAssignmentDialogViewModel, AssignAssignmentDialogArgs>(
                new AssignAssignmentDialogArgs(Assignment.Id));

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
                AssignmentStatus.Active,
                string.Empty,
                EnumeratorUIResources.Dashboard_ReopenAssignment_Title,
                EnumeratorUIResources.Dashboard_Reopen);
        }

        private async Task ChangeAssignmentStatusAsync(AssignmentStatus newStatus, string message, string title, string okButton)
        {
            var comment = await userInteractionService.ConfirmWithTextInputAsync(
                message,
                title: title,
                okButton: okButton,
                cancelButton: UIResources.Cancel);

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
    }
}
