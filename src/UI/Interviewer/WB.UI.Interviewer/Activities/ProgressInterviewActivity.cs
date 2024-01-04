using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    public abstract class ProgressInterviewActivity<T> : BaseActivity<T> where T : ProgressViewModel
    {
        protected override int ViewResourceId => Resource.Layout.loading;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(toolbar);
        }
        
        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
            this.CancelLoadingAndFinishActivity();
        }

        private void CancelLoadingAndFinishActivity()
        {
            this.ViewModel.CancelLoadingCommand.Execute();
            this.Finish();
        }
    }
}
