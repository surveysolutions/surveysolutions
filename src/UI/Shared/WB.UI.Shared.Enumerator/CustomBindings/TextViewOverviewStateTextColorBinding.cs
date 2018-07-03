using Android.Graphics;
using Android.Support.V4.Content;
using Android.Widget;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewNodeStateTextColorBinding : BaseBinding<TextView, OverviewNodeState>
    {
        public TextViewNodeStateTextColorBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, OverviewNodeState value)
        {
            if (value == OverviewNodeState.Answered || value == OverviewNodeState.Unanswered)
            {
                var color =  new Color(ContextCompat.GetColor(control.Context, Resource.Color.recordedAnswerText));
                control.SetTextColor(color);
            }
            else if (value == OverviewNodeState.Invalid)
            {
                var color =  new Color(ContextCompat.GetColor(control.Context, Resource.Color.overview_invalid_text));
                control.SetTextColor(color);
            }
        }
    }
}
