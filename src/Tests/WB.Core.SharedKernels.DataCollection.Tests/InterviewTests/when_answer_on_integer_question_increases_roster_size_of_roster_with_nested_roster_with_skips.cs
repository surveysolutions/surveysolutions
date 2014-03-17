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
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("uningnore after KP-3219 fix")]
    internal class when_answer_on_integer_question_increases_roster_size_of_roster_with_nested_roster_with_skips : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");
            questionWhichIncreasesNestedRosterSizeId = Guid.Parse("31111111111111111111111111111111");

            var questionnaire = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(questionWhichIncreasesRosterSizeId) == true
                                                        && _.GetQuestionType(questionWhichIncreasesRosterSizeId) == QuestionType.Numeric
                                                        && _.IsQuestionInteger(questionWhichIncreasesRosterSizeId) == true
                                                        && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesRosterSizeId) == new[] { parentRosterGroupId }

                                                        && _.HasQuestion(questionWhichIncreasesNestedRosterSizeId) == true
                                                        && _.GetQuestionType(questionWhichIncreasesNestedRosterSizeId) == QuestionType.Numeric
                                                        && _.IsQuestionInteger(questionWhichIncreasesNestedRosterSizeId) == true
                                                        && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesNestedRosterSizeId) == new[] { nestedRosterGroupId }
                                                        && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesNestedRosterSizeId) == new Guid[0]

                                                        && _.HasGroup(nestedRosterGroupId) == true
                                                        && _.GetRosterLevelForGroup(nestedRosterGroupId) == 2
                                                        && _.GetRosterLevelForGroup(parentRosterGroupId) == 1
                                                        && _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(nestedRosterGroupId) == new[] { parentRosterGroupId, nestedRosterGroupId }
                                                        && _.GetRostersFromTopToSpecifiedGroup(nestedRosterGroupId) == new[] { parentRosterGroupId, nestedRosterGroupId }
                                                        && _.GetRostersFromTopToSpecifiedGroup(parentRosterGroupId) == new[] { parentRosterGroupId }
                                                        && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesRosterSizeId) == new Guid[0]);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new NumericIntegerQuestionAnswered(userId, questionWhichIncreasesNestedRosterSizeId, new decimal[0],
                DateTime.Now, 1));
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        It should_raise_RosterRowAdded_event_for_first_row = () =>
          eventContext.ShouldContainEvent<RosterRowAdded>(@event
              => @event.GroupId == parentRosterGroupId && @event.RosterInstanceId == 0 && @event.OuterRosterVector.Length == 0);

        It should_raise_RosterRowAdded_event_for_first_nested_row = () =>
          eventContext.ShouldContainEvent<RosterRowAdded>(@event
              => @event.GroupId == nestedRosterGroupId && @event.RosterInstanceId == 0 && @event.OuterRosterVector.Length == 1 && @event.OuterRosterVector[0] == 0);

        It should_not_raise_RosterRowRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterRowRemoved>(@event
                => @event.GroupId == nestedRosterGroupId);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid questionWhichIncreasesNestedRosterSizeId;
        private static Guid nestedRosterGroupId;
        private static Guid parentRosterGroupId;
    }
}
