using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfQuestionTypeConverterTests
{
    public class PdfQuestionTypeConverterTestsContext
    {
        protected static PdfQuestionTypeConverter CreatePdfQuestionTypeConverter()
        {
            return new PdfQuestionTypeConverter();
        }

        protected static PdfQuestionType[] GetAllPdfQuestionTypes()
        {
            return (PdfQuestionType[]) Enum.GetValues(typeof (PdfQuestionType));
        }

        protected static QuestionType[] GetAllQuestionTypes()
        {
            var type = typeof(QuestionType);
            var questionTypesNames = Enum.GetNames(typeof (QuestionType));
            return questionTypesNames
                .Where(qt => type.GetMember(qt.ToString())[0].GetCustomAttributes(typeof(ObsoleteAttribute), false).Length == 0)
                .Select(qn => (QuestionType)Enum.Parse(typeof(QuestionType), qn))
                .ToArray();
        }

        protected static IQuestion CreateQuestion(QuestionType questionType)
        {
            return Mock.Of<IQuestion>(q => q.QuestionType == questionType);
        }
    }
}