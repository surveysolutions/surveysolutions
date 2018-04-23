using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class MediaQuestionButtonBackgroundConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionStateStyle.NonAnsweredEnabled:
                    return Resource.Drawable.question_input_button;

                case QuestionStateStyle.InvalidEnabled:
                case QuestionStateStyle.AnsweredEnabled:
                    return Resource.Drawable.question_input_button_answered;

                default:
                    return Resource.Drawable.question_input_button;
            }
        }
    }
}