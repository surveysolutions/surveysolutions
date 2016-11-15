using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Ignore("KP-8159")]
    internal class when_restore_interview_with_disabled_static_text : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.NewGuid();
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            staticTextId = Guid.Parse("11111111111111111111111111111111");

            interviewExpressionStateMock = new Mock<ILatestInterviewExpressionState>();
            interviewExpressionStateMock.Setup(x => x.Clone()).Returns(interviewExpressionStateMock.Object);
            var structuralChanges = new StructuralChanges();
            interviewExpressionStateMock.Setup(x => x.GetStructuralChanges()).Returns(structuralChanges);

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(
                    Create.Entity.QuestionnaireIdentity(questionnaireId, 1),
                    Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new[]
                    {
                        Create.Entity.StaticText(publicKey: staticTextId)
                    }));

            var interviewExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_
                => _.GetExpressionState(questionnaireId, Moq.It.IsAny<long>()) == interviewExpressionStateMock.Object);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: interviewExpressionStatePrototypeProvider);

            interviewSynchronizationDto =
                Create.Entity.InterviewSynchronizationDto(interviewId: interview.EventSourceId,
                    userId: userId,
                    questionnaireId: questionnaireId,
                    questionnaireVersion: 1,
                    disabledStaticTexts: new List<Identity>() {Create.Entity.Identity(staticTextId, RosterVector.Empty)}
                    );

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => interview.SynchronizeInterview(userId, interviewSynchronizationDto);

        It should_raise_InterviewSynchronized_event = () =>
            eventContext.ShouldContainEvent<InterviewSynchronized>(@event
                => @event.InterviewData == interviewSynchronizationDto);

        It should_disable_static_text_in_expression_state = () =>
            interviewExpressionStateMock.Verify(
                x =>
                    x.DisableStaticTexts(
                        Moq.It.Is<IEnumerable<Identity>>(
                            s => s.Count() == 1 && s.First() == Create.Entity.Identity(staticTextId, RosterVector.Empty))),
                Times.Once);


        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid staticTextId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Mock<ILatestInterviewExpressionState> interviewExpressionStateMock;
    }
}