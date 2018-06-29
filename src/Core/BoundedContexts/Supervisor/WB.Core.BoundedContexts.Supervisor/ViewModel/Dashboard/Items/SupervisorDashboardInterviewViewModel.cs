using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items
{
    public class SupervisorDashboardInterviewViewModel : InterviewDashboardItemViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;

        public SupervisorDashboardInterviewViewModel(IServiceLocator serviceLocator, IAuditLogService auditLogService,
            IViewModelNavigationService viewModelNavigationService) : base(serviceLocator, auditLogService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
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
    }
}
