using MvvmCross.IoC;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Views;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Services
{
    public class CurrentViewModelPresenter : ICurrentViewModelPresenter
    {
        public IMvxViewModel CurrentViewModel => ((IMvxAndroidView)MvxIoCProvider.Instance.Resolve<IMvxAndroidCurrentTopActivity>()?.Activity)?.ViewModel;
    }
}
