using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using MvvmCross.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    public abstract class ProgressInterviewActivity<T> : BaseActivity<T> where T : class, IMvxViewModel
    {
        protected override int ViewResourceId => Resource.Layout.loading;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(toolbar);
        }
    }
}
