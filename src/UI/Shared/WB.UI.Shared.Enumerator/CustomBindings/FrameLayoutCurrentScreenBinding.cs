using System;
using Android.Support.V4.App;
using Android.Widget;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class FrameLayoutCurrentScreenBinding : BaseBinding<FrameLayout, InterviewStageViewModel>
    {
        public FrameLayoutCurrentScreenBinding(FrameLayout frameLayout)
            : base(frameLayout) {}

        protected override void SetValueToView(FrameLayout frameLayout, InterviewStageViewModel stageViewModel)
        {
            if (stageViewModel == null)
                return;

            var frameViewModel = stageViewModel.Stage;

            var viewModelType = frameViewModel.GetType();
            var mvxViewFinder = Mvx.Resolve<IMvxViewsContainer>();
            var fragmentType = mvxViewFinder.GetViewType(viewModelType);
            MvxFragment mvxFragment = Mvx.Resolve(fragmentType) as MvxFragment;

            if (mvxFragment == null)
                throw new ArgumentException("Can't resolve frame for ViewModel: " + viewModelType);

            mvxFragment.ViewModel = frameViewModel;

            var topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = (FragmentActivity)topActivity.Activity;
            var transaction = activity.SupportFragmentManager.BeginTransaction();

            SetCustomAnimations(transaction, stageViewModel.Direction);
            transaction.Replace(frameLayout.Id, mvxFragment);
            transaction.Commit();
        }

        private static FragmentTransaction SetCustomAnimations(FragmentTransaction transaction, NavigationDirection direction)
        {
            switch (direction)
            {
                case NavigationDirection.Previous:
                    return transaction.SetCustomAnimations(
                        Resource.Animation.slide_from_left,
                        Resource.Animation.abc_fade_out);

                case NavigationDirection.Next:
                    return transaction.SetCustomAnimations(
                        Resource.Animation.slide_from_right,
                        Resource.Animation.abc_fade_out);

                default:
                    return transaction.SetCustomAnimations(
                        Resource.Animation.zoom_in_from_center,
                        Resource.Animation.abc_fade_out);
            }
        }
    }
}