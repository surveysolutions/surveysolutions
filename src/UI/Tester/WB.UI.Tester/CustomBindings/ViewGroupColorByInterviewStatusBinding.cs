using System;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;


namespace WB.UI.Tester.CustomBindings
{
    public class ViewGroupColorByInterviewStatusBinding : BaseBinding<ViewGroup, GroupStatus>
    {
        public ViewGroupColorByInterviewStatusBinding(ViewGroup androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(ViewGroup target, GroupStatus value)
        {
            switch (value)
            {
                case GroupStatus.CompletedInvalid:
                case GroupStatus.StartedInvalid:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderErrors);
                    break;

                case GroupStatus.Completed:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderComplited);
                    break;

                case GroupStatus.Started:
                case GroupStatus.NotStarted:
                default:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderInProgress);
                    break;
            }
        }

        private static void SetBackgroundColor(ViewGroup target, int colorResourceId)
        {
            target.SetBackgroundColor(target.Resources.GetColor(colorResourceId));
        }
    }
}