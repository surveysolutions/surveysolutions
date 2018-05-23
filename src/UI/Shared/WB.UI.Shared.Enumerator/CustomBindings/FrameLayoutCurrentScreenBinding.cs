using System;
using Android.Support.V4.App;
using Android.Widget;
using MvvmCross;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android;
using MvvmCross.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class FrameLayoutCurrentScreenBinding : BaseBinding<FrameLayout, InterviewStageViewModel>
    {
        public FrameLayoutCurrentScreenBinding(FrameLayout frameLayout)
            : base(frameLayout) {}

        protected override void SetValueToView(FrameLayout frameLayout, InterviewStageViewModel stageViewModel)
        {
            var userInterfaceStateService = Mvx.Resolve<IUserInterfaceStateService>();

            userInterfaceStateService.NotifyRefreshStarted();
            try
            {
                SetValueToViewImpl(frameLayout, stageViewModel);
            }
            finally
            {
                userInterfaceStateService.NotifyRefreshFinished();
            }
        }

        private static void SetValueToViewImpl(FrameLayout frameLayout, InterviewStageViewModel stageViewModel)
        {
            if (stageViewModel == null)
                return;

            var frameViewModel = stageViewModel.Stage;

            var viewModelType = frameViewModel.GetType();
            var mvxViewFinder = Mvx.Resolve<IMvxViewsContainer>();
            var fragmentType = mvxViewFinder.GetViewType(viewModelType);

            if (!(Mvx.Resolve(fragmentType) is MvxFragment mvxFragment))
                throw new ArgumentException("Can't resolve frame for ViewModel: " + viewModelType);

            mvxFragment.ViewModel = frameViewModel;

            var topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = (FragmentActivity)topActivity.Activity;
            var transaction = activity.SupportFragmentManager.BeginTransaction();

            SetCustomAnimations(transaction, stageViewModel.Direction);
            transaction.Replace(frameLayout.Id, mvxFragment);
            transaction.Commit();

            activity.SupportFragmentManager.ExecutePendingTransactions();
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

                case NavigationDirection.Inside:
                    return transaction.SetCustomAnimations(
                        Resource.Animation.zoom_in_from_center,
                        Resource.Animation.abc_fade_out);

                case NavigationDirection.Outside:
                    return transaction.SetCustomAnimations(
                        Resource.Animation.abc_fade_in,
                        Resource.Animation.zoom_out_to_center);

                default:
                    return transaction;
            }
        }
    }
}
