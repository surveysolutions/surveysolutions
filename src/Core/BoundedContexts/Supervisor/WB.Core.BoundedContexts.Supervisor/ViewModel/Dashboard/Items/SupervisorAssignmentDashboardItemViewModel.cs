using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
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

        public SupervisorAssignmentDashboardItemViewModel(IServiceLocator serviceLocator, 
            IMapInteractionService mapInteractionService,
            IViewModelNavigationService navigationService) 
            : base(serviceLocator, mapInteractionService)
        {
            this.navigationService = navigationService;
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
            
            BindTargetAreaAction(Assignment.Id, Assignment.TargetArea);
        }

        private Task SelectInterviewerAsync() =>
            navigationService.NavigateToAsync<AssignAssignmentDialogViewModel, AssignAssignmentDialogArgs>(
                new AssignAssignmentDialogArgs(Assignment.Id));
    }
}
