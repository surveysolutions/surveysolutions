using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseFragment<TViewModel> : MvxFragment<TViewModel> where TViewModel : BaseViewModel
    {
        protected abstract int ViewResourceId { get; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.EnsureBindingContextIsSet(inflater);
            return this.BindingInflate(ViewResourceId, null);
        }

        public override void OnDestroy()
        {
            this.BindingContext?.ClearAllBindings();
            
            //will handle it with ViewModel.ViewDestroy 
            //base.ViewModel?.DisposeIfDisposable();
            
            base.OnDestroy();
        }
    }
}
