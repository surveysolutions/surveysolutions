using System;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.Views.FinishInstallation
{
    public class FinishIntallationViewModel : MvxViewModel
    {
        public string SyncEndpoint { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public FinishIntallationViewModel() {}

        public FinishIntallationViewModel(string login, string password, string syncEndpoint)
            : this()
        {
            this.Login = login;
            this.Password = password;
            this.SyncEndpoint = syncEndpoint;
        }
    }
}