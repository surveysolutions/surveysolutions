using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_completing_interview_and_interview_has_3_invalid_and_2_valid_questions_outside_of_rosters
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: invalidQuestion1Id, variable: "q1"),
                Create.Entity.TextQuestion(questionId: invalidQuestion2Id, variable: "q2"),
                Create.Entity.TextQuestion(questionId: invalidQuestion3Id, variable: "q3"),
                Create.Entity.TextQuestion(questionId: validQuestion1Id, variable: "q4"),
                Create.Entity.TextQuestion(questionId: validQuestion2Id, variable: "q5"),
            });

            interview = SetUp.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(Create.Event.AnswersDeclaredInvalid(questions: new []
            {
                Create.Entity.Identity(invalidQuestion1Id, RosterVector.Empty),
                Create.Entity.Identity(invalidQuestion2Id, RosterVector.Empty),
                Create.Entity.Identity(invalidQuestion3Id, RosterVector.Empty),
            }));

            interview.Apply(Create.Event.AnswersDeclaredValid(questions: new []
            {
                Create.Entity.Identity(validQuestion1Id, RosterVector.Empty),
                Create.Entity.Identity(validQuestion2Id, RosterVector.Empty),
            }));

            eventContext = new EventContext();

            BecauseOf();
        }

        private void BecauseOf() =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow, null);

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>();

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_with_ids_of_invalid_questions_and_empty_roster_vectors () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>().Questions.Should().BeEquivalentTo(new[]
            {
                Create.Entity.Identity(invalidQuestion1Id, RosterVector.Empty),
                Create.Entity.Identity(invalidQuestion2Id, RosterVector.Empty),
                Create.Entity.Identity(invalidQuestion3Id, RosterVector.Empty),
            });

        [OneTimeTearDown]
        public void TearDown()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Guid invalidQuestion1Id = Guid.Parse("BBBBBBBBBBBBBBBB1111111111111111");
        private static Guid invalidQuestion2Id = Guid.Parse("BBBBBBBBBBBBBBBB2222222222222222");
        private static Guid invalidQuestion3Id = Guid.Parse("BBBBBBBBBBBBBBBB3333333333333333");
        private static Guid validQuestion1Id = Guid.Parse("CCCCCCCCCCCCCCCC1111111111111111");
        private static Guid validQuestion2Id = Guid.Parse("CCCCCCCCCCCCCCCC2222222222222222");
    }
}
