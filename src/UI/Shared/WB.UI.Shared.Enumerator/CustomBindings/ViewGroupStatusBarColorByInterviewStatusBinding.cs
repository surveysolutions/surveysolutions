﻿using Android.App;
using Android.OS;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewGroupStatusBarColorByInterviewStatusBinding : BaseBinding<ViewGroup, GroupStatus>
    {
        public ViewGroupStatusBarColorByInterviewStatusBinding(ViewGroup androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(ViewGroup target, GroupStatus value)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                return;

            switch (value)
            {
                case GroupStatus.CompletedInvalid:
                case GroupStatus.StartedInvalid:
                    SetBackgroundColor(target, Resource.Color.interviewStatusBarErrors);
                    break;

                case GroupStatus.Completed:
                    SetBackgroundColor(target, Resource.Color.interviewStatusBarCompleted);
                    break;

                case GroupStatus.Started:
                case GroupStatus.NotStarted:
                default:
                    SetBackgroundColor(target, Resource.Color.interviewStatusBarInProgress);
                    break;
            }
        }

        private static void SetBackgroundColor(ViewGroup target, int colorResourceId)
        {
            var color = target.Resources.GetColor(colorResourceId);

            var activity = (Activity)target.Context;
            Window window = activity.Window;
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            window.SetStatusBarColor(color);
        }
    }
}