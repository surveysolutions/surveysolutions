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
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using it = Moq.It;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_propagated_group_is_conditionally_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            propagatedGroupId = Guid.Parse("11111111111111111111111111111111");
           
            questionWhichIsForcesPropagationId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(questionWhichIsForcesPropagationId) == true
                                                        && _.GetQuestionType(questionWhichIsForcesPropagationId) == QuestionType.AutoPropagate
                                                        && _.IsQuestionInteger(questionWhichIsForcesPropagationId) == true
                                                        && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIsForcesPropagationId) == new Guid[] { propagatedGroupId }

                                                        && _.HasGroup(propagatedGroupId) == true
                                                        && _.GetRosterLevelForGroup(propagatedGroupId)==1
                                                        && _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(propagatedGroupId) == new Guid[] { propagatedGroupId }
                                                        && _.GetParentRosterGroupsAndGroupItselfIfRosterStartingFromTop(propagatedGroupId) == new Guid[] { propagatedGroupId });

            var expressionProcessor = new Mock<IExpressionProcessor>();
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(it.IsAny<string>(), it.IsAny<Func<string, object>>())).Returns(false);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

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
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIsForcesPropagationId, new int[] { }, DateTime.Now, 1);


        It should_raise_GroupDisabled_event_with_GroupId_equal_to_propagatedGroupId_with_disablement_condition = () =>
            eventContext.ShouldContainEvent<GroupDisabled>(@event
                => @event.GroupId == propagatedGroupId);
       
        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIsForcesPropagationId;
        private static Guid propagatedGroupId;
    }
}
