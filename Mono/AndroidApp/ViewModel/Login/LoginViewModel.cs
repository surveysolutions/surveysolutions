using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Authorization;
using AndroidApp.ViewModel.Dashboard;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ViewModels;
namespace AndroidApp.ViewModel.Login
{
    public class LoginViewModel : MvxViewModel
    {
        private readonly IAuthentication membership;
        public Guid PublicKey { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public LoginViewModel()
        {
            this.membership = CapiApplication.Membership;
        }

        public LoginViewModel(Guid publicKey, string login, string password):this()
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