using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    //[Ignore("http://issues.wbcapi.org/youtrack/issue/KP-5249")]
    internal class when_answer_on_multy_option_question_increases_roster_size_and_disable_rows_at_the_same_time : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            numericQuestionInsideRoster = Guid.Parse("33333333333333333333333333333333");
            rosterGroupId = Guid.Parse("11111111111111111111111111111111");
            multyOptionRosterSizeId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: multyOptionRosterSizeId, answers: new decimal[] { 1, 2, 3 }),
                Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: multyOptionRosterSizeId, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: numericQuestionInsideRoster),
                })
            }));


            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            var enablementQueue = new Queue<EnablementChanges>();
            // init .ctor call
            enablementQueue.Enqueue(Create.Entity.EnablementChanges());
            // add first row to roster
            enablementQueue.Enqueue(Create.Entity.EnablementChanges());
            // add second row to roster with disable
            enablementQueue.Enqueue(
                Create.Entity.EnablementChanges(
                    groupsToBeDisabled: new List <Identity>
                    {
                        new Identity(rosterGroupId, new decimal[] {1}),
                        new Identity(rosterGroupId, new decimal[] {2})
                    },
                    questionsToBeDisabled: new List <Identity> {
                        new Identity(numericQuestionInsideRoster, new decimal[] {1}),
                        new Identity(numericQuestionInsideRoster, new decimal[] {2})
                    }));
            //remove first row
            enablementQueue.Enqueue(Create.Entity.EnablementChanges());
            //remove second row
            enablementQueue.Enqueue(Create.Entity.EnablementChanges());
            //return first row
            enablementQueue.Enqueue(
                Create.Entity.EnablementChanges(
                    questionsToBeEnabled: new List<Identity>{
                      // if uncomment this line the test  become succesefull                                                                                                                         
                        new Identity(numericQuestionInsideRoster, new decimal[] {1}),
                        new Identity(numericQuestionInsideRoster, new decimal[] {2})
                    }));
            //answer on numeric question
            enablementQueue.Enqueue(Create.Entity.EnablementChanges());

            interviewExpressionState = new Mock<ILatestInterviewExpressionState>();
            interviewExpressionState.Setup(x => x.ProcessEnablementConditions()).Returns(enablementQueue.Dequeue);
            interviewExpressionState.Setup(x => x.Clone()).Returns(interviewExpressionState.Object);

            interviewExpressionState.Setup(x => x.GetStructuralChanges()).Returns(new StructuralChanges());

            var interviewExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_
                => _.GetExpressionState(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == interviewExpressionState.Object);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: interviewExpressionStatePrototypeProvider);

            interview.SynchronizeInterview(
                userId,
                Create.Entity.InterviewSynchronizationDto(interviewId: interview.EventSourceId,
                    status: InterviewStatus.InterviewerAssigned,
                    userId: userId,
                    questionnaireId: questionnaireId,
                    questionnaireVersion: questionnaire.Version,
                    wasCompleted: true
                    ));

            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1 });
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1, 2 });
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 2 });
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[0]);
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1});

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, numericQuestionInsideRoster, new decimal[]{1}, DateTime.Now, 18);

        It should_raise_NumericIntegerQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>(@event
                => @event.Answer==18 && @event.QuestionId==numericQuestionInsideRoster);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid multyOptionRosterSizeId;
        private static Guid numericQuestionInsideRoster;
        private static Guid rosterGroupId;
        private static Mock<ILatestInterviewExpressionState> interviewExpressionState;
    }
}
