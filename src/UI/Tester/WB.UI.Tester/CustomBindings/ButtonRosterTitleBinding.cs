using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.UI.Tester.CustomBindings
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

            var rosterTitle = this.SubstitutionService.GenerateRosterName(value.GroupState.Title, value.RosterTitle);

            var span = new SpannableString(rosterTitle);
            span.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), value.GroupState.Title.Length, rosterTitle.Length, SpanTypes.ExclusiveExclusive);

            control.TextFormatted = span;
        }
    }
}