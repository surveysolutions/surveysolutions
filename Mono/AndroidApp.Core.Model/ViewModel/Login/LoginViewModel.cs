using System;
using AndroidApp.Core.Model.Authorization;
using AndroidApp.Core.Model.ViewModel.Dashboard;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
namespace AndroidApp.Core.Model.ViewModel.Login
{
    public class LoginViewModel : MvxViewModel
    {
        private readonly IAuthentication membership;
        public Guid PublicKey { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public LoginViewModel(IAuthentication membership)
        {
            this.membership = membership;
        }

        public LoginViewModel(Guid publicKey, string login, string password, IAuthentication membership)
            : this(membership)
        {
            PublicKey = publicKey;
            Login = login;
            Password = password;
        }
        public System.Windows.Input.ICommand LoginCommand
        {
            get
            {
                return new MvxRelayCommand(PerfomLogin);
            }
        }
        private void PerfomLogin()
        {
            var result = this.membership.LogOn(this.Login, this.Password);
            if (result)
                RequestNavigate<DashboardModel>();
        }
    }
}