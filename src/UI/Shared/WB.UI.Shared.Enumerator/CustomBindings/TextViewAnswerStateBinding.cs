using Android.Graphics;
using Android.Support.V4.Content;
using Android.Widget;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewAnswerStateBinding : BaseBinding<TextView, OverviewNodeState>
    {
        public TextViewAnswerStateBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, OverviewNodeState value)
        {
            int? colorid = null;
            switch (value)
            {
                case OverviewNodeState.Answered:
                {
                    colorid = Resource.Color.recordedAnswerText;
                    break;
                }
                case OverviewNodeState.Unanswered:
                {
                    colorid = Resource.Color.disabledTextColor;
                    control.SetText(UIResources.Interview_Overview_NotAnswered, TextView.BufferType.Normal);
                    break;
                }
                case OverviewNodeState.Invalid:
                {
                    colorid = Resource.Color.errorTextColor;
                    break;
                }
                case OverviewNodeState.Commented:
                {
                    colorid = Resource.Color.commentsTextColor;
                    break;
                }
            }

            if (colorid.HasValue)
            {
                control.SetTextColor(new Color(ContextCompat.GetColor(control.Context, colorid.Value)));
            }
        }
    }
}
