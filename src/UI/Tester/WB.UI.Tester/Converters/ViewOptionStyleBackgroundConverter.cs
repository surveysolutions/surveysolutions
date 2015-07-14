using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace WB.UI.Tester.Converters
{
    public class ViewOptionStyleBackgroundConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionStateStyle.NonAnswered:
                    return Resource.Drawable.question_option_background;

                case QuestionStateStyle.Invalid:
                case QuestionStateStyle.Answered:
                    return Resource.Drawable.question_option_background_answered;

                default:
                    return Resource.Drawable.question_option_background;
            }
        }
    }
}