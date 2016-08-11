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
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Ignore("C#, KP-4391 Interview reevalution")]
    internal class when_answering_expression_supporting_question_and_interview_restored_after_synch_and_contains_answer_on_not_linked_multioption_question_and_questionnaire_and_multioption_should_be_disabled_by_answer_throws_when_getting_linked_to_question_for_multioption_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            answeredQuestionId = Guid.Parse("11111111111111111111111111111111");

            multyOptionAnsweredQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaireMock = Mock.Get(Mock.Of<IQuestionnaire>(_
                => _.Version == 1

                && _.HasQuestion(multyOptionAnsweredQuestionId) == true
                && _.GetQuestionType(multyOptionAnsweredQuestionId) == QuestionType.MultyOption
                && _.GetMultiSelectAnswerOptionsAsValues(multyOptionAnsweredQuestionId) == new decimal[] { 1, 2, 3 }
                && _.IsQuestionLinked(multyOptionAnsweredQuestionId) == false

                && _.HasQuestion(answeredQuestionId) == true
                && _.GetQuestionType(answeredQuestionId) == QuestionType.Numeric
               // && _.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(answeredQuestionId) == new[] { multyOptionAnsweredQuestionId }
            ));

            questionnaireMock
                .Setup(x => x.GetQuestionReferencedByLinkedQuestion(multyOptionAnsweredQuestionId))
                .Throws(new QuestionnaireException("not a linked"));

           // var expressionProcessor = new Mock<IExpressionProcessor>();
            
            //Setup.InstanceToMockedServiceLocator<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>(expressionProcessor.Object);

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaireMock.Object);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.SynchronizeInterview(userId,
                Create.Entity.InterviewSynchronizationDto(interviewId: interview.EventSourceId,
                    status: InterviewStatus.RejectedBySupervisor,
                    userId: userId,
                    questionnaireId: questionnaireId,
                    questionnaireVersion: questionnaireMock.Object.Version,
                    answers:
                        new[]
                        {
                            new AnsweredQuestionSynchronizationDto(multyOptionAnsweredQuestionId, new decimal[0],
                                new decimal[] {1}, string.Empty)
                        },
                    wasCompleted: true
                    ));


            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                interview.AnswerNumericRealQuestion(userId, answeredQuestionId, new decimal[] { }, DateTime.Now, 5));

        It should_not_fail = () =>
            exception.ShouldEqual(null);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid multyOptionAnsweredQuestionId;
        private static Guid answeredQuestionId;
        private static Mock<IQuestionnaire> questionnaireMock;
        private static Exception exception;
    }
}
