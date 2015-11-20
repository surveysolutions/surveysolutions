using WB.Core.SharedKernels.Enumerator.ViewModels;
using Cirrious.MvvmCross.Droid.Support.AppCompat;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseActivity<TViewModel> : MvxAppCompatActivity<TViewModel> where TViewModel : BaseViewModel
    {
        protected abstract int ViewResourceId { get; }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.SetContentView(this.ViewResourceId);
        }
    }
}