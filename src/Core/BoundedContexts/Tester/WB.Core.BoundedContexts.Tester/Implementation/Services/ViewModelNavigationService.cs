using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class ViewModelNavigationService : MvxNavigatingObject, IViewModelNavigationService
    {
        public void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>();
        }

        public void NavigateTo<TViewModel>(object perameterValuesObject) where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>(perameterValuesObject);
        }
    }
}