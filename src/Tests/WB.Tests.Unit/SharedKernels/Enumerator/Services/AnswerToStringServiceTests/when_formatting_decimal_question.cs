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
    internal class when_formatting_decimal_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            cultureInfo = new CultureInfo("ru-RU");
            cultureChange = new ChangeCurrentCulture(cultureInfo);

            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericRealQuestion(id: questionId, useFomatting: true)));
            service = CreateAnswerToStringService();

            answer = Create.Entity.RealNumericAnswer(Create.Entity.Identity(questionId, RosterVector.Empty), answer: 1234.42256);

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
        };

        Because of = () => stringAnswer = service.AnswerToUIString(questionId, answer, statefulInterview, questionnaire);

        It should_return_formatted_according_to_local_culture_string = () => 
            stringAnswer.ShouldEqual($"1{cultureInfo.NumberFormat.CurrencyGroupSeparator}234,42256");

        static IAnswerToStringService service;
        static IQuestionnaire questionnaire;
        static Guid questionId;
        static string stringAnswer;
        static RealNumericAnswer answer;
        static StatefulInterview statefulInterview;
        static ChangeCurrentCulture cultureChange;

        Cleanup c = () =>
        {
            cultureChange.Dispose();
        };

        static CultureInfo cultureInfo;
    }
}