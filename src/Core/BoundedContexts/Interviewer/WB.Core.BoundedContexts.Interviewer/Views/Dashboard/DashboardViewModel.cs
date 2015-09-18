using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;


namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel, 
        ILiteEventHandler<InterviewDeleted>
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerDashboardFactory dashboardFactory;
        private readonly IPrincipal principal;
        private readonly ILiteEventRegistry liteEventRegistry;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IInterviewerDashboardFactory dashboardFactory,
            IPrincipal principal, 
            SynchronizationViewModel synchronization,
            ILiteEventRegistry liteEventRegistry)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.dashboardFactory = dashboardFactory;
            this.principal = principal;
            this.liteEventRegistry = liteEventRegistry;
            this.Synchronization = synchronization;
        }

        public async void Init()
        {
            if (!this.principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateTo<LoginViewModel>();
                return;
            }

            liteEventRegistry.SubscribeOnAllAggregateRoots(this);

            await this.RefreshDashboardAsync();
        }

        private async Task RefreshDashboardAsync()
        {
            this.DashboardInformation = await this.dashboardFactory.GetDashboardItems(
                this.principal.CurrentUserIdentity.UserId,
                this.currentDashboardStatus);

            if ((CurrentDashboardStatus == DashboardInterviewStatus.Completed && this.CompletedInterviewsCount == 0)
                || (CurrentDashboardStatus == DashboardInterviewStatus.InProgress && this.StartedInterviewsCount == 0))
            {
                this.CurrentDashboardStatus = DashboardInterviewStatus.New;
            }


            this.RefreshTab();

            IsLoaded = true;
        }

        private void RefreshTab()
        {
            switch (this.CurrentDashboardStatus)
            {
                 case DashboardInterviewStatus.New:
                    this.DashboardItems = dashboardInformation.CensusQuestionniories.Union(dashboardInformation.NewInterviews);
                    break;
                 case DashboardInterviewStatus.InProgress:
                    this.DashboardItems = dashboardInformation.StartedInterviews;
                    break;
                 case DashboardInterviewStatus.Completed:
                    this.DashboardItems = dashboardInformation.CompletedInterviews;
                    break;
                 case DashboardInterviewStatus.Rejected:
                    this.DashboardItems = dashboardInformation.RejectedInterviews;
                    break;
            }
        }
        
        private SynchronizationViewModel synchronization;
        public SynchronizationViewModel Synchronization
        {
            get { return synchronization; }
            set
            {
                this.synchronization = value;
                this.RaisePropertyChanged();
            }
        }

        private DashboardInformation dashboardInformation = new DashboardInformation();
        private DashboardInterviewStatus currentDashboardStatus;
        public bool IsNewInterviewsCategorySelected { get { return this.CurrentDashboardStatus == DashboardInterviewStatus.New; } }
        public bool IsStartedInterviewsCategorySelected { get { return this.CurrentDashboardStatus == DashboardInterviewStatus.InProgress; } }
        public bool IsCompletedInterviewsCategorySelected { get { return this.CurrentDashboardStatus == DashboardInterviewStatus.Completed; } }
        public bool IsRejectedInterviewsCategorySelected { get { return this.CurrentDashboardStatus == DashboardInterviewStatus.Rejected; } }

        private bool isLoaded;
        public bool IsLoaded
        {
            get { return this.isLoaded; }
            set { this.isLoaded = value; this.RaisePropertyChanged(); }
        }

        public DashboardInterviewStatus CurrentDashboardStatus
        {
            get { return this.currentDashboardStatus; }
            set 
            {
                this.currentDashboardStatus = value; 
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => IsNewInterviewsCategorySelected);
                this.RaisePropertyChanged(() => IsStartedInterviewsCategorySelected);
                this.RaisePropertyChanged(() => IsCompletedInterviewsCategorySelected);
                this.RaisePropertyChanged(() => IsRejectedInterviewsCategorySelected); 
            }
        }

        public DashboardInformation DashboardInformation
        {
            get { return this.dashboardInformation; }
            set
            {
                this.dashboardInformation = value;
                this.RaisePropertyChanged(() => NewInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyNewInterview);
                this.RaisePropertyChanged(() => StartedInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyStartedInterview);
                this.RaisePropertyChanged(() => CompletedInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyCompletedInterview);
                this.RaisePropertyChanged(() => RejectedInterviewsCount);
                this.RaisePropertyChanged(() => IsExistsAnyRejectedInterview);
                this.RaisePropertyChanged(() => DashboardTitle);
            }
        }
        private IMvxCommand synchronizationCommand;
        public IMvxCommand SynchronizationCommand
        {
            get
            {
                return synchronizationCommand ??
                       (synchronizationCommand = new MvxCommand(async () => await this.RunSynchronizationAsync(),
                           () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }

        private async Task RunSynchronizationAsync()
        {
            await this.Synchronization.SynchronizeAsync();
            await this.RefreshDashboardAsync();
        }

        public string DashboardTitle
        {
            get
            {
                var numberOfAssignedInterviews = this.NewInterviewsCount 
                    + this.StartedInterviewsCount
                    + this.CompletedInterviewsCount 
                    + this.RejectedInterviewsCount;

                var userName = this.principal.CurrentUserIdentity.Name;
                return InterviewerUIResources.Dashboard_Title.FormatString(numberOfAssignedInterviews, userName);
            }
        }

        public int NewInterviewsCount { get { return this.dashboardInformation.NewInterviews.Count; } }
        public int StartedInterviewsCount { get { return this.dashboardInformation.StartedInterviews.Count; } }
        public int CompletedInterviewsCount { get { return this.dashboardInformation.CompletedInterviews.Count; } }
        public int RejectedInterviewsCount { get { return this.dashboardInformation.RejectedInterviews.Count; } }

        public bool IsExistsAnyCensusQuestionniories { get { return this.dashboardInformation.CensusQuestionniories.Count > 0; } }
        public bool IsExistsAnyNewInterview { get { return this.dashboardInformation.CensusQuestionniories.Count > 0 && this.NewInterviewsCount > 0; } }
        public bool IsExistsAnyStartedInterview { get { return this.StartedInterviewsCount > 0; } }
        public bool IsExistsAnyCompletedInterview { get { return this.CompletedInterviewsCount > 0; } }
        public bool IsExistsAnyRejectedInterview { get { return this.RejectedInterviewsCount > 0; } }

        private IEnumerable<IDashboardItem> dashboardItems;
        public IEnumerable<IDashboardItem> DashboardItems
        {
            get { return this.dashboardItems; }
            set { this.dashboardItems = value;  this.RaisePropertyChanged(); }
        }

        public IMvxCommand ShowNewItemsInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewStatus.New)); }
        }

        public IMvxCommand ShowStartedInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewStatus.InProgress)); }
        }

        public IMvxCommand ShowCompletedInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewStatus.Completed)); }
        }

        public IMvxCommand ShowRejectedInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewStatus.Rejected)); }
        }

        private void ShowInterviewsCommand(DashboardInterviewStatus status)
        {
            if (status == this.CurrentDashboardStatus)
                return;

            this.CurrentDashboardStatus = status;
            this.RefreshTab();
        }

        public IMvxCommand SignOutCommand
        {
            get { return new MvxCommand(this.SignOut); }
        }

        void SignOut()
        {
            this.principal.SignOut();
            this.viewModelNavigationService.NavigateTo<LoginViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }

        public async void Handle(InterviewDeleted @event)
        {
            await this.RefreshDashboardAsync();
        }
    }
}