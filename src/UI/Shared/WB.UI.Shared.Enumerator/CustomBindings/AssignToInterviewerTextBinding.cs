using System;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class AssignToInterviewerTextBinding : BaseBinding<TextView, InterviewerToSelectViewModel>
    {
        public AssignToInterviewerTextBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, InterviewerToSelectViewModel value)
        {
            var fullNameFormatted = string.IsNullOrEmpty(value.FullName) ? "" : $"{value.FullName} - ";

            var text = $"{fullNameFormatted}{value.Login} - {value.InterviewsCount}";

            var formattedText = new SpannableString(text);

            formattedText.SetSpan(new StyleSpan(TypefaceStyle.Bold), fullNameFormatted.Length,
                fullNameFormatted.Length + value.Login.Length, SpanTypes.ExclusiveExclusive);

            control.TextFormatted = formattedText;
        }
    }
}
