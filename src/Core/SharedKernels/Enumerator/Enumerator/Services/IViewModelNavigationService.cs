using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IViewModelNavigationService
    {
        void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel;
        void NavigateTo<TViewModel>(object perameters) where TViewModel : IMvxViewModel;
        void NavigateToDashboard();
    }
}