using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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
        private readonly IUserInteractionService userInteractionService;

        public SupervisorDashboardInterviewViewModel(IServiceLocator serviceLocator,
            IAuditLogService auditLogService,
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPlainStorage<InterviewerDocument> interviewers,
            IUserInteractionService userInteractionService) : base(serviceLocator, auditLogService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.interviewers = interviewers;
            this.userInteractionService = userInteractionService;
        }

        protected override void BindActions()
        {
            Actions.Clear();

            BindLocationAction(interview.LocationQuestionId, interview.LocationLatitude, interview.LocationLongitude);

            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Primary,
                Command = new MvxAsyncCommand(this.LoadInterviewAsync, () => this.isInterviewReadyToLoad),
                Label = EnumeratorUIResources.Dashboard_Open
            });
        }

        public override async Task LoadInterviewAsync()
        {
            if (interview.Mode == InterviewMode.CAWI)
            {
                await userInteractionService.AlertAsync(EnumeratorUIResources.Dashboard_CantOpenCawi);
                return;
            }
            
            this.isInterviewReadyToLoad = false;
            try
            {
                Logger.Warn($"Open Interview {this.interview.InterviewId} (key: {this.interview.InterviewKey}, assignment: {this.interview.Assignment}) at {DateTime.Now}");
                await viewModelNavigationService.NavigateToAsync<LoadingInterviewViewModel, LoadingViewModelArg>(
                    new LoadingViewModelArg
                    {
                        InterviewId = this.interview.InterviewId
                    }, true);
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

            if (interview.Mode == InterviewMode.CAWI)
            {
                IdLabel = string.Format(EnumeratorUIResources.Dashboard_CawiLabel, IdLabel);
            }
            
            if (this.interview.ResponsibleId == this.principal.CurrentUserIdentity.UserId)
            {
                Responsible = this.principal.CurrentUserIdentity.Name;
            }
            else
            {
                var interviewer = this.interviewers.GetById(this.interview.ResponsibleId.FormatGuid());
                if (interviewer != null)
                {
                    Responsible = string.Format(EnumeratorUIResources.DashboardItem_Responsible, interviewer.UserName);
                }
            }
        }
    }
}
