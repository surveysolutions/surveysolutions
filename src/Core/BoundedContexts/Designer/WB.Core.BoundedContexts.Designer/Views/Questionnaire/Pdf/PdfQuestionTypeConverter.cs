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

            return (PdfQuestionType)question.QuestionType;
        }

        public PdfQuestionType GetPdfQuestionTypeFromFullQuestionDataEvent(FullQuestionDataEvent @event)
        {
            if (@event.QuestionType == QuestionType.MultyOption)
            {
                if (@event.YesNoView == true)
                    return PdfQuestionType.YesNo;
            }

            return (PdfQuestionType)@event.QuestionType;
        }
    }
}