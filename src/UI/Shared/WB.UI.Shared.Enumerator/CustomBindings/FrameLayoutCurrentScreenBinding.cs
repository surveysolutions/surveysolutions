using System;
using Android.Widget;
using AndroidX.Fragment.App;
using MvvmCross;
using MvvmCross.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using FragmentTransaction=AndroidX.Fragment.App.FragmentTransaction;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class FrameLayoutCurrentScreenBinding : BaseBinding<FrameLayout, InterviewStageViewModel>
    {
        public FrameLayoutCurrentScreenBinding(FrameLayout frameLayout)
            : base(frameLayout) {}

        protected override void SetValueToView(FrameLayout frameLayout, InterviewStageViewModel stageViewModel)
        {
            var userInterfaceStateService = Mvx.IoCProvider.Resolve<IUserInterfaceStateService>();

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
            var mvxViewFinder = Mvx.IoCProvider.Resolve<IMvxViewsContainer>();
            var fragmentType = mvxViewFinder.GetViewType(viewModelType);

            if (!(Mvx.IoCProvider.Resolve(fragmentType) is MvvmCross.Platforms.Android.Views.Fragments.MvxFragment mvxFragment))
                throw new ArgumentException("Can't resolve frame for ViewModel: " + viewModelType);

            mvxFragment.ViewModel = frameViewModel;

            var activity = (FragmentActivity)frameLayout.Context;
            var transaction = activity.SupportFragmentManager.BeginTransaction();

            transaction = SetCustomAnimations(transaction, stageViewModel.Direction);
            transaction.Replace(frameLayout.Id, mvxFragment);
            transaction.Commit();

            activity.SupportFragmentManager.ExecutePendingTransactions();

            transaction.Dispose();
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
