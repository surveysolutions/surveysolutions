using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_decreases_roster_size_of_roster_with_nested_roster_with_answered_questions_inside_triggered_by_the_same_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            questionFromRosterId = Guid.Parse("32222222222222222222222222222222");
            questionFromNestedRosterId = Guid.Parse("42222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),

                Create.Entity.Roster(rosterId: parentRosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId, children: new IComposite[]
                {
                    Create.Entity.Question(questionId: questionFromRosterId),

                    Create.Entity.Roster(rosterId: nestedRosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId, children: new IComposite[]
                    {
                        Create.Entity.Question(questionId: questionFromNestedRosterId),
                    }),
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);
            interview.Apply(new TextQuestionAnswered(userId, questionFromRosterId, new decimal[] { 0 }, DateTime.Now, "t1"));
            interview.Apply(new TextQuestionAnswered(userId, questionFromNestedRosterId, new decimal[] { 0, 0 }, DateTime.Now, "t2"));
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 0);

        It should_raise_RosterInstancesRemoved_event_for_parent_roster_row_and_for_nested_roster_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Count(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 0)==1
                && @event.Instances.Count(instance => instance.GroupId == nestedRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 }))==1);

        It should_raise_AnswersRemoved_event_for_parent_roster_row_and_for_nested_roster_row = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event
                => @event.Questions.Count(instance => instance.Id == questionFromRosterId && instance.RosterVector.SequenceEqual(new decimal[] { 0 })) == 1
                && @event.Questions.Count(instance => instance.Id == questionFromNestedRosterId && instance.RosterVector.SequenceEqual(new decimal[] { 0, 0 })) == 1);

        It should_not_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == nestedRosterGroupId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid nestedRosterGroupId;
        private static Guid parentRosterGroupId;

        private static Guid questionFromRosterId;
        private static Guid questionFromNestedRosterId;
    }
}
