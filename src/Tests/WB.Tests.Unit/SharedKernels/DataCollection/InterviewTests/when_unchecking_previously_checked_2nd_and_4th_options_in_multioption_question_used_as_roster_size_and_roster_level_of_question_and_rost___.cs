using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_unchecking_previously_checked_2nd_and_4th_options_in_multioption_question_used_as_roster_size_and_roster_level_of_question_and_roster_is_zero : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            emptyRosterVector = new decimal[] { };
            userId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, textAnswers: new []
                {
                    Create.Entity.Answer("option 1", 14m),
                    Create.Entity.Answer("option 2", option2 = 18),
                    Create.Entity.Answer("option 3", option3 = 3),
                    Create.Entity.Answer("option 4", option4 = -1),
                    Create.Entity.Answer("option 5", 2568m),
                }),

                Create.Entity.Roster(rosterId: rosterId, rosterSizeQuestionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerMultipleOptionsQuestion(userId, questionId, emptyRosterVector, DateTime.Now, new[] { option2, option3, option4 });

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerMultipleOptionsQuestion(userId, questionId, emptyRosterVector, DateTime.Now, new[] { option3 });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_MultipleOptionsQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<MultipleOptionsQuestionAnswered>();

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesAdded_event () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>();

        [NUnit.Framework.Test] public void should_raise_RosterInstancesRemoved_event_with_2_instances () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_set_roster_id_to_all_instances_in_RosterInstancesRemoved_event () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances
                .Should().OnlyContain(instance => instance.GroupId == rosterId);

        [NUnit.Framework.Test] public void should_set_empty_outer_roster_vector_to_all_instances_in_RosterInstancesRemoved_event () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances
                .Should().OnlyContain(instance => instance.OuterRosterVector.Length == 0);

        [NUnit.Framework.Test] public void should_set_2nd_and_4th_options_as_roster_instance_ids_in_RosterInstancesRemoved_event () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Select(instance => instance.RosterInstanceId).ToArray()
                .Should().BeEquivalentTo(new[]{ option2, option4 });

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] emptyRosterVector;
        private static int option2;
        private static int option3;
        private static int option4;
        private static Guid rosterId;
    }
}
