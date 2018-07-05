using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items
{
    public class SupervisorAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel
    {
        private IInterviewerSelectorDialog interviewerSelectorDialog = null;
        private IInterviewerSelectorDialog InterviewerSelectorDialog
            => interviewerSelectorDialog ?? (interviewerSelectorDialog = serviceLocator.GetInstance<IInterviewerSelectorDialog>());

        public event EventHandler<InterviewerChangedArgs> ResponsibleChanged;

        public SupervisorAssignmentDashboardItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
        }

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment?.LocationLatitude, Assignment?.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Primary,
                Command = new MvxAsyncCommand(this.SelectInterviewerAsync, () => true),
                Label = InterviewerUIResources.Dashboard_Assign
            });
        }

        private async Task SelectInterviewerAsync()
        {
            this.InterviewerSelectorDialog.Selected += OnInterviewerSelected;
            this.InterviewerSelectorDialog.Cancelled += OnSelectionCancelled;
            this.InterviewerSelectorDialog.SelectInterviewer(this.Assignment);
        }

        private void OnSelectionCancelled(object sender, EventArgs e)
        {
            this.UnsubscribeDialog();
        }

        private void OnInterviewerSelected(object sender, InterviewerSelectedArgs e)
        {
            this.UnsubscribeDialog();

            var interviewerChangedArgs = new InterviewerChangedArgs(Assignment.ResponsibleId, UserRoles.Supervisor, e.InterviewerId, UserRoles.Interviewer);

            Assignment.ResponsibleId = e.InterviewerId;
            AssignmentsRepository.Store(Assignment);

            this.ResponsibleChanged?.Invoke(this, interviewerChangedArgs);
        }

        private void UnsubscribeDialog()
        {
            this.InterviewerSelectorDialog.Selected -= OnInterviewerSelected;
            this.InterviewerSelectorDialog.Cancelled -= OnSelectionCancelled;
        }
    }
}
