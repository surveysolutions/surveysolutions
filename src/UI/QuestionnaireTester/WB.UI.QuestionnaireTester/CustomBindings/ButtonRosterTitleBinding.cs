using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ButtonRosterTitleBinding : BaseBinding<Button, RosterStateViewModel>
    {
        private ISubstitutionService SubstitutionService
        {
            get { return Mvx.Resolve<ISubstitutionService>(); }
        }

        public ButtonRosterTitleBinding(Button androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(Button control, RosterStateViewModel value)
        {
            if (value == null) return;

            var rosterTitle = string.Format("{0} - {1}", value.GroupState.Title, string.IsNullOrEmpty(value.RosterTitle) ? SubstitutionService.DefaultSubstitutionText : value.RosterTitle);

            var span = new SpannableString(rosterTitle);

            span.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), value.GroupState.Title.Length, rosterTitle.Length, SpanTypes.ExclusiveExclusive);

            control.TextFormatted = span;
        }
    }
}