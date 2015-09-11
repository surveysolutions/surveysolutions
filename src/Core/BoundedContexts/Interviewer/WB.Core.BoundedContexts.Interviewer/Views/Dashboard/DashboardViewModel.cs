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

            this.CurrentDashboardCategory = DashboardInterviewCategories.New;

            this.RefreshDashboard();
        }

        private async void RefreshDashboard()
        {
            this.dashboardInformation = await this.dashboardFactory.GetDashboardItems(
                this.principal.CurrentUserIdentity.UserId,
                this.currentDashboardCategory);

            this.RefreshTab();
        }

        private void RefreshTab()
        {
            switch (CurrentDashboardCategory)
            {
                 case DashboardInterviewCategories.New:
                    this.DashboardItems = dashboardInformation.NewInterviews;
                    break;
                 case DashboardInterviewCategories.InProgress:
                    this.DashboardItems = dashboardInformation.StartedInterviews;
                    break;
                 case DashboardInterviewCategories.Complited:
                    this.DashboardItems = dashboardInformation.CompletedInterviews;
                    break;
                 case DashboardInterviewCategories.Rejected:
                    this.DashboardItems = dashboardInformation.RejectedInterviews;
                    break;
            }
        }

        private DashboardInformation dashboardInformation;
        private DashboardInterviewCategories currentDashboardCategory;
        public bool IsNewInterviewsCategorySelected { get { return CurrentDashboardCategory == DashboardInterviewCategories.New; } }
        public bool IsStartedInterviewsCategorySelected { get { return CurrentDashboardCategory == DashboardInterviewCategories.InProgress; } }
        public bool IsCompletedInterviewsCategorySelected { get { return CurrentDashboardCategory == DashboardInterviewCategories.Complited; } }
        public bool IsRejectedInterviewsCategorySelected { get { return CurrentDashboardCategory == DashboardInterviewCategories.Rejected; } }

        public DashboardInterviewCategories CurrentDashboardCategory
        {
            get { return this.currentDashboardCategory; }
            set 
            {
                this.currentDashboardCategory = value; 
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

        public bool IsExistsAnyNewInterview { get { return this.NewInterviewsCount > 0; } }
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
            if (category == this.CurrentDashboardCategory)
                return;

            this.CurrentDashboardCategory = category;
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
    }
}