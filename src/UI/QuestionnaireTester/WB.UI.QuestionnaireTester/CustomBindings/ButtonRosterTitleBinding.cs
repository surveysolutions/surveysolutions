using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ButtonRosterTitleBinding : BindingWrapper<Button, RosterStateViewModel>
    {
        public ButtonRosterTitleBinding(Button androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(Button androidControl, RosterStateViewModel value)
        {
            if (value == null) return;

            var rosterTitle = string.Format("{0} - {1}", value.GroupState.Title, value.RosterTitle);

            var span = new SpannableString(rosterTitle);

            span.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), value.GroupState.Title.Length, rosterTitle.Length, SpanTypes.ExclusiveExclusive);

            androidControl.TextFormatted = span;
        }
    }
}