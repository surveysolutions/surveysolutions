using System;
using System.Globalization;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_answering_prefilled_timestamp_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.DateTimeQuestion(questionId: questionId, isTimestamp: true, preFilled: true)
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();

            BecauseOf();
        }

        private void BecauseOf() =>
            interview.AnswerDateTimeQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, dateAnswer);

        [OneTimeTearDown]
        public void TearmDown() 
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_DateTimeQuestionAnswered_event () =>
            interview
                .GetAnswerAsString(Create.Entity.Identity(questionId, RosterVector.Empty), new CultureInfo("ru-RU"))
                .Should().Be(dateAnswer.ToString(DateTimeFormat.DateWithTimeFormat));

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static DateTime dateAnswer = new DateTime(2011, 03, 08, 14, 29, 33);
    }
}
