using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.Views.Login
{
    public class LoginViewModel : MvxViewModel
    {
        public string Login { get; private set; }
        public string Password { get; private set; }
        public LoginViewModel()
        {
        }

        public LoginViewModel(string login, string password)
            : this()
        {
            this.Login = login;
            this.Password = password;
        }
       
    }
}