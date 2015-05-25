using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class QuestionEditorStyleBackgroundConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionStateStyle.NonAnswered:
                    return Resource.Drawable.question_text_question_nonanswered;

                case QuestionStateStyle.Invalid:
                    return Resource.Drawable.question_text_question_invalid;

                case QuestionStateStyle.Answered:
                    return Resource.Drawable.question_text_question;

                default:
                    return Resource.Drawable.question_text_question;
            }
        }
    }
}