using System;
using System.Globalization;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.AnswerToStringServiceTests
{
    internal class when_use_formatting_numeric_integer_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            cultureInfo = new CultureInfo("ru-RU");
            cultureChange = new ChangeCurrentCulture(cultureInfo);
           
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = Create.PlainQuestionnaire(
                Create.QuestionnaireDocumentWithOneChapter(Create.NumericIntegerQuestion(useFormatting: true,
                 id: questionId)));
            service = CreateAnswerToStringService();

            answer = Create.Answers.IntegerNumericAnswer(Create.Identity(questionId, RosterVector.Empty), answer: 1234);

            statefulInterview = Create.StatefulInterview(questionnaire: questionnaire);
        };

        Because of = () => stringAnswer = service.AnswerToUIString(questionId, answer, statefulInterview, questionnaire);

        It should_return_formatted_according_to_local_culture_string = () => stringAnswer.ShouldEqual($"1{cultureInfo.NumberFormat.CurrencyGroupSeparator}234");

        static IStringConverter service;
        static IQuestionnaire questionnaire;
        static Guid questionId;
        static string stringAnswer;
        static IntegerNumericAnswer answer;
        static StatefulInterview statefulInterview;
        static ChangeCurrentCulture cultureChange;

        Cleanup c = () =>
        {
            cultureChange.Dispose();
        };

        private static CultureInfo cultureInfo;
    }
}