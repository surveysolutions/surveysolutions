using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        public readonly ILogger Logger;

        protected BaseViewModel() { }

        protected BaseViewModel(ILogger logger)
        {
            this.Logger = logger;
        }

        public abstract void NavigateToPreviousViewModel();
    }
}