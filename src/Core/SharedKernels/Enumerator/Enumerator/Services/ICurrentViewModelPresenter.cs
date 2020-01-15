using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ICurrentViewModelPresenter
    {
        IMvxViewModel CurrentViewModel { get; }
    }
}
