using Android.Graphics;
using Android.Widget;
using AndroidX.Core.Content;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewIsSelectedYesNoOptionColorBinding : BaseBinding<TextView, bool>
    {
        public TextViewIsSelectedYesNoOptionColorBinding(TextView textView)
            : base(textView) {}

        protected override void SetValueToView(TextView textView, bool isSelected)
        {
            int textColor = isSelected
                ? Resource.Color.interview_question_yesno_selected_text
                : Resource.Color.interview_question_yesno_not_selected_text;

            if (textView.CurrentTextColor != textColor)
            {
                var color = new Color(ContextCompat.GetColor(Target.Context, textColor));
                textView.SetTextColor(color);
            }
        }
    }
}
