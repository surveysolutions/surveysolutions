using Android.App;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Widget;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.Fragging.Fragments;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class FrameLayoutCurrentScreenBinding : BaseBinding<FrameLayout, MvxViewModel>
    {
        public FrameLayoutCurrentScreenBinding(FrameLayout frameLayout)
            : base(frameLayout) {}

        protected override void SetValueToView(FrameLayout frameLayout, MvxViewModel frameViewModel)
        {
            MvxFragment mvxFragment = frameViewModel is ActiveStageViewModel
                ? Mvx.Resolve<InterviewEntitiesListFragment>()
                : (MvxFragment)Mvx.Resolve<CompleteInterviewFragment>();

            mvxFragment.ViewModel = frameViewModel;

            IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = (FragmentActivity)topActivity.Activity;
            var trans = activity.SupportFragmentManager.BeginTransaction();
            trans.Replace(frameLayout.Id, mvxFragment);
            trans.Commit();
        }
    }
}