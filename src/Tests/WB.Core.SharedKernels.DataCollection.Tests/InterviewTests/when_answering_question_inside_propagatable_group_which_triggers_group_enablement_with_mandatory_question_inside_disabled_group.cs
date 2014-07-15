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
    [Ignore("C#")]
    internal class when_answering_question_inside_propagatable_group_which_triggers_group_enablement_with_mandatory_question_inside_disabled_group : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            propagatedGroupId = Guid.Parse("11111111111111111111111111111111");
            mandatoryQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionWhichIsForcesPropagationId = Guid.Parse("22222222222222222222222222222222");
            answeringQuestionId = Guid.Parse("33333333333333333333333333333333");
            disabledPropagatedGroupId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => 
                   _.HasQuestion(questionWhichIsForcesPropagationId) == true
                   && _.GetQuestionType(questionWhichIsForcesPropagationId) == QuestionType.AutoPropagate
                   && _.IsQuestionInteger(questionWhichIsForcesPropagationId) == true
                   && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIsForcesPropagationId) == new Guid[] { propagatedGroupId, disabledPropagatedGroupId }
                   && _.HasGroup(propagatedGroupId) == true
                   && _.HasGroup(disabledPropagatedGroupId) == true
                   && _.GetRosterLevelForGroup(propagatedGroupId) == 1
                   && _.GetRosterLevelForGroup(disabledPropagatedGroupId) == 1
                   && _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(disabledPropagatedGroupId) == new Guid[] { disabledPropagatedGroupId }
                   && _.GetRostersFromTopToSpecifiedGroup(propagatedGroupId) == new Guid[] { propagatedGroupId }
                   && _.GetRostersFromTopToSpecifiedGroup(disabledPropagatedGroupId) == new Guid[] { disabledPropagatedGroupId }

                   && _.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(answeringQuestionId) == new[] { disabledPropagatedGroupId }
                   && _.GetUnderlyingMandatoryQuestions(disabledPropagatedGroupId) == new[] { mandatoryQuestionId }


                   && _.GetAllParentGroupsForQuestion(mandatoryQuestionId) == new Guid[] { disabledPropagatedGroupId }
                   && _.GetRostersFromTopToSpecifiedQuestion(mandatoryQuestionId) == new Guid[] { disabledPropagatedGroupId }
                   && _.GetUnderlyingMandatoryQuestions(disabledPropagatedGroupId) == new Guid[] { mandatoryQuestionId }

                   
                   && _.IsQuestionMandatory(mandatoryQuestionId) == true
                   && _.GetAllParentGroupsForQuestion(mandatoryQuestionId) == new[] { disabledPropagatedGroupId }
                   && _.GetRosterLevelForQuestion(mandatoryQuestionId)==1

                   && _.HasQuestion(answeringQuestionId) == true
                   && _.GetRostersFromTopToSpecifiedQuestion(answeringQuestionId) == new Guid[] { propagatedGroupId }
                   && _.GetQuestionType(answeringQuestionId) == QuestionType.Numeric
                   && _.GetRosterLevelForQuestion(answeringQuestionId) == 1
                );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                              questionnaire);
            var expressionProcessor = new Mock<IExpressionProcessor>();

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
            Mock.Get(ServiceLocator.Current)
              .Setup(locator => locator.GetInstance<IExpressionProcessor>())
              .Returns(expressionProcessor.Object);

            

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.AnswerNumericIntegerQuestion(userId, questionWhichIsForcesPropagationId, new decimal[] { }, DateTime.Now, 1);

            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
                .Returns(true);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericRealQuestion(userId, answeringQuestionId, new decimal[] { 0 }, DateTime.Now, 0);

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
                => @event.Questions.Any(question
                    => question.Id == mandatoryQuestionId
                    && question.RosterVector.Length == 1
                    && question.RosterVector[0] == 0));

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Guid mandatoryQuestionId;
        private static Guid answeringQuestionId;
        private static Interview interview;
        private static Guid questionWhichIsForcesPropagationId;
        private static Guid propagatedGroupId;
        private static Guid disabledPropagatedGroupId;
    }
}
