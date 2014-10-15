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
    internal class when_roster_size_question_of_neted_roster_increase_row_count_but_row_is_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            nestedRosterId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");
            nestedRosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            var parentRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Mock.Of<IQuestionnaire>(_
                =>
                _.HasQuestion(nestedRosterSizeQuestionId) == true
                    && _.GetQuestionType(nestedRosterSizeQuestionId) == QuestionType.Numeric
                    && _.IsQuestionInteger(nestedRosterSizeQuestionId) == true
                    && _.GetRosterGroupsByRosterSizeQuestion(nestedRosterSizeQuestionId) == new[] { nestedRosterId }
                    && _.GetRostersFromTopToSpecifiedQuestion(nestedRosterSizeQuestionId) == new[] { parentRosterId }

                    //&& _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(nestedRosterId) == new[] { nestedRosterId }
                    && _.GetRosterLevelForGroup(nestedRosterId) == 2
                    && _.GetRostersFromTopToSpecifiedGroup(nestedRosterId) == new[] { parentRosterId, nestedRosterId }
                );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                              questionnaire);
            var expressionProcessor = new Mock<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>();

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
            Mock.Get(ServiceLocator.Current)
              .Setup(locator => locator.GetInstance<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>())
              .Returns(expressionProcessor.Object);

            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
                .Returns(false);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.Apply(new RosterRowAdded(parentRosterId, new decimal[0], 0, null));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, nestedRosterSizeQuestionId, new decimal[] {0 }, DateTime.Now, 1);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_not_raise_GroupsEnabled_event = () =>
            eventContext.ShouldNotContainEvent<GroupsEnabled>(@event
                => @event.Groups.Any(group => group.Id == nestedRosterId && group.RosterVector.Length == 2 && group.RosterVector[0] == 0 && group.RosterVector[1] == 0));

        It should_raise_GroupsDisabled_event = () =>
            eventContext.ShouldContainEvent<GroupsDisabled>(@event
                => @event.Groups.Any(group => group.Id == nestedRosterId && group.RosterVector.Length == 2 && group.RosterVector[0] == 0 && group.RosterVector[1] == 0));

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Guid nestedRosterSizeQuestionId;
        private static Interview interview;
        private static Guid nestedRosterId;
    }
}
