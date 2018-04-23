using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class QuestionEditorStyleBackgroundConverter : MvxValueConverter<QuestionStateStyle, int>
    {
        protected override int Convert(QuestionStateStyle value, Type targetType, object parameter, CultureInfo culture)
        {
            var trimBottomCorners = parameter as bool? ?? false;
            switch (value)
            {
                case QuestionStateStyle.NonAnsweredEnabled:
                    return trimBottomCorners 
                        ? Resource.Drawable.question_text_question_nonanswered_no_bottom_corners 
                        : Resource.Drawable.question_text_question_nonanswered;

                case QuestionStateStyle.NonAnsweredDisabled:
                    return trimBottomCorners 
                        ? Resource.Drawable.question_text_question_nonanswered_disabled_no_bottom_corners 
                        : Resource.Drawable.question_text_question_nonanswered_disabled;

                case QuestionStateStyle.InvalidEnabled:
                    return trimBottomCorners 
                        ? Resource.Drawable.question_text_question_invalid_no_bottom_corners 
                        : Resource.Drawable.question_text_question_invalid;

                case QuestionStateStyle.InvalidDisabled:
                    return trimBottomCorners 
                        ? Resource.Drawable.question_text_question_invalid_disabled_no_bottom_corners 
                        : Resource.Drawable.question_text_question_invalid_disabled;

                case QuestionStateStyle.AnsweredEnabled:
                    return trimBottomCorners 
                        ? Resource.Drawable.question_text_question_no_bottom_corners 
                        : Resource.Drawable.question_text_question;

                case QuestionStateStyle.AnsweredDisabled:
                    return trimBottomCorners 
                        ? Resource.Drawable.question_text_question_disabled_no_bottom_corners 
                        : Resource.Drawable.question_text_question_disabled;

                default:
                    return Resource.Drawable.question_text_question;
            }
        }
    }
}