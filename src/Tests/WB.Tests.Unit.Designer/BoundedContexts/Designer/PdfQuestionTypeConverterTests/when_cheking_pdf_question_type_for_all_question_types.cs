using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using NHibernate.Util;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionTypeConverterTests
{
    public class when_cheking_pdf_question_type_for_all_question_types : PdfQuestionTypeConverterTestsContext
    {
        Establish context = () =>
        {
            pdfQuestionTypes = GetAllPdfQuestionTypes();
            var questionTypes = GetAllQuestionTypes();
            questions = questionTypes.Select(CreateQuestion).ToArray();

            converter = CreatePdfQuestionTypeConverter();
        };

        Because of = () =>
            resultPdfQuestionTypes = questions.Select(q => converter.GetPdfQuestionTypeFromQuestion(q)).ToArray();

        It should_exists_all_pdf_question_types_for_all_questions = () =>
        {
            pdfQuestionTypes.ShouldContain(resultPdfQuestionTypes);
        };

        private static PdfQuestionTypeConverter converter;
        private static PdfQuestionType[] pdfQuestionTypes;
        private static PdfQuestionType[] resultPdfQuestionTypes;
        private static IQuestion[] questions;
    }
}