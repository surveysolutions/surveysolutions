using Android.App;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Shared.Fragments;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;

namespace WB.UI.Shared.Enumerator.Utils
{
    public static class DialogHelper
    {
        public static Dialog ShowDialog(int resourceId, MvxViewModel viewModel)
        {
            var parentActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            var view = new MvxAndroidBindingContext(parentActivity, new MvxSimpleLayoutInflaterHolder(parentActivity.LayoutInflater), viewModel)
                .BindingInflate(resourceId, null, false);

            var dialog = new AppCompatDialog(parentActivity);
            dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            dialog.SetCancelable(false);
            dialog.SetContentView(view);

            dialog.Show();
            dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            dialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);

            return dialog;
        }
    }
}