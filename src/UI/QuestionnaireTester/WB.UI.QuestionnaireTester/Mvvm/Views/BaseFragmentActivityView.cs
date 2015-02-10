using Android.OS;
using Cirrious.MvvmCross.Droid.Fragging;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    public abstract class BaseFragmentActivityView<TViewModel> : MvxFragmentActivity
        where TViewModel : BaseViewModel
    {
        protected abstract int ViewResourceId { get; }

        public new TViewModel ViewModel
        {
            get { return (TViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetContentView(this.ViewResourceId);
        }
    }
}