using System.Threading;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        public readonly SynchronizationViewModel Synchronization;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal, SynchronizationViewModel synchronization)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.Synchronization = synchronization;
        }

        public void Init()
        {
            if (!this.principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
                return;
            }

            LoggedInUserName = this.principal.CurrentUserIdentity.Name;
            DashboardTitle = InterviewerUIResources.Dashboard_Title.FormatString(14, LoggedInUserName);

            NewInterviewsTabMenu = new DashboardTabMenuViewModel(1);
            StartedInterviewsTabMenu = new DashboardTabMenuViewModel(2);
            CompletedInterviewsTabMenu = new DashboardTabMenuViewModel(7);
            RejectedInterviewsTabMenu = new DashboardTabMenuViewModel(2);

            DashboardItems = new DashboardItemViewModel[]
            {
                new DashboardItemViewModel(), 
                new DashboardItemViewModel(), 
                new DashboardItemViewModel(), 
            };
        }

        private bool isSynchronizationInfoShowed;
        public bool IsSynchronizationInfoShowed
        {
            get { return this.isSynchronizationInfoShowed; }
            set { this.isSynchronizationInfoShowed = value; this.RaisePropertyChanged(); }
        }

        private IMvxCommand synchronizationCommand;
        public IMvxCommand SynchronizationCommand
        {
            get
            {
                return synchronizationCommand ??
                       (synchronizationCommand =
                           new MvxCommand(async () => await this.Synchronization.SynchronizeAsync(),
                               () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }

        private string dashboardTitle;
        public string DashboardTitle
        {
            get { return this.dashboardTitle; }
            set { this.dashboardTitle = value; this.RaisePropertyChanged(); }
        }

        private string loggedInUserName;
        public string LoggedInUserName
        {
            get { return this.loggedInUserName; }
            set { this.loggedInUserName = value; this.RaisePropertyChanged(); }
        }

        private DashboardTabMenuViewModel newInterviewsTabMenu;
        public DashboardTabMenuViewModel NewInterviewsTabMenu
        {
            get { return this.newInterviewsTabMenu; }
            set { this.newInterviewsTabMenu = value; this.RaisePropertyChanged(); }
        }

        private DashboardTabMenuViewModel startedInterviewsTabMenu;
        public DashboardTabMenuViewModel StartedInterviewsTabMenu
        {
            get { return this.startedInterviewsTabMenu; }
            set { this.startedInterviewsTabMenu = value; this.RaisePropertyChanged(); }
        }

        private DashboardTabMenuViewModel completedInterviewsTabMenu;
        public DashboardTabMenuViewModel CompletedInterviewsTabMenu
        {
            get { return this.completedInterviewsTabMenu; }
            set { this.completedInterviewsTabMenu = value; this.RaisePropertyChanged(); }
        }

        private DashboardTabMenuViewModel rejectedInterviewsTabMenu;
        public DashboardTabMenuViewModel RejectedInterviewsTabMenu
        {
            get { return this.rejectedInterviewsTabMenu; }
            set { this.rejectedInterviewsTabMenu = value; this.RaisePropertyChanged(); }
        }

        private DashboardItemViewModel[] dashboardItems;
        public DashboardItemViewModel[] DashboardItems
        {
            get { return this.dashboardItems; }
            set { this.dashboardItems = value;  this.RaisePropertyChanged(); }
        }

        public IMvxCommand ShowNewItemsInterviewsCommand
        {
            get { return new MvxCommand(() => SwitchTabCommand()); }
        }

        public IMvxCommand ShowStartedInterviewsCommand
        {
            get { return new MvxCommand(() => SwitchTabCommand()); }
        }

        public IMvxCommand ShowCompletedInterviewsCommand
        {
            get { return new MvxCommand(() => SwitchTabCommand()); }
        }

        public IMvxCommand ShowRejectedInterviewsCommand
        {
            get { return new MvxCommand(() => SwitchTabCommand()); }
        }

        private void SwitchTabCommand()
        {

        }

        public IMvxCommand SignOutCommand
        {
            get { return new MvxCommand(this.SignOut); }
        }

        void SignOut()
        {
            principal.SignOut();
            this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}