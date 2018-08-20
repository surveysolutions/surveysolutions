using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items
{
    public class SupervisorAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel
    {
        private readonly IAuditLogService auditLogService;
        private readonly IViewModelNavigationService navigationService;

        public SupervisorAssignmentDashboardItemViewModel(IServiceLocator serviceLocator, 
            IAuditLogService auditLogService,
            IViewModelNavigationService navigationService) 
            : base(serviceLocator)
        {
            this.auditLogService = auditLogService;
            this.navigationService = navigationService;
        }

        protected override void BindTitles()
        {
            base.BindTitles();
            Responsible =  string.Format(InterviewerUIResources.DashboardItem_Responsible,  Assignment.ResponsibleName);
        }

        protected override void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(Assignment.LocationQuestionId, Assignment?.LocationLatitude, Assignment?.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Primary,
                Command = new MvxAsyncCommand(this.SelectInterviewerAsync),
                Label = InterviewerUIResources.Dashboard_Assign
            });
        }

        private Task SelectInterviewerAsync() =>
            this.navigationService.NavigateToAsync<SelectResponsibleForAssignmentViewModel, SelectResponsibleForAssignmentArgs>(
                    new SelectResponsibleForAssignmentArgs(Assignment.Id));
    }
}
