﻿using System;
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
    [Ignore("C#, KP-4385 Misc Corner Cases")]
    internal class when_answering_question_which_enable_group_with_mandatory_question : InterviewTestsContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            mandatoryQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");
            answeringQuestionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetAllMandatoryQuestions() == new Guid[] { mandatoryQuestionId }
                   //&& _.GetAllGroupsWithNotEmptyCustomEnablementConditions() == new[] { groupId }
                   //&& _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(answeringQuestionId) == new[] { groupId }
                   //&& _.GetUnderlyingMandatoryQuestions(groupId)==new[] { mandatoryQuestionId }
                   && _.GetAllParentGroupsForQuestion(mandatoryQuestionId) == new[] { groupId }
                   && _.HasQuestion(answeringQuestionId)==true
                   && _.IsQuestionMandatory(mandatoryQuestionId)==true
                   && _.GetQuestionType(answeringQuestionId)==QuestionType.Numeric
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
                .Returns(true);

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericRealQuestion(userId, answeringQuestionId, new decimal[] { }, DateTime.Now, 0);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_not_raise_AnswersDeclaredValid_event = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredValid>(@event
                => @event.Questions.Any(question => question.Id == mandatoryQuestionId));

        It should_raise_AnswersDeclaredInvalid_event = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>(@event
                => @event.Questions.Any(question => question.Id == mandatoryQuestionId));

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Guid mandatoryQuestionId;
        private static Guid answeringQuestionId;
        private static Interview interview;
        private static Guid groupId;
    }
}
