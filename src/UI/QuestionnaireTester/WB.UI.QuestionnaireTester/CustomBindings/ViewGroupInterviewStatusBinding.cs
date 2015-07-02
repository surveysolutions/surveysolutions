using System;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;


namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ViewGroupInterviewStatusBinding : BaseBinding<ViewGroup, GroupStatus>
    {
        public ViewGroupInterviewStatusBinding(ViewGroup androidControl)
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