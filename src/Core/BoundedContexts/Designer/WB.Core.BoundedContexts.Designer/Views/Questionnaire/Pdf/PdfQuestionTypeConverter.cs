using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionTypeConverter
    {
        public PdfQuestionType GetPdfQuestionTypeFromQuestion(IQuestion question)
        {
            if (question.QuestionType == QuestionType.MultyOption)
            {
                var multyOptionsQuestion = question as MultyOptionsQuestion;
                if (multyOptionsQuestion != null && multyOptionsQuestion.YesNoView)
                    return PdfQuestionType.YesNo;
            }

            return GetPdfQuestionTypeByQuestionType(question.QuestionType);
        }

        public PdfQuestionType GetPdfQuestionTypeFromFullQuestionDataEvent(FullQuestionDataEvent @event)
        {
            if (@event.QuestionType == QuestionType.MultyOption)
            {
                if (@event.YesNoView == true)
                    return PdfQuestionType.YesNo;
            }

            return GetPdfQuestionTypeByQuestionType(@event.QuestionType);
        }

        private PdfQuestionType GetPdfQuestionTypeByQuestionType(QuestionType questionType)
        {
            switch (questionType)
            {
                case QuestionType.DateTime: return PdfQuestionType.DateTime;
                case QuestionType.GpsCoordinates: return PdfQuestionType.GpsCoordinates;
                case QuestionType.Multimedia: return PdfQuestionType.Multimedia;
                case QuestionType.MultyOption: return PdfQuestionType.MultyOption;
                case QuestionType.Numeric: return PdfQuestionType.Numeric;
                case QuestionType.QRBarcode: return PdfQuestionType.QRBarcode;
                case QuestionType.SingleOption: return PdfQuestionType.SingleOption;
                case QuestionType.Text: return PdfQuestionType.Text;
                case QuestionType.TextList: return PdfQuestionType.TextList;
                // there are some legacy questionnaires on solutions that have yes/no question.
                case QuestionType.YesNo: return PdfQuestionType.YesNo;
                default:
                    throw new ArgumentException(nameof(questionType));
            }
        }
    }
}