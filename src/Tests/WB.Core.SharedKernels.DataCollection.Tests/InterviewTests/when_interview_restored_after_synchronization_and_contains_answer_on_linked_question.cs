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
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_interview_restored_after_synchronization_and_contains_answer_on_linked_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            answeredQuestionId = Guid.Parse("11111111111111111111111111111111");

            multyOptionAnsweredQuestionId = Guid.Parse("22222222222222222222222222222222");


            questionnaireMock = new Mock<IQuestionnaire>();
            questionnaireMock.Setup(x => x.Version).Returns(1);
            questionnaireMock.Setup(x => x.HasQuestion(multyOptionAnsweredQuestionId)).Returns(true);
            questionnaireMock.Setup(x => x.GetQuestionType(multyOptionAnsweredQuestionId)).Returns(QuestionType.MultyOption);
            questionnaireMock.Setup(x => x.GetAnswerOptionsAsValues(multyOptionAnsweredQuestionId)).Returns(new decimal[] { 1, 2, 3 });

            questionnaireMock.Setup(x => x.HasQuestion(answeredQuestionId)).Returns(true);
            questionnaireMock.Setup(x => x.GetQuestionType(answeredQuestionId)).Returns(QuestionType.Numeric);

            questionnaireMock.Setup(x => x.GetQuestionLinkedQuestionId(multyOptionAnsweredQuestionId)).Returns((Guid?)null);
            
            questionnaireMock.Setup(x => x.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(answeredQuestionId)).Returns(new Guid[] { multyOptionAnsweredQuestionId });

            var expressionProcessor = new Mock<IExpressionProcessor>();
            
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(Moq.It.IsAny<string>(), Moq.It.IsAny<Func<string, object>>()))
                .Returns(false);
            
            Mock.Get(ServiceLocator.Current)
            .Setup(locator => locator.GetInstance<IExpressionProcessor>())
            .Returns(expressionProcessor.Object);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaireMock.Object);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.SynchronizeInterview(userId,
                new InterviewSynchronizationDto(interview.EventSourceId, InterviewStatus.RejectedBySupervisor, userId, questionnaireId,
                    questionnaireMock.Object.Version,
                    new[] { new AnsweredQuestionSynchronizationDto(multyOptionAnsweredQuestionId, new decimal[0], new decimal[] { 1 }, string.Empty) },
                    new HashSet<InterviewItemId>(),
                    new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), null,
                    new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), true));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericRealQuestion(userId, answeredQuestionId, new decimal[] { }, DateTime.Now, 5);

        private It should_not_call_GetQuestionReferencedByLinkedQuestion_for_multyOptionAnsweredQuestionId = () =>
            questionnaireMock.Verify(x => x.GetQuestionReferencedByLinkedQuestion(multyOptionAnsweredQuestionId), Times.Never());

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid multyOptionAnsweredQuestionId;
        private static Guid answeredQuestionId;
        private static Mock<IQuestionnaire> questionnaireMock;
    }
}
