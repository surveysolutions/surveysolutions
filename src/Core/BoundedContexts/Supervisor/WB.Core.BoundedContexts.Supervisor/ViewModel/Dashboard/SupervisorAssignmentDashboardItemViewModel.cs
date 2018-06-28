using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class SupervisorAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel
    {
        private IInterviewerSelectorDialog interviewerSelectorDialog
            => serviceLocator.GetInstance<IInterviewerSelectorDialog>();

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
            this.interviewerSelectorDialog.SelectInterviewer(this.Assignment);
        }
    }
}
