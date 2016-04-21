using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.Fragging.Fragments;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseFragment<TViewModel> : MvxFragment<TViewModel> where TViewModel : MvxViewModel
    {
        protected abstract int ViewResourceId { get; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(ViewResourceId, null);
        }
    }
}