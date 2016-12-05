using System;
using System.Globalization;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_answering_prefilled_timestamp_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.DateTimeQuestion(questionId: questionId, isTimestamp: true, preFilled: true)
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerDateTimeQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, new DateTime(2011, 03, 08, 14, 29, 33));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_DateTimeQuestionAnswered_event = () =>
            interview
                .GetAnswerAsString(Create.Entity.Identity(questionId, RosterVector.Empty), new CultureInfo("ru-RU"))
                .ShouldEqual("08.03.2011 14:29:33");

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");

    }
}