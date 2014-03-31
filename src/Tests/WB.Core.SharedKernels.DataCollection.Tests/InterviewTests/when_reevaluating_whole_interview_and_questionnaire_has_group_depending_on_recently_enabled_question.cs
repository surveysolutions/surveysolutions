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
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_reevaluating_whole_interview_and_questionnaire_has_group_depending_on_recently_enabled_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            conditionallyDependentGroupId = Guid.Parse("33333333333333333333333333333333");
            conditionallyRecentlyEnabledQuestionId = Guid.Parse("43333333333333333333333333333333");

            var questionaire = Mock.Of<IQuestionnaire>(_ =>
                _.GetAllGroupsWithNotEmptyCustomEnablementConditions() == new Guid[] { conditionallyDependentGroupId }
                    &&
                    _.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(conditionallyDependentGroupId) ==
                        new [] { new QuestionIdAndVariableName(conditionallyRecentlyEnabledQuestionId, "q1") }
                    && _.GetCustomEnablementConditionForGroup(conditionallyDependentGroupId)=="[q1]==2"
                    && _.GetCustomEnablementConditionForQuestion(conditionallyRecentlyEnabledQuestionId)=="2==2"
                    && _.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(conditionallyRecentlyEnabledQuestionId) == new[] { new QuestionIdAndVariableName(Guid.NewGuid(), "qsomething") });

            var expressionProcessor = new WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services.ExpressionProcessor();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.Apply(new GroupDisabled(conditionallyDependentGroupId, new decimal[0]));

            interview.Apply(new QuestionDisabled(conditionallyRecentlyEnabledQuestionId, new decimal[0]));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, conditionallyRecentlyEnabledQuestionId, new decimal[0],
                DateTime.Now, 2));
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_not_raise_GroupsDisabled_event_with_GroupId_equal_to_conditionallyDependentGroupId = () =>
            eventContext.ShouldNotContainEvent<GroupsDisabled>(@event
                => @event.Groups.Any(group => group.Id == conditionallyDependentGroupId));

        It should_raise_GroupsEnabled_event_with_GroupId_equal_to_conditionallyDependentGroupId = () =>
           eventContext.ShouldContainEvent<GroupsEnabled>(@event
                => @event.Groups.Any(group => group.Id == conditionallyDependentGroupId));

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static Interview interview;
        private static Guid conditionallyDependentGroupId;
        private static Guid conditionallyRecentlyEnabledQuestionId;
    }
}
