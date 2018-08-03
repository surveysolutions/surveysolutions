using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class QuestionOptionBackgroundConverter :  MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionStateStyle.NonAnsweredEnabled:
                    return Resource.Drawable.question_option_background_nonanswered;

                case QuestionStateStyle.InvalidEnabled:
                    return Resource.Drawable.question_option_background_invalid;

                case QuestionStateStyle.AnsweredEnabled:
                    return Resource.Drawable.question_option_background_answered;

                case QuestionStateStyle.AnsweredProtected: 
                case QuestionStateStyle.MaxAnswersCountReached:
                    return Resource.Drawable.question_option_background_protected;

                default:
                    return Resource.Drawable.question_text_question;
            }
        }
    }
}
