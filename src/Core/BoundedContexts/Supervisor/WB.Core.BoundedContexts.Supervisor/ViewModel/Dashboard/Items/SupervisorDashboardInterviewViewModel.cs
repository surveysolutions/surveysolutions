using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items
{
    public class SupervisorDashboardInterviewViewModel : InterviewDashboardItemViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        private readonly IPlainStorage<InterviewerDocument> interviewers;

        public SupervisorDashboardInterviewViewModel(IServiceLocator serviceLocator,
            IAuditLogService auditLogService,
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPlainStorage<InterviewerDocument> interviewers) : base(serviceLocator, auditLogService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.interviewers = interviewers;
        }

        protected override void BindActions()
        {
            Actions.Clear();

            BindLocationAction(interview.LocationQuestionId, interview.LocationLatitude, interview.LocationLongitude);

            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Primary,
                Command = new MvxAsyncCommand(this.LoadInterviewAsync, () => this.isInterviewReadyToLoad),
                Label = MainLabel()
            });

            string MainLabel()
            {
                return InterviewerUIResources.Dashboard_Open;
            }
        }

        public override async Task LoadInterviewAsync()
        {
            this.isInterviewReadyToLoad = false;
            try
            {
                Logger.Warn($"Open Interview {this.interview.InterviewId} (key: {this.interview.InterviewKey}, assignment: {this.interview.Assignment}) at {DateTime.Now}");
                await viewModelNavigationService.NavigateToAsync<LoadingViewModel, LoadingViewModelArg>(new LoadingViewModelArg{ InterviewId = this.interview.InterviewId });
            }
            finally
            {
                this.isInterviewReadyToLoad = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release managed resources here
            }

            base.Dispose(disposing);
        }

        protected override void BindTitles()
        {
            base.BindTitles();
            if (this.interview.ResponsibleId == this.principal.CurrentUserIdentity.UserId)
            {
                Responsible = this.principal.CurrentUserIdentity.Name;
            }
            else
            {
                var interviewer = this.interviewers.GetById(this.interview.ResponsibleId.FormatGuid());
                Responsible = string.Format(InterviewerUIResources.DashboardItem_Responsible, interviewer.UserName);
            }
        }
    }
}
