﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
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
    internal class when_reevaluating_interview_with_mandatory_question_inside_recently_enabled_group : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            conditionallyRecentlyEnabledMandatoryQuestionId = Guid.Parse("33333333333333333333333333333333");
            conditionallyRecentlyEnabledGroupId = Guid.Parse("43333333333333333333333333333333");

            var questionaire = Mock.Of<IQuestionnaire>(_ =>
                                                        _.GetAllGroupsWithNotEmptyCustomEnablementConditions() == new Guid[] { conditionallyRecentlyEnabledGroupId }
                                                        && _.GetAllMandatoryQuestions() == new Guid[] { conditionallyRecentlyEnabledMandatoryQuestionId }
                                                        && _.GetAllParentGroupsForQuestion(conditionallyRecentlyEnabledMandatoryQuestionId) == new Guid[] { conditionallyRecentlyEnabledGroupId });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            //setup expression processor throw exception
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
                .Returns(true);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor.Object);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.Apply(new GroupDisabled(conditionallyRecentlyEnabledGroupId, new decimal[0]));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, conditionallyRecentlyEnabledMandatoryQuestionId, new decimal[0],
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

        It should_not_raise_AnswersDeclaredInvalid_event_with_QuestionId_equal_to_conditionallyRecentlyMandatoryQuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredInvalid>(@event
                => @event.Questions.Any(question => question.Id == conditionallyRecentlyEnabledMandatoryQuestionId));

        It should_raise_AnswersDeclaredValid_event_with_QuestionId_equal_to_conditionallyRecentlyMandatoryQuestionId = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>(@event
                => @event.Questions.Any(question => question.Id == conditionallyRecentlyEnabledMandatoryQuestionId));

        It should_raise_GroupsEnabled_event_with_GroupId_equal_to_conditionallyRecentlyEnabledGroupId = () =>
            eventContext.ShouldContainEvent<GroupsEnabled>(@event
                => @event.Groups.Any(group => group.Id == conditionallyRecentlyEnabledGroupId));

        It should_not_raise_GroupsDisabled_event_with_GroupId_equal_to_conditionallyRecentlyEnabledGroupId = () =>
            eventContext.ShouldNotContainEvent<GroupsDisabled>(@event
                => @event.Groups.Any(group => group.Id == conditionallyRecentlyEnabledGroupId));

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static Interview interview;
        private static Guid conditionallyRecentlyEnabledMandatoryQuestionId;
        private static Guid conditionallyRecentlyEnabledGroupId;
    }
}
