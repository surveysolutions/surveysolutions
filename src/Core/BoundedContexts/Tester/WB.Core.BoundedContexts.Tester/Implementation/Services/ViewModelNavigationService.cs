using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class ViewModelNavigationService : MvxNavigatingObject, IViewModelNavigationService
    {
        public void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>();
        }

        public void NavigateTo<TViewModel>(object perameterValuesObject) where TViewModel : IMvxViewModel
        {
            this.ShowViewModel<TViewModel>(perameterValuesObject);
        }

        public void NavigateToDashboard()
        {
            NavigateTo<DashboardViewModel>();
        }
    }
}