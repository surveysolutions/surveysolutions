using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_which_is_roster_size_question : with_event_context
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.YesNoQuestion(questionId: questionId, answers: new[]
                {
                    option_NotChanged_FromNothing_ToNothing,
                    option_NotChanged_FromNo______ToNo_____,
                    option_NotChanged_FromYes_____ToYes____,
                    option__Selected__FromNothing_ToYes____,
                    option__Selected__FromNo______ToYes____,
                    option_Deselected_FromNothing_ToNo_____,
                    option_Deselected_FromNo______ToNothing,
                    option_Deselected_FromYes_____ToNo_____,
                    option_Deselected_FromYes_____ToNothing,
                }),
                Create.Entity.Roster(rosterId: rosterId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: questionId),
            });

            interview = Setup.InterviewForQuestionnaireDocument(questionnaireDocument);
            interview.Apply(Create.Event.YesNoQuestionAnswered(questionId: questionId, answeredOptions: new []
            {
                Create.Entity.AnsweredYesNoOption(value: option_NotChanged_FromNo______ToNo_____, answer: false),
                Create.Entity.AnsweredYesNoOption(value: option__Selected__FromNo______ToYes____, answer: false),
                Create.Entity.AnsweredYesNoOption(value: option_Deselected_FromNo______ToNothing, answer: false),
                Create.Entity.AnsweredYesNoOption(value: option_NotChanged_FromYes_____ToYes____, answer: true),
                Create.Entity.AnsweredYesNoOption(value: option_Deselected_FromYes_____ToNo_____, answer: true),
                Create.Entity.AnsweredYesNoOption(value: option_Deselected_FromYes_____ToNothing, answer: true),
            }));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId: rosterId, fullRosterVectors: new decimal[][]
            {
                Create.Entity.RosterVector(option_NotChanged_FromYes_____ToYes____),
                Create.Entity.RosterVector(option_Deselected_FromYes_____ToNo_____),
                Create.Entity.RosterVector(option_Deselected_FromYes_____ToNothing),
            }));

            command = Create.Command.AnswerYesNoQuestion(
                questionId: questionId,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(value: option_NotChanged_FromNo______ToNo_____, answer: false),
                    Create.Entity.AnsweredYesNoOption(value: option_Deselected_FromNothing_ToNo_____, answer: false),
                    Create.Entity.AnsweredYesNoOption(value: option_Deselected_FromYes_____ToNo_____, answer: false),
                    Create.Entity.AnsweredYesNoOption(value: option_NotChanged_FromYes_____ToYes____, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: option__Selected__FromNothing_ToYes____, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: option__Selected__FromNo______ToYes____, answer: true),
                });
        };

        Because of = () =>
            interview.AnswerYesNoQuestion(command);

        It should_raise_YesNoQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<YesNoQuestionAnswered>();

        It should_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>();

        It should_raise_RosterInstancesAdded_event_with_Instances_containing_only_selected_options_which_were_not_selected_before = () =>
            eventContext.GetSingleEvent<RosterInstancesAdded>()
                .Instances.Select(rosterInstance => rosterInstance.RosterInstanceId).ShouldContainOnly(
                    option__Selected__FromNothing_ToYes____,
                    option__Selected__FromNo______ToYes____);

        It should_raise_RosterInstancesAdded_event_with_SortIndexes = () =>
        {
            var addedRosters = eventContext.GetSingleEvent<RosterInstancesAdded>().Instances;
            addedRosters.Select(x => x.SortIndex).ShouldContainOnly(1, 2);
        };

        It should_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>();

        It should_raise_RosterInstancesRemoved_event_with_Instances_containing_only_deselected_options_which_were_selected_before = () =>
            eventContext.GetSingleEvent<RosterInstancesRemoved>()
                .Instances.Select(rosterInstance => rosterInstance.RosterInstanceId).ShouldContainOnly(
                    option_Deselected_FromYes_____ToNo_____,
                    option_Deselected_FromYes_____ToNothing);

        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static int option_NotChanged_FromNothing_ToNothing = 1;
        private static int option_NotChanged_FromNo______ToNo_____ = 2;
        private static int option_NotChanged_FromYes_____ToYes____ = 3;
        private static int option__Selected__FromNothing_ToYes____ = 4;
        private static int option__Selected__FromNo______ToYes____ = 5;
        private static int option_Deselected_FromNothing_ToNo_____ = 6;
        private static int option_Deselected_FromNo______ToNothing = 7;
        private static int option_Deselected_FromYes_____ToNo_____ = 8;
        private static int option_Deselected_FromYes_____ToNothing = 9;
        private static Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}