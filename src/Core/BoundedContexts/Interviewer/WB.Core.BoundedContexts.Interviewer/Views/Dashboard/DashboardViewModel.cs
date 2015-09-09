using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerDashboardFactory dashboardFactory;
        private readonly IPrincipal principal;
        public readonly SynchronizationViewModel Synchronization;

        public DashboardViewModel(IViewModelNavigationService viewModelNavigationService,
            IInterviewerDashboardFactory dashboardFactory,
            IPrincipal principal, SynchronizationViewModel synchronization)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.dashboardFactory = dashboardFactory;
            this.principal = principal;
            this.Synchronization = synchronization;
        }

        public void Init()
        {
            if (!this.principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateTo<LoginViewModel>();
                return;
            }

            LoggedInUserName = this.principal.CurrentUserIdentity.Name;
            currentDashboardCategory = DashboardInterviewCategories.New;
            this.RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            var staistic = this.dashboardFactory.GetDashboardItems(this.principal.CurrentUserIdentity.UserId);

            this.DashboardTitle = InterviewerUIResources.Dashboard_Title.FormatString(14, this.LoggedInUserName);

            this.NewInterviewsCount = staistic.NewInterviewsCount;
            this.StartedInterviewsCount = staistic.StartedInterviewsCount;
            this.CompletedInterviewsCount = staistic.CompletedInterviewsCount;
            this.RejectedInterviewsCount = staistic.RejectedInterviewsCount;

            this.DashboardItems = staistic.DashboardItems;
        }

        private DashboardInterviewCategories currentDashboardCategory;

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
                return synchronizationCommand ?? (synchronizationCommand = new MvxCommand(this.RunSynchronization, () => !this.Synchronization.IsSynchronizationInProgress));
            }
        }

        private async void RunSynchronization()
        {
            await this.Synchronization.SynchronizeAsync();

            RefreshDashboard();
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

        private IEnumerable<InterviewDashboardItemViewModel> dashboardItems;
        public IEnumerable<InterviewDashboardItemViewModel> DashboardItems
        {
            get { return this.dashboardItems; }
            set { this.dashboardItems = value;  this.RaisePropertyChanged(); }
        }

        public IMvxCommand ShowNewItemsInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewCategories.New)); }
        }

        public IMvxCommand ShowStartedInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewCategories.InProgress)); }
        }

        public IMvxCommand ShowCompletedInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewCategories.Complited)); }
        }

        public IMvxCommand ShowRejectedInterviewsCommand
        {
            get { return new MvxCommand(() => ShowInterviewsCommand(DashboardInterviewCategories.Rejected)); }
        }

        private void ShowInterviewsCommand(DashboardInterviewCategories category)
        {
            this.currentDashboardCategory = category;
            this.RefreshDashboard();
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
    }
}