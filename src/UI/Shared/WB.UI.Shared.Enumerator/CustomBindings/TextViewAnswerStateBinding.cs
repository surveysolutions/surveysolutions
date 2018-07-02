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
            var typeFace = TypefaceStyle.Bold;
            switch (value)
            {
                case OverviewNodeState.Answered:
                {
                    colorid = Resource.Color.recordedAnswerText;
                    break;
                }
                case OverviewNodeState.Unanswered:
                {
                    colorid = Resource.Color.notAnsweredLabel;
                    typeFace = TypefaceStyle.BoldItalic;
                    control.SetText(UIResources.Interview_Overview_NotAnswered, TextView.BufferType.Normal);
                    break;
                }
                case OverviewNodeState.Invalid:
                {
                    colorid = Resource.Color.overview_invalid_text;
                    break;
                }
            }

            if (colorid.HasValue)
            {
                control.SetTypeface(null, typeFace);
                control.SetTextColor(new Color(ContextCompat.GetColor(control.Context, colorid.Value)));
            }
        }
    }
}
