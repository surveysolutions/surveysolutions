using System;
using AndroidApp.Core.Model.Authorization;
using AndroidApp.Core.Model.ViewModel.Dashboard;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
namespace AndroidApp.Core.Model.ViewModel.Login
{
    public class LoginViewModel : MvxViewModel
    {
        public Guid PublicKey { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public LoginViewModel()
        {
        }

        public LoginViewModel(Guid publicKey, string login, string password, IAuthentication membership)
            : this()
        {
            PublicKey = publicKey;
            Login = login;
            Password = password;
        }
       
    }
}