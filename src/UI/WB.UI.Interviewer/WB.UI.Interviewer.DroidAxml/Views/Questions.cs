using Android.App;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;

namespace AxmlTester.Droid.Views
{
    [Activity(Label = "View for FirstViewModel")]
    public class Questions : MvxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Questions);
        }
    }
}