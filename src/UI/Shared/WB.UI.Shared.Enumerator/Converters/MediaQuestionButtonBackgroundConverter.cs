using System;
using System.Globalization;
using MvvmCross.Platform.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class MediaQuestionButtonBackgroundConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionStateStyle.NonAnswered:
                    return Resource.Drawable.question_input_button;

                case QuestionStateStyle.Invalid:
                case QuestionStateStyle.Answered:
                    return Resource.Drawable.question_input_button_answered;

                default:
                    return Resource.Drawable.question_input_button;
            }
        }
    }
}