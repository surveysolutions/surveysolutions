using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#")]
    internal class when_answer_on_integer_question_increases_roster_size_of_disabled_roster_with_disabled_nested_roster_with_disabled_questions_inside_triggered_by_the_same_question : InterviewTestsContext
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

            var questionnaire = Mock.Of<IQuestionnaire>(_

                => _.HasQuestion(questionWhichIncreasesRosterSizeId) == true
                    && _.GetQuestionType(questionWhichIncreasesRosterSizeId) == QuestionType.Numeric
                    && _.IsQuestionInteger(questionWhichIncreasesRosterSizeId) == true
                    &&
                    _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesRosterSizeId) ==
                        new[] { parentRosterGroupId, nestedRosterGroupId }

                    && _.HasGroup(nestedRosterGroupId) == true
                    && _.GetRosterLevelForGroup(nestedRosterGroupId) == 2
                    && _.GetRosterLevelForGroup(parentRosterGroupId) == 1
                    &&
                    _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(parentRosterGroupId) ==
                        new[] { parentRosterGroupId, nestedRosterGroupId }
                    &&
                    _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(nestedRosterGroupId) ==
                        new[] { nestedRosterGroupId }

                   && _.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(parentRosterGroupId) == new[] { questionFromRosterId, questionFromNestedRosterId }
                   && _.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(nestedRosterGroupId) == new[] { questionFromNestedRosterId }
                   && _.GetRosterLevelForQuestion(questionFromRosterId)==1
                   && _.GetRosterLevelForQuestion(questionFromNestedRosterId) == 2
                   && _.GetRostersFromTopToSpecifiedQuestion(questionFromRosterId)==new[] { parentRosterGroupId}
                   && _.GetRostersFromTopToSpecifiedQuestion(questionFromNestedRosterId) == new[] { parentRosterGroupId, nestedRosterGroupId }

                    && _.GetRostersFromTopToSpecifiedGroup(nestedRosterGroupId) == new[] { parentRosterGroupId, nestedRosterGroupId}
                    && _.GetRostersFromTopToSpecifiedGroup(parentRosterGroupId) == new[] { parentRosterGroupId }
                    && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesRosterSizeId) == new Guid[0]

                    && _.GetNestedRostersOfGroupById(parentRosterGroupId) == new[] { nestedRosterGroupId }
                    && _.GetRosterSizeQuestion(nestedRosterGroupId) == questionWhichIncreasesRosterSizeId);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            var expressionProcessor = new Mock<IExpressionProcessor>();
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor.Object);

            interview = CreateInterview(questionnaireId: questionnaireId);
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        It should_raise_RosterInstancesAdded_event_for_parent_roster_row_and_for_nested_roster_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Count(instance => instance.GroupId == parentRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.Length == 0)==1
                && @event.Instances.Count(instance => instance.GroupId == nestedRosterGroupId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[] { 0 }))==1);

        It should_raise_GroupsDisabled_event_for_parent_roster_row = () =>
         eventContext.ShouldContainEvent<GroupsDisabled>(@event
             => @event.Groups.Count(instance => instance.Id == nestedRosterGroupId && instance.RosterVector.SequenceEqual(new decimal[] { 0, 0 })) == 1);

        It should_raise_GroupsDisabled_event_for_nested_roster_row = () =>
        eventContext.ShouldContainEvent<GroupsDisabled>(@event
            => @event.Groups.Count(instance => instance.Id == parentRosterGroupId && instance.RosterVector.SequenceEqual(new decimal[] { 0 })) == 1
          );

        It should_raise_QuestionsDisabled_event_for_parent_roster_row = () =>
        eventContext.ShouldContainEvent<QuestionsDisabled>(@event
            => @event.Questions.Count(instance => instance.Id == questionFromRosterId && instance.RosterVector.SequenceEqual(new decimal[] { 0 })) == 1);

        It should_raise_QuestionsDisabled_event_for_nested_roster_row = () =>
        eventContext.ShouldContainEvent<QuestionsDisabled>(@event
            => @event.Questions.Count(instance => instance.Id == questionFromNestedRosterId && instance.RosterVector.SequenceEqual(new decimal[] { 0, 0 })) == 1);

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
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
