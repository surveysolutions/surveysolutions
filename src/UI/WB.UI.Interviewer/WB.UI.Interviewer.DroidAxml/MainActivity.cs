using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using InterviewViewModel = WB.Core.BoundedContexts.Capi.ViewModel.InterviewViewModel;

namespace AxmlTester.Droid
{
    [Activity(Label = "AxmlTester.Droid", MainLauncher = true, Icon = "@drawable/icon", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class MainActivity : BaseMvxActivity<InterviewViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Questions);

            var list = FindViewById<MvxListView>(Resource.Id.TheListView);
            list.Adapter = new CustomAdapter(this, (IMvxAndroidBindingContext)BindingContext);
        }
    }
}

