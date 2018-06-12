using Android.Graphics;
using Android.Support.V4.Content;
using Android.Widget;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewUnansweredTextColorBinding : BaseBinding<TextView, OverviewNodeState>
    {
        public TextViewUnansweredTextColorBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, OverviewNodeState value)
        {
            if (value == OverviewNodeState.Unanswered)
            {
                var color =  new Color(ContextCompat.GetColor(control.Context, Resource.Color.disabledTextColor));
                control.SetTextColor(color);
            }
        }
    }
}
