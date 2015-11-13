using Android.Graphics;
using Android.Widget;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewIsSelectedYesNoOptionColorBinding : BaseBinding<TextView, bool>
    {
        public TextViewIsSelectedYesNoOptionColorBinding(TextView textView)
            : base(textView) {}

        protected override void SetValueToView(TextView textView, bool isSelected)
        {
            Color textColor = isSelected
                ? textView.Resources.GetColor(Resource.Color.interview_question_yesno_selected_text)
                : textView.Resources.GetColor(Resource.Color.interview_question_yesno_not_selected_text);

            textView.SetTextColor(textColor);
        }
    }
}