using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ButtonRosterTitleBinding : BindingWrapper<Button, RosterItemViewModel>
    {
        public ButtonRosterTitleBinding(Button androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(Button androidControl, RosterItemViewModel value)
        {
            if (value == null) return;

            var rosterTitle = string.Format("{0} - {1}", value.QuestionnaireRosterTitle, value.InterviewRosterTitle);

            var span = new SpannableString(rosterTitle);

            span.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), value.QuestionnaireRosterTitle.Length, rosterTitle.Length, SpanTypes.ExclusiveExclusive);

            androidControl.TextFormatted = span;
        }
    }
}