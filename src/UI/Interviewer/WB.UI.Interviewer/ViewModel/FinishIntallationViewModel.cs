using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Interviewer.ViewModel
{
    public class FinishIntallationViewModel : BaseViewModel
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IInterviewerSettings interviewerSettings;
        readonly IViewModelNavigationService viewModelNavigationService;

        public FinishIntallationViewModel(IPasswordHasher passwordHasher, IInterviewerSettings interviewerSettings, IViewModelNavigationService viewModelNavigationService)
        {
            this.passwordHasher = passwordHasher;
            this.interviewerSettings = interviewerSettings;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        private bool canSetSyncEndpoint = true;
        public bool CanSetSyncEndpoint
        {
            get { return this.canSetSyncEndpoint; }
            set
            {
                this.canSetSyncEndpoint = value;
                this.RaisePropertyChanged();
            }
        }

        public string SyncEndpoint { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }

        public IMvxCommand StartSynchronizationCommand
        {
            get { return new MvxCommand(this.StartSynchronization); }
        }

        private void StartSynchronization()
        {
            try
            {
                this.interviewerSettings.SetSyncAddressPoint(this.SyncEndpoint);
                this.viewModelNavigationService.NavigateTo<SynchronizationViewModel>(new
                {
                    login = this.Login,
                    passwordHash = this.passwordHasher.Hash(this.Password)
                });
            }
            catch(ArgumentException)
            {
                this.CanSetSyncEndpoint = false;
            }
        }

        public void Init()
        {
            this.SyncEndpoint = "";
#if DEBUG
            this.SyncEndpoint = "http://192.168.88.226/headquarters";
            this.Login = "int";
            this.Password = "1";
#endif
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}