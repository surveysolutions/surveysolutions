using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class QuestionCornerOptionBackgroundConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTop = (bool)parameter;
            switch (value)
            {
                case QuestionStateStyle.NonAnsweredEnabled:
                    return isTop ? Resource.Drawable.question_option_top_border_nonanswered : Resource.Drawable.question_option_bottom_border_nonanswered;

                case QuestionStateStyle.InvalidEnabled:
                    return isTop ? Resource.Drawable.question_option_top_border_invalid : Resource.Drawable.question_option_bottom_border_invalid;

                case QuestionStateStyle.AnsweredEnabled:
                    return Resource.Drawable.question_option_corner_answered;

                default:
                    return Resource.Drawable.question_text_question;
            }
        }
    }
}