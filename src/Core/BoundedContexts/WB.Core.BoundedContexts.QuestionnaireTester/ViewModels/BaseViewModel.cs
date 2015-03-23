using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
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

        private IMvxCommand goBackCommand;
        public IMvxCommand GoBackCommand
        {
            get
            {
                return goBackCommand ?? (goBackCommand = new MvxCommand(this.GoBack));
            }
        }
        public virtual void GoBack()
        {
        }

        protected void SignOut()
        {
            this.Principal.SignOut();
            this.ShowViewModel<LoginViewModel>();
        }
    }
}