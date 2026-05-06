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

            // Active: supervisor can Complete
            if (Assignment.Status == AssignmentStatus.Active)
            {
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Context,
                    Command = new MvxAsyncCommand(this.CompleteAssignmentAsync),
                    Label = EnumeratorUIResources.Dashboard_CompleteAssignment
                });
            }

            // Finished: supervisor can Complete or Reopen
            if (Assignment.Status == AssignmentStatus.Finished)
            {
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Context,
                    Command = new MvxAsyncCommand(this.CompleteAssignmentAsync),
                    Label = EnumeratorUIResources.Dashboard_CompleteAssignment
                });
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Context,
                    Command = new MvxAsyncCommand(this.ReopenAssignmentAsync),
                    Label = EnumeratorUIResources.Dashboard_Reopen
                });
            }

            // Completed: supervisor can only Reopen
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
            var confirmed = await userInteractionService.ConfirmAsync(
                EnumeratorUIResources.Dashboard_CompleteAssignment_Message,
                title: EnumeratorUIResources.Dashboard_CompleteAssignment_Title,
                okButton: EnumeratorUIResources.Dashboard_CompleteAssignment,
                cancelButton: UIResources.Cancel);

            if (!confirmed)
                return;

            var comment = await userInteractionService.ConfirmWithTextInputAsync(
                EnumeratorUIResources.Dashboard_Assignment_Comment,
                title: EnumeratorUIResources.Dashboard_CompleteAssignment_Title,
                okButton: EnumeratorUIResources.Dashboard_CompleteAssignment,
                cancelButton: UIResources.Cancel);

            if (comment == null) // user cancelled
                return;

            Assignment.Status = AssignmentStatus.Completed;
            Assignment.StatusComment = string.IsNullOrWhiteSpace(comment) ? null : comment;
            AssignmentsRepository.Store(Assignment);

            RaiseOnItemUpdated();
        }

        private async Task ReopenAssignmentAsync()
        {
            var comment = await userInteractionService.ConfirmWithTextInputAsync(
                EnumeratorUIResources.Dashboard_Assignment_Comment,
                title: EnumeratorUIResources.Dashboard_ReopenAssignment_Title,
                okButton: EnumeratorUIResources.Dashboard_Reopen,
                cancelButton: UIResources.Cancel);

            if (comment == null) // user cancelled
                return;

            Assignment.Status = AssignmentStatus.Active;
            Assignment.StatusComment = string.IsNullOrWhiteSpace(comment) ? null : comment;
            AssignmentsRepository.Store(Assignment);

            RaiseOnItemUpdated();
        }
    }
}
