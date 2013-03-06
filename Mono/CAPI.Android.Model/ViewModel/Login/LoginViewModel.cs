using System;
using CAPI.Android.Core.Model.Authorization;
using Cirrious.MvvmCross.ViewModels;

namespace CAPI.Android.Core.Model.ViewModel.Login
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