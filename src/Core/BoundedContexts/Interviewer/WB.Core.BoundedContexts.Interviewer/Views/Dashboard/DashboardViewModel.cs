using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        readonly IDataCollectionAuthentication authenticationService;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IDataCollectionAuthentication authenticationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.authenticationService = authenticationService;
        }

        public void Init()
        {
            if (!this.authenticationService.IsLoggedIn)
            {
                this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
                return;
            }

            this.LoggedInUserName = this.authenticationService.CurrentUser.Name;

            NewInterviewsTabMenu = new DashboardTabMenuViewModel(5);
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
            set
            {
                this.newInterviewsTabMenu = value;
                this.RaisePropertyChanged();
            }
        }

        private DashboardTabMenuViewModel startedInterviewsTabMenu;
        public DashboardTabMenuViewModel StartedInterviewsTabMenu
        {
            get { return this.startedInterviewsTabMenu; }
            set
            {
                this.startedInterviewsTabMenu = value;
                this.RaisePropertyChanged();
            }
        }

        private DashboardTabMenuViewModel completedInterviewsTabMenu;
        public DashboardTabMenuViewModel CompletedInterviewsTabMenu
        {
            get { return this.completedInterviewsTabMenu; }
            set
            {
                this.completedInterviewsTabMenu = value;
                this.RaisePropertyChanged();
            }
        }

        private DashboardTabMenuViewModel rejectedInterviewsTabMenu;
        public DashboardTabMenuViewModel RejectedInterviewsTabMenu
        {
            get { return this.rejectedInterviewsTabMenu; }
            set
            {
                this.rejectedInterviewsTabMenu = value;
                this.RaisePropertyChanged();
            }
        }

        private DashboardItemViewModel[] dashboardItems;
        public DashboardItemViewModel[] DashboardItems
        {
            get { return this.dashboardItems; }
            set
            {
                this.dashboardItems = value; 
                this.RaisePropertyChanged();
            }
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

        void SignOut()
        {
            authenticationService.LogOff();
            this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}