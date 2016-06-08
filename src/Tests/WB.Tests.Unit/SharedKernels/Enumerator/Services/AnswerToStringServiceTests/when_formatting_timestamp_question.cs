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
    internal class when_formatting_timestamp_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.DateTimeQuestion(questionId: questionId, isTimestamp: true)));
            service = CreateAnswerToStringService();

            answer = Create.Entity.DateTimeAnswer(Create.Entity.Identity(questionId, RosterVector.Empty), answer: new DateTime(2016, 06, 08, 13, 35, 01));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
        };

        Because of = () => stringAnswer = service.AnswerToUIString(questionId, answer, statefulInterview, questionnaire);

        It should_return_formatted_according_to_local_time_string = () => 
            stringAnswer.ShouldEqual(answer.Answer?.ToLocalTime().ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern));

        static IAnswerToStringService service;
        static IQuestionnaire questionnaire;
        static Guid questionId;
        static string stringAnswer;
        static DateTimeAnswer answer;
        static StatefulInterview statefulInterview;
    }
}