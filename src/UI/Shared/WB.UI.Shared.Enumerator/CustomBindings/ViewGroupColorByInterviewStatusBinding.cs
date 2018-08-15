using Android.Graphics;
using Android.Support.V4.Content;
using Android.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Shared.Enumerator.CustomBindings
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
                    SetBackgroundColor(target, Resource.Color.interviewHeaderCompleted);
                    break;
                case GroupStatus.Disabled:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderDisabled);
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
            var color = new Color(ContextCompat.GetColor(target.Context, colorResourceId));
            target.SetBackgroundColor(color);
        }
    }
}
