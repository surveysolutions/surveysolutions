using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_reanswering_yesno_question_which_is_ordered_roster_size_question : with_event_context
    {
        [OneTimeSetUp] 
        public void SetUp () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(Id.gA, children: new IComposite[]
            {
                Create.Entity.YesNoQuestion(questionId: Id.g1, answers: new[]{ option_1, option_2, option_3 }, ordered: true),
                Create.Entity.Roster(rosterId: Id.g2, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: Id.g1),
            });

            interview = Setup.StatefulInterview(questionnaireDocument);
            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                questionId: Id.g1,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(value: option_1, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: option_2, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: option_3, answer: true),
                }));

            interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(
                questionId: Id.g1,
                answeredOptions: new[]
                {
                    Create.Entity.AnsweredYesNoOption(value: option_2, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: option_3, answer: true),
                    Create.Entity.AnsweredYesNoOption(value: option_1, answer: true),
                }));
        }

        [Test] 
        public void should_raise_YesNoQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<YesNoQuestionAnswered>();

        [Test] 
        public void should_raise_RosterInstancesAdded_event_where_first_roster_has_id_3_and_Sort_index_0 ()
        {
            var instances = interview.GetRosterInstances(Create.Identity(Id.gA, RosterVector.Empty), Id.g2);
            Assert.That(instances[0].RosterVector.Last(), Is.EqualTo(option_2));
            Assert.That(instances[1].RosterVector.Last(), Is.EqualTo(option_3));
            Assert.That(instances[2].RosterVector.Last(), Is.EqualTo(option_1));
        }


        private static AnswerYesNoQuestion command;
        private static StatefulInterview interview;
        private static int option_1 = 1;
        private static int option_2 = 2;
        private static int option_3 = 3;
    }
}
