using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class IsCurrentUserCommentToColorConverter : MvxValueConverter<CommentState, int>
    {
        protected override int Convert(CommentState value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == CommentState.ResolvedComment)
            {
                return Resource.Color.disabledTextColor;
            }

            return value == CommentState.OwnComment ? Resource.Color.comment_from_interviewer : Resource.Color.comment_from_authority;
        }
    }
}
