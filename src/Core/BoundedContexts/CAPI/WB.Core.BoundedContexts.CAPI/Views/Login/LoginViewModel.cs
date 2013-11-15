using System;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.Views.Login
{
    public class LoginViewModel : MvxViewModel
    {
        public Guid PublicKey { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public LoginViewModel()
        {
        }

        public LoginViewModel(Guid publicKey, string login, string password)
            : this()
        {
            this.PublicKey = publicKey;
            this.Login = login;
            this.Password = password;
        }
       
    }
}