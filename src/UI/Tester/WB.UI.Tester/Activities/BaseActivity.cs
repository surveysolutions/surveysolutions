using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.UI.Tester.Activities
{
    public abstract class BaseActivity<TViewModel> : BaseMvxActivity where TViewModel : BaseViewModel
    {
        protected abstract int ViewResourceId { get; }

        public new TViewModel ViewModel
        {
            get { return (TViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.SetContentView(this.ViewResourceId);
        }

        public override void OnBackPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel();
        }
    }
}