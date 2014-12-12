using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_autopropagatable_question_which_triggers_group_propagation_with_mandatory_question_inside_disabled_group : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            propagatedGroupId = Guid.Parse("11111111111111111111111111111111");

            mandatoryQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionWhichIsForcesPropagationId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(questionWhichIsForcesPropagationId) == true
                                                        && _.GetQuestionType(questionWhichIsForcesPropagationId) == QuestionType.AutoPropagate
                                                        && _.IsQuestionInteger(questionWhichIsForcesPropagationId) == true
                                                        && _.GetRosterLevelForQuestion(mandatoryQuestionId) == 1

                                                        && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIsForcesPropagationId) == new Guid[] { propagatedGroupId }
                                                        && _.HasGroup(propagatedGroupId) == true
                                                        && _.GetRosterLevelForGroup(propagatedGroupId) == 1
                                                        && _.GetRostersFromTopToSpecifiedGroup(propagatedGroupId) == new Guid[] { propagatedGroupId }
                                                        && _.GetAllParentGroupsForQuestion(mandatoryQuestionId) == new Guid[] { propagatedGroupId }
                                                        && _.GetRostersFromTopToSpecifiedQuestion(mandatoryQuestionId) == new Guid[] { propagatedGroupId }
                                                        );

            //var expressionProcessor = new Mock<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>();
//            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
//                .Returns(false);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            //Mock.Get(ServiceLocator.Current)
            // .Setup(locator => locator.GetInstance<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>())
            // .Returns(expressionProcessor.Object);

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIsForcesPropagationId, new decimal[] { }, DateTime.Now, 1);


        private It should_not_raise_AnswerDeclaredValid_event = () =>
             eventContext.ShouldNotContainEvent<AnswersDeclaredValid>(@event
                 => @event.Questions.Any(x => x.Id == mandatoryQuestionId));

        private It should_not_raise_AnswerDeclaredInvalid_event = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredInvalid>(@event
                => @event.Questions.Any(x => x.Id == mandatoryQuestionId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIsForcesPropagationId;
        private static Guid propagatedGroupId;
        private static Guid mandatoryQuestionId;
    }
}
