using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items
{
    public class SupervisorAssignmentDashboardItemViewModel : AssignmentDashboardItemViewModel
    {
        private IInterviewerSelectorDialog interviewerSelectorDialog;
        private readonly IAuditLogService auditLogService;

        public SupervisorAssignmentDashboardItemViewModel(IServiceLocator serviceLocator, 
            IInterviewerSelectorDialog interviewerSelectorDialog,
            IAuditLogService auditLogService) 
            : base(serviceLocator)
        {
            this.interviewerSelectorDialog = interviewerSelectorDialog;
            this.auditLogService = auditLogService;
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
                Command = new MvxCommand(this.SelectInterviewer),
                Label = InterviewerUIResources.Dashboard_Assign
            });
        }

        private void SelectInterviewer()
        {
            this.interviewerSelectorDialog.Selected += OnInterviewerSelected;
            this.interviewerSelectorDialog.Cancelled += OnSelectionCancelled;
            this.interviewerSelectorDialog.SelectInterviewer(string.Format("Select responsible for assignment #{0}", this.Assignment.Id));
        }

        private void OnSelectionCancelled(object sender, EventArgs e)
        {
            this.UnsubscribeDialog();
        }

        private void OnInterviewerSelected(object sender, InterviewerSelectedArgs e)
        {
            this.UnsubscribeDialog();

            if (Assignment.ResponsibleId != e.InterviewerId)
                Assignment.ReceivedByInterviewerAt = null;
            Assignment.ResponsibleId = e.InterviewerId;
            Assignment.ResponsibleName = e.Login;

            AssignmentsRepository.Store(Assignment);

            auditLogService.Write(new AssignResponsibleToAssignmentAuditLogEntity(Assignment.Id, e.InterviewerId, e.Login));

            BindTitles();

            serviceLocator.GetInstance<IMvxMessenger>().Publish(new DashboardChangedMsg(this));
        }

        private void UnsubscribeDialog()
        {
            if (this.interviewerSelectorDialog==null)
                return;

            this.interviewerSelectorDialog.Selected -= OnInterviewerSelected;
            this.interviewerSelectorDialog.Cancelled -= OnSelectionCancelled;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeDialog();
                this.interviewerSelectorDialog.DisposeIfDisposable();
                this.interviewerSelectorDialog = null;
            }

            base.Dispose(disposing);
        }
    }
}
