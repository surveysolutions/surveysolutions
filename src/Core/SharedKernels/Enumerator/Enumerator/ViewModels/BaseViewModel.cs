using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        public abstract void NavigateToPreviousViewModel();
    }
}