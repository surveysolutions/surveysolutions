using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

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

        private bool AllowSupervisorChangeAssignmentStatus =>
            serviceLocator.GetInstance<IEnumeratorSettings>().AllowSupervisorChangeAssignmentStatus;

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

            if (AllowSupervisorChangeAssignmentStatus)
            {
                // Status-change actions go in the context menu (⋮); "Assign" is the only primary button.
                if (Assignment.Status != AssignmentStatus.Closed)
                {
                    Actions.Add(new ActionDefinition
                    {
                        ActionType = ActionType.Context,
                        Command = new MvxAsyncCommand(this.CloseAssignmentAsync),
                        Label = EnumeratorUIResources.Dashboard_CloseAssignment
                    });
                }
                if (Assignment.Status != AssignmentStatus.Open)
                {
                    Actions.Add(new ActionDefinition
                    {
                        ActionType = ActionType.Context,
                        Command = new MvxAsyncCommand(this.ReopenAssignmentAsync),
                        Label = EnumeratorUIResources.Dashboard_Reopen
                    });
                }
            }
            
            BindTargetAreaAction(Assignment.Id, Assignment.TargetArea);
        }

        private Task SelectInterviewerAsync() =>
            navigationService.NavigateToAsync<AssignAssignmentDialogViewModel, AssignAssignmentDialogArgs>(
                new AssignAssignmentDialogArgs(Assignment.Id));

        private async Task CloseAssignmentAsync()
        {
            // Single dialog: shows warning message and optional comment field together
            await ChangeAssignmentStatusAsync(
                AssignmentStatus.Closed,
                EnumeratorUIResources.Dashboard_CloseAssignment_Message,
                EnumeratorUIResources.Dashboard_CloseAssignment_Title,
                EnumeratorUIResources.Dashboard_CloseAssignment);
        }

        private async Task ReopenAssignmentAsync()
        {
            await ChangeAssignmentStatusAsync(
                AssignmentStatus.Open,
                EnumeratorUIResources.Dashboard_ReopenAssignment_Message,
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
            serviceLocator.GetInstance<IMvxMessenger>().Publish(new DashboardChangedMessage(this));
        }
    }
}
