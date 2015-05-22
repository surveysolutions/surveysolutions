using System;
using System.Globalization;
using Android.Provider;
using Cirrious.CrossCore.Converters;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class QuestionLayoutStyleBackgroudConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionStateStyle.NonAnswered:
                    return Resource.Drawable.question_background_nonanswered;

                case QuestionStateStyle.Invalid:
                    return Resource.Drawable.question_background_invalid;

                case QuestionStateStyle.Answered:
                    return Resource.Drawable.question_background_answered;
                
                default:
                    return Resource.Drawable.question_background_answered;
            }
        }
    }
}