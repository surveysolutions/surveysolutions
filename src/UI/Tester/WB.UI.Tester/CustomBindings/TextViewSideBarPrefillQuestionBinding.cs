using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.UI.Tester.CustomBindings
{
    public class TextViewSideBarPrefillQuestionBinding : BaseBinding<TextView, SideBarPrefillQuestion>
    {
        public TextViewSideBarPrefillQuestionBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, SideBarPrefillQuestion value)
        {
            if (value == null)
                return;

            var answerText = value.Answer ?? string.Empty;
            var questionText = value.Question == null ? string.Empty : value.Question.ToUpper();

            var message = string.Format(@"{0} : {1}", questionText, answerText);
            SpannableString str = new SpannableString(message);

            str.SetSpan(new StyleSpan(TypefaceStyle.Bold),
                message.Length - answerText.Length,
                message.Length,
                SpanTypes.ExclusiveExclusive);
            str.SetSpan(new ForegroundColorSpan(GetAnswerColor(control)),
                message.Length - answerText.Length,
                message.Length,
                SpanTypes.ExclusiveExclusive);

            control.TextFormatted = str;
        }

        private Color GetAnswerColor(TextView control)
        {
            return control.Resources.GetColor(Resource.Color.sideBarPrefilledAnswerText);
        }
    }
}