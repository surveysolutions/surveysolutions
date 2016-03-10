using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_completing_interview_and_interview_has_1_disabled_and_1_enabled_questions_in_fixed_roster_with_3_instances
    {
        Establish context = () =>
        {
            Guid rosterId = Guid.NewGuid();
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Roster(
                    rosterId: rosterId,
                    fixedRosterTitles: new[]
                    {
                        Create.FixedRosterTitle(1, "First"),
                        Create.FixedRosterTitle(2, "Second"),
                        Create.FixedRosterTitle(3, "Third"),
                    },
                    children: new IComposite[]
                    {
                        Create.TextQuestion(questionId: disabledQuestionId),
                        Create.TextQuestion(questionId: enabledQuestionId),
                    }),
            });

            interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, new []
            {
                Create.RosterVector(1),
                Create.RosterVector(2),
                Create.RosterVector(3),
            }));

            interview.Apply(Create.Event.QuestionsDisabled(new []
            {
                Create.Identity(disabledQuestionId, Create.RosterVector(1)),
                Create.Identity(disabledQuestionId, Create.RosterVector(2)),
                Create.Identity(disabledQuestionId, Create.RosterVector(3)),
            }));

            eventContext = Create.EventContext();
        };

        Because of = () =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        It should_raise_QuestionsDisabled_event = () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>();

        It should_raise_QuestionsDisabled_event_with_id_of_disabled_question_and_roster_vectors_of_fixed_roster = () =>
            eventContext.GetEvent<QuestionsDisabled>().Questions.ShouldContainOnly(new[]
            {
                Create.Identity(disabledQuestionId, Create.RosterVector(1)),
                Create.Identity(disabledQuestionId, Create.RosterVector(2)),
                Create.Identity(disabledQuestionId, Create.RosterVector(3)),
            });

        It should_raise_QuestionsEnabled_event = () =>
            eventContext.ShouldContainEvent<QuestionsEnabled>();

        It should_raise_QuestionsEnabled_event_with_id_of_enabled_question_and_roster_vectors_of_fixed_roster = () =>
            eventContext.GetEvent<QuestionsEnabled>().Questions.ShouldContainOnly(new[]
            {
                Create.Identity(enabledQuestionId, Create.RosterVector(1)),
                Create.Identity(enabledQuestionId, Create.RosterVector(2)),
                Create.Identity(enabledQuestionId, Create.RosterVector(3)),
            });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Guid disabledQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid enabledQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }
}