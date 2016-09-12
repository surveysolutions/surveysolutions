using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_roster_row_with_nested_roster_is_removed : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");
            questionInParentRosterId = Guid.Parse("31111111111111111111111111111111");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),

                Create.Entity.Roster(rosterId: parentRosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId, children: new IComposite[]
                {
                    Create.Entity.Question(questionId: parentRosterGroupId),

                    Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId),
                }),
            }));

            var questionnaire1 = Mock.Of<IQuestionnaire>(_

                => _.HasQuestion(questionWhichIncreasesRosterSizeId) == true
                    && _.GetQuestionType(questionWhichIncreasesRosterSizeId) == QuestionType.Numeric
                    && _.IsQuestionInteger(questionWhichIncreasesRosterSizeId) == true
                    && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesRosterSizeId) == new[] { parentRosterGroupId }
                    && _.GetNestedRostersOfGroupById(parentRosterGroupId) == new[] { rosterGroupId }

                    && _.HasGroup(rosterGroupId) == true
                    && _.GetRosterLevelForGroup(rosterGroupId) == 2
                    && _.GetRosterLevelForGroup(parentRosterGroupId) == 1
                    /*&&
                    _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(rosterGroupId) ==
                        new[] { parentRosterGroupId, rosterGroupId }
                    */
                    && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new[] { parentRosterGroupId, rosterGroupId }
                    && _.GetRostersFromTopToSpecifiedGroup(parentRosterGroupId) == new[] { parentRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesRosterSizeId) == new Guid[0]

                    && _.GetAllUnderlyingQuestions(parentRosterGroupId) == new[] { questionInParentRosterId }
                    && _.GetRosterLevelForQuestion(questionInParentRosterId) == 1
                    && _.GetRosterLevelForEntity(questionInParentRosterId) == 1
                    && _.GetRostersFromTopToSpecifiedQuestion(questionInParentRosterId) == new[] { parentRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedEntity(questionInParentRosterId) == new[] { parentRosterGroupId });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 2));

            interview.Apply(Create.Event.RosterInstancesAdded(parentRosterGroupId, new decimal[0], 0, null));
            interview.Apply(Create.Event.RosterInstancesAdded(parentRosterGroupId, new decimal[0], 1, null));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId, new decimal[] { 0 }, 0, null));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId, new decimal[] { 1 }, 0, null));

            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionInParentRosterId, new decimal[]{0}, DateTime.Now, 2));

            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionInParentRosterId, new decimal[] { 1 }, DateTime.Now, 2));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        It should_not_raise_RosterInstancesRemoved_event_for_first_row = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 0));

        It should_raise_RosterInstancesRemoved_event_for_second_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 1 && instance.OuterRosterVector.Length == 0));

        It should_not_raise_RosterInstancesRemoved_of_nested_roster_event_for_first_row = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 })));

        It should_raise_RosterInstancesRemoved_of_nested_roster_event_for_second_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 1 })));

        It should_not_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId));

        It should_not_raise_AnswersRemoved_event_for_first_row = () =>
            eventContext.ShouldNotContainEvent<AnswersRemoved>(@event
                => @event.Questions.Any(question => question.Id == questionInParentRosterId && question.RosterVector[0] == 0 && question.RosterVector.Length == 1));

        It should_raise_AnswersRemoved_event_for_second_row = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event
                => @event.Questions.Any(question => question.Id == questionInParentRosterId && question.RosterVector[0] == 1 && question.RosterVector.Length == 1));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid rosterGroupId;
        private static Guid questionInParentRosterId;
        private static Guid parentRosterGroupId;
    }
}
