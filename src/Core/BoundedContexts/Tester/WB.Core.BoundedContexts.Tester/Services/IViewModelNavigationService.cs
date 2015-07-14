using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IViewModelNavigationService
    {
        void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel;
        void NavigateTo<TViewModel>(object perameters) where TViewModel : IMvxViewModel;
    }
}