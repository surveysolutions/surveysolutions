using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.ViewModels
{
    public class BaseViewModel : MvxViewModel
    {
        protected readonly ILogger Logger;
        protected readonly IPrincipal Principal;
        protected readonly IUserInteraction UIDialogs;

        public BaseViewModel(ILogger logger, IPrincipal principal, IUserInteraction uiDialogs = null)
        {
            this.Logger = logger;
            this.Principal = principal;
            this.UIDialogs = uiDialogs;
        }

        public virtual void GoBack()
        {
            this.ShowViewModel<DashboardViewModel>();
        }

        protected virtual void SignOut()
        {
            this.Principal.SignOut();
            this.ShowViewModel<LoginViewModel>();
        }
    }
}