using System;
using System.Collections.Generic;

using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.Views.Adapters
{
    public class QuestionEditorViewAdapter : IQuestionEditorViewAdapter
    {
        private static readonly Dictionary<Type, int> QuestionEditorsTemplates = new Dictionary<Type, int>
        {
            {typeof (SingleOptionQuestionViewModel), Resource.Layout.interview_single_option_question },
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_date_question },
            {typeof (MaskedTextQuestionViewModel), Resource.Layout.interview_text_question },
            {typeof (RealNumericQuestionViewModel), Resource.Layout.interview_decimal_question },
            {typeof (MultimediaQuestionViewModel), Resource.Layout.interview_image_question },
            {typeof (IntegerNumericQuestionViewModel), Resource.Layout.interview_integer_question },
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_multi_option_question },
            {typeof (LinkedSingleOptionQuestionViewModel), Resource.Layout.interview_linked_single_question },
            {typeof (LinkedMultiOptionQuestionViewModel), Resource.Layout.interview_linked_multi_question },
            {typeof (TextListQuestionViewModel), Resource.Layout.interview_text_list_question },
            {typeof (QrBarcodeQuestionViewModel), Resource.Layout.interview_qr_question },
            {typeof (GpsCoordinatesQuestionViewModel), Resource.Layout.interview_gps_question },
        };

        public int GetItemViewType(object viewModel)
        {
            Type viewModelType = viewModel.GetType();

            if (QuestionEditorsTemplates.ContainsKey(viewModelType))
            {
                return QuestionEditorsTemplates[viewModelType];
            }
            return 0;
        }
    }
}