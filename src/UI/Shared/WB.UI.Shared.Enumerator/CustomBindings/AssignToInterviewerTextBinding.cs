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
            var text = $"{(string.IsNullOrEmpty(value.FullName) ? "" : $"{value.FullName} - ")}{value.Login} - {value.InterviewsCount}";

            var formattedText = new SpannableString(text);
            var strartIndexOfUserName = text.IndexOf(value.Login, StringComparison.Ordinal);

            formattedText.SetSpan(new StyleSpan(TypefaceStyle.Bold), strartIndexOfUserName,
                strartIndexOfUserName + value.Login.Length, SpanTypes.ExclusiveExclusive);

            control.TextFormatted = formattedText;
        }
    }
}
