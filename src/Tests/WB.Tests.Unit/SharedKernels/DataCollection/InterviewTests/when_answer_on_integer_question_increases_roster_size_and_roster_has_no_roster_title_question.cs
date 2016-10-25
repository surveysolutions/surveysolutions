using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_increases_roster_size_and_roster_has_no_roster_title_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var sectionId = Guid.Parse("FFFFFFFF0000000000000000DDDDDDDD");
            rosterGroupId = Guid.Parse("11111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, questionnaireId,
                  Create.Entity.NumericIntegerQuestion(questionWhichIncreasesRosterSizeId, variable: "num"),
                  Create.Entity.Roster(rosterGroupId, variable: "r",
                      rosterSizeSourceType: RosterSizeSourceType.Question,
                      rosterSizeQuestionId: questionWhichIncreasesRosterSizeId,
                      rosterTitleQuestionId: null)));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[] { }, DateTime.Now, 3);

        It should_raise_RosterInstancesAdded_event_for_that_roster = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.All(instance => instance.GroupId == rosterGroupId));

        It should_raise_RosterInstancesTitleChanged_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>();

        It should_raise_RosterInstancesTitleChanged_event_with_numeric_ordered_titles_starting_from_1 = () =>
            eventContext.GetSingleEvent<RosterInstancesTitleChanged>()
                .ChangedInstances.Select(instance => instance.Title).ShouldContainOnly(new[] { "1", "2", "3" });

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>();

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid rosterGroupId;
    }
}