using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_yesno_question_which_is_ordered_roster_size_question : with_event_context
    {
        [OneTimeSetUp] public void context () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.YesNoQuestion(questionId: questionId, answers: new[]{ option_1, option_2, option_3 }, ordered: true),
                Create.Entity.Roster(rosterId: rosterId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: questionId),
            });

            interview = Setup.InterviewForQuestionnaireDocument(questionnaireDocument);

            command = Create.Command.AnswerYesNoQuestion(
                questionId: questionId,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(value: option_3, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: option_1, answer: true),
                });
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerYesNoQuestion(command);

        [Test] public void should_raise_YesNoQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<YesNoQuestionAnswered>();

        [Test] public void should_raise_RosterInstancesAdded_event () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>();

        [Test] public void should_raise_RosterInstancesAdded_event_where_first_roster_has_id_3_and_Sort_index_0 ()
        {
            var rosterIdentity = eventContext.GetSingleEvent<RosterInstancesAdded>().Instances.ElementAt(0);
            rosterIdentity.RosterInstanceId.Should().Be(option_3);
            rosterIdentity.SortIndex.Should().Be(0);
        }

        [Test] public void should_raise_RosterInstancesAdded_event_where_second_roster_has_id_1_and_Sort_index_1 () 
        {
            var rosterIdentity = eventContext.GetSingleEvent<RosterInstancesAdded>().Instances.ElementAt(1);
            rosterIdentity.RosterInstanceId.Should().Be(option_1);
            rosterIdentity.SortIndex.Should().Be(1);
        }


        private static AnswerYesNoQuestion command;
        private static Interview interview;
        private static int option_1 = 1;
        private static int option_2 = 2;
        private static int option_3 = 3;
        private static Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}
