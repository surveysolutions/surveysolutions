using System;
using System.Collections.Generic;
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
        readonly Dictionary<Type, Func<MvxFragment>> mapViewModelToFragment = new Dictionary<Type, Func<MvxFragment>>()
        {
            { typeof(ActiveStageViewModel),        Mvx.Resolve<InterviewEntitiesListFragment> },
            { typeof(CompleteInterviewViewModel),  Mvx.Resolve<CompleteInterviewFragment>     },
        };


        public FrameLayoutCurrentScreenBinding(FrameLayout frameLayout)
            : base(frameLayout) {}

        protected override void SetValueToView(FrameLayout frameLayout, MvxViewModel frameViewModel)
        {
            var viewModelType = frameViewModel.GetType();
            Func<MvxFragment> frameResolver;

            if (!this.mapViewModelToFragment.TryGetValue(viewModelType, out frameResolver))
                throw new ArgumentException("Can't resolve frame for ViewModel: " + viewModelType);

            MvxFragment mvxFragment = frameResolver.Invoke();
            mvxFragment.ViewModel = frameViewModel;

            IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = (FragmentActivity)topActivity.Activity;
            var trans = activity.SupportFragmentManager.BeginTransaction();
            trans.Replace(frameLayout.Id, mvxFragment);
            trans.Commit();
        }
    }
}