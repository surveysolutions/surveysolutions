using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        readonly IPrincipal principal;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
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

            NewInterviewsCount = 5;
            StartedInterviewsCount = 2;
            CompletedInterviewsCount = 7;
            RejectedInterviewsCount = 2;

            DashboardItems = new DashboardItemViewModel[]
            {
                new DashboardItemViewModel(), 
                new DashboardItemViewModel(), 
                new DashboardItemViewModel(), 
            };
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

        private int newInterviewsCount;
        public int NewInterviewsCount
        {
            get { return this.newInterviewsCount; }
            set 
            { 
                this.newInterviewsCount = value; 
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => IsExistsAnyNewInterview);
            }
        }

        public bool IsExistsAnyNewInterview
        {
            get { return this.newInterviewsCount > 0; }
        }

        private int startedInterviewsCount;
        public int StartedInterviewsCount
        {
            get { return this.startedInterviewsCount; }
            set 
            { 
                this.startedInterviewsCount = value; 
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => IsExistsAnyStartedInterview);
            }
        }

        public bool IsExistsAnyStartedInterview
        {
            get { return this.startedInterviewsCount > 0; }
        }

        private int completedInterviewsCount;
        public int CompletedInterviewsCount
        {
            get { return this.completedInterviewsCount; }
            set
            {
                this.completedInterviewsCount = value; 
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => IsExistsAnyCompletedInterview);
            }
        }

        public bool IsExistsAnyCompletedInterview
        {
            get { return this.completedInterviewsCount > 0; }
        }

        private int rejectedInterviewsCount;
        public int RejectedInterviewsCount
        {
            get { return this.rejectedInterviewsCount; }
            set
            {
                this.rejectedInterviewsCount = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => IsExistsAnyRejectedInterview);
            }
        }

        public bool IsExistsAnyRejectedInterview
        {
            get { return this.rejectedInterviewsCount > 0; }
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

        public IMvxCommand SynchronizationCommand
        {
            get { return new MvxCommand(this.ExecuteSynchronization); }
        }

        private void ExecuteSynchronization()
        {
            // do something
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