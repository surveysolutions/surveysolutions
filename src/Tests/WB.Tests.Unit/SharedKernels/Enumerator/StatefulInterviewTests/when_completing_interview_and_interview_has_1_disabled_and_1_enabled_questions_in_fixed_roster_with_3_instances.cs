using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_completing_interview_and_interview_has_1_disabled_and_1_enabled_questions_in_fixed_roster_with_3_instances
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid rosterId = Guid.NewGuid();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.FixedRoster(rosterId: rosterId, fixedTitles: new[] {
                        Create.Entity.FixedTitle(1, "First"),
                        Create.Entity.FixedTitle(2, "Second"),
                        Create.Entity.FixedTitle(3, "Third"),
                    },
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: disabledQuestionId, variable: "q1"),
                        Create.Entity.TextQuestion(questionId: enabledQuestionId, variable: "q2"),
                    }),
            });

            interview = SetUp.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));
            

            interview.Apply(Create.Event.QuestionsDisabled(new []
            {
                Create.Entity.Identity(disabledQuestionId, Create.Entity.RosterVector(1)),
                Create.Entity.Identity(disabledQuestionId, Create.Entity.RosterVector(2)),
                Create.Entity.Identity(disabledQuestionId, Create.Entity.RosterVector(3)),
            }));

            eventContext = new EventContext();
            BecauseOf();
        }

        private void BecauseOf() =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow, null);

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>();

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event_with_id_of_disabled_question_and_roster_vectors_of_fixed_roster () =>
            eventContext.GetEvent<QuestionsDisabled>().Questions.Should().BeEquivalentTo(new[]
            {
                Create.Entity.Identity(disabledQuestionId, Create.Entity.RosterVector(1)),
                Create.Entity.Identity(disabledQuestionId, Create.Entity.RosterVector(2)),
                Create.Entity.Identity(disabledQuestionId, Create.Entity.RosterVector(3)),
            });
        

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Guid disabledQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid enabledQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }
}
