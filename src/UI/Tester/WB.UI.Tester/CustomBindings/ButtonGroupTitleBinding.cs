using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Cirrious.CrossCore;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.UI.Tester.CustomBindings
{
    public class ButtonGroupTitleBinding : BaseBinding<Button, GroupViewModel>
    {
        private ISubstitutionService SubstitutionService
        {
            get { return Mvx.Resolve<ISubstitutionService>(); }
        }

        public ButtonGroupTitleBinding(Button androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(Button control, GroupViewModel value)
        {
            if (value == null) return;

            if (string.IsNullOrEmpty(value.RosterTitle))
            {
                control.Text = value.Title;
            }
            else
            {
                var rosterTitle = this.SubstitutionService.GenerateRosterName(value.Title, value.RosterTitle);
                var span = new SpannableString(rosterTitle);
                span.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), value.Title.Length, rosterTitle.Length,
                    SpanTypes.ExclusiveExclusive);

                control.TextFormatted = span;
            }
        }
    }
}