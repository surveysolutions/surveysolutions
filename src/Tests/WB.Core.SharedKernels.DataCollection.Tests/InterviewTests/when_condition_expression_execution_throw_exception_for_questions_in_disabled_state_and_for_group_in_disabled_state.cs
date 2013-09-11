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
    internal class when_condition_expression_execution_throw_exception_for_questions_in_disabled_state_and_for_group_in_disabled_state : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            conditionallyDisabledQuestionId = Guid.Parse("11111111111111111111111111111111");
            zeroValueQuestionId = Guid.Parse("22222222222222222222222222222222");
            conditionallyDisabledGroupId = Guid.Parse("33333333333333333333333333333333");

            var conditionWithDivision = string.Format("1/[{0}] > 5 ", zeroValueQuestionId);

            var questionaire = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(conditionallyDisabledQuestionId) == true
                                                        && _.GetQuestionType(conditionallyDisabledQuestionId) == QuestionType.Text
                                                        && _.GetCustomEnablementConditionForQuestion(conditionallyDisabledQuestionId) == conditionWithDivision
                                                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(conditionallyDisabledQuestionId) == new Guid[] { zeroValueQuestionId }

                                                        && _.HasGroup(conditionallyDisabledGroupId) == true
                                                        && _.GetCustomEnablementConditionForGroup(conditionallyDisabledGroupId) == conditionWithDivision
                                                        && _.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(conditionallyDisabledGroupId) == new Guid[] { zeroValueQuestionId }

                                                        && _.HasQuestion(zeroValueQuestionId) == true
                                                        && _.GetQuestionType(zeroValueQuestionId) == QuestionType.Numeric
                                                        && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(zeroValueQuestionId) == new Guid[] { conditionallyDisabledQuestionId }
                                                        && _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(zeroValueQuestionId) == new Guid[] { conditionallyDisabledGroupId });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(new ExpressionProcessor());

            interview = CreateInterview(questionnaireId: questionnaireId);

            //give an answer which would disable conditionallyDisabledQuestion at first
            interview.AnswerNumericQuestion(userId, zeroValueQuestionId, new int[] {}, DateTime.Now, 5);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
         interview.AnswerNumericQuestion(userId, zeroValueQuestionId, new int[] { }, DateTime.Now, 0);

        It should_not_raise_QuestionDisabled_event_with_QuestionId_equal_to_conditionallyDisabledQuestionId = () =>
          eventContext.ShouldNotContainEvent<QuestionDisabled>(@event
              => @event.QuestionId == conditionallyDisabledQuestionId );

        It should_raise_QuestionEnabled_event_with_QuestionId_equal_to_conditionallyDisabledQuestionId = () =>
         eventContext.ShouldContainEvent<QuestionEnabled>(@event
             => @event.QuestionId == conditionallyDisabledQuestionId);

        It should_not_raise_GroupDisabled_event_with_QuestionId_equal_to_conditionallyDisabledGroupId = () =>
          eventContext.ShouldNotContainEvent<GroupDisabled>(@event
              => @event.GroupId == conditionallyDisabledGroupId);

        It should_raise_GroupEnabled_event_with_QuestionId_equal_to_conditionallyDisabledGroupId = () =>
         eventContext.ShouldContainEvent<GroupEnabled>(@event
             => @event.GroupId == conditionallyDisabledGroupId);

        private static EventContext eventContext;
        private static Guid conditionallyDisabledQuestionId;
        private static Guid conditionallyDisabledGroupId;
        private static Interview interview;
        private static Guid userId;
        private static Guid zeroValueQuestionId;
    }
}
