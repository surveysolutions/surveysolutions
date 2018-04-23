using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class QuestionLayoutStyleBackgroundConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionStateStyle.NonAnsweredEnabled:
                    return Resource.Drawable.question_background_nonanswered;

                case QuestionStateStyle.InvalidEnabled:
                    return Resource.Drawable.question_background_invalid;

                case QuestionStateStyle.AnsweredEnabled:
                    return Resource.Drawable.question_background_answered;
                
                default:
                    return Resource.Drawable.question_background_answered;
            }
        }
    }
}