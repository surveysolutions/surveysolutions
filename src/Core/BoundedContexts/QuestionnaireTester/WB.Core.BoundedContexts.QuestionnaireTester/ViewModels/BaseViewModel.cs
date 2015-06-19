using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

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