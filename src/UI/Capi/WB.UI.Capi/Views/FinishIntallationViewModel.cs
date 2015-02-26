using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.ValueObjects;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi.Views
{
    public class FinishIntallationViewModel : MvxViewModel
    {
        private INavigationService NavigationService
        {
            get { return ServiceLocator.Current.GetInstance<INavigationService>(); }
        }

        private IPasswordHasher passwordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
        }

        private IInterviewerSettings interviewerSettings
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewerSettings>(); }
        }

        private bool canSetSyncEndpoint = true;

        public bool CanSetSyncEndpoint
        {
            get { return canSetSyncEndpoint; }
            set
            {
                canSetSyncEndpoint = value;
                RaisePropertyChanged(() => CanSetSyncEndpoint);
            }
        }

        public string SyncEndpoint { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }

        public FinishIntallationViewModel()
        {
            this.SyncEndpoint = "";
#if DEBUG
            this.SyncEndpoint = "http://192.168.88.226/headquarters";
            this.Login = "int";
            this.Password = "1";
#endif
        }

        public FinishIntallationViewModel(string login, string password, string syncEndpoint)
            : this()
        {
            this.Login = login;
            this.Password = password;
            this.SyncEndpoint = syncEndpoint;
        }

        public IMvxCommand StartSynchronizationCommand
        {
            get { return new MvxCommand(this.StartSynchronization); }
        }

        private void StartSynchronization()
        {
            try
            {
                this.interviewerSettings.SetSyncAddressPoint(this.SyncEndpoint);
                NavigationService.NavigateTo(CapiPages.Synchronization, new Dictionary<string, string>
                {
                    {"Login", Login},
                    {"PasswordHash", passwordHasher.Hash(Password)}
                });
            }
            catch(ArgumentException)
            {
                this.CanSetSyncEndpoint = false;
            }
        }
    }
}