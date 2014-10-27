using System.Collections.Specialized;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Utility;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.ValueObjects;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi.Views
{
    public class FinishIntallationViewModel : MvxViewModel
    {
        private INavigationService NavigationService
        {
            get
            {
                return  ServiceLocator.Current.GetInstance<INavigationService>();
            }
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
#if DEBUG
            this.SyncEndpoint = "http://192.168.173.1/headquarters";
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
            if (!SettingsManager.SetSyncAddressPoint(this.SyncEndpoint))
            {
                this.CanSetSyncEndpoint = false;
                return;
            }
            
            NavigationService.NavigateTo(CapiPages.Synchronization, new NameValueCollection
            {
                {"Login", Login},
                {"PasswordHash", SimpleHash.ComputeHash(Password)}
            });
        }
    }
}