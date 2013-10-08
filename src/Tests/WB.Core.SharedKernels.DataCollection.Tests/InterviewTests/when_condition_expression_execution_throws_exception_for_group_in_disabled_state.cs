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
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_condition_expression_execution_throws_exception_for_group_in_disabled_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            answeringQuestionId = Guid.Parse("22222222222222222222222222222222");
            conditionallyDisabledGroupId = Guid.Parse("33333333333333333333333333333333");


            var questionaire = Mock.Of<IQuestionnaire>(_=>
                                                        _.HasGroup(conditionallyDisabledGroupId) == true
                                                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(conditionallyDisabledGroupId) == new Guid[] { answeringQuestionId }

                                                        && _.HasQuestion(answeringQuestionId) == true
                                                        && _.GetQuestionType(answeringQuestionId) == QuestionType.Numeric
                                                        && _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(answeringQuestionId) == new Guid[] { conditionallyDisabledGroupId });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor.Object);

            interview = CreateInterview(questionnaireId: questionnaireId);

            //give an answer which would disable conditionallyDisabledQuestion at first
            interview.AnswerNumericIntegerQuestion(userId, answeringQuestionId, new int[] { }, DateTime.Now, 5);

            //setup expression processor throw exception
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>())).Throws(new Exception());

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, answeringQuestionId, new int[] { }, DateTime.Now, 0);

        It should_not_raise_GroupDisabled_event_with_GroupId_equal_to_conditionallyDisabledGroupId = () =>
            eventContext.ShouldNotContainEvent<GroupDisabled>(@event
              => @event.GroupId == conditionallyDisabledGroupId);

        It should_raise_GroupEnabled_event_with_GroupId_equal_to_conditionallyDisabledGroupId = () =>
            eventContext.ShouldContainEvent<GroupEnabled>(@event
             => @event.GroupId == conditionallyDisabledGroupId);

        private static EventContext eventContext;
        private static Guid conditionallyDisabledGroupId;
        private static Interview interview;
        private static Guid userId;
        private static Guid answeringQuestionId;
    }
}
