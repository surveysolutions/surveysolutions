using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_restore_interview_with_nested_rosters : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.NewGuid();
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            nestedRosterGroupId = Guid.Parse("22222222222222222222222222222222");

            int callOrder = 0;
            interviewExpressionStateMock = new Mock<ILatestInterviewExpressionState>();
            interviewExpressionStateMock.Setup(
                x => x.AddRoster(Moq.It.IsAny<Guid>(), Moq.It.IsAny<decimal[]>(), Moq.It.IsAny<decimal>(), Moq.It.IsAny<int?>()))
                .Callback<Guid, decimal[], decimal, int?>(
                    (id, vector, rosterId, sort) =>
                    {
                        rosterAddIndex[id] = callOrder;
                        callOrder++;
                    });
            interviewExpressionStateMock.Setup(x => x.Clone()).Returns(interviewExpressionStateMock.Object);
            var structuralChanges = new StructuralChanges();
            interviewExpressionStateMock.Setup(x => x.GetStructuralChanges()).Returns(structuralChanges);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Roster(rosterGroupId, fixedRosterTitles: new [] { Create.Entity.FixedRosterTitle(1, "level 1")}, children: new IComposite[]
                {
                    Create.Entity.Roster(nestedRosterGroupId, fixedRosterTitles: new [] { Create.Entity.FixedRosterTitle(1, "level 2")})
                })
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            var interviewExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_
                => _.GetExpressionState(questionnaireId, Moq.It.IsAny<long>()) == interviewExpressionStateMock.Object);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, 
                questionnaireRepository: questionnaireRepository,
                interviewExpressionStatePrototypeProvider: interviewExpressionStatePrototypeProvider,
                shouldApplyOnClientCreatedEvent: false);

            interviewSynchronizationDto =
                Create.Entity.InterviewSynchronizationDto(interviewId: interview.EventSourceId,
                    status: InterviewStatus.RejectedBySupervisor,
                    userId: userId,
                    questionnaireId: questionnaireId,
                    questionnaireVersion: questionnaire.Version,
                    rosterGroupInstances:
                        new Dictionary<InterviewItemId, RosterSynchronizationDto[]>
                        {
                            {
                                new InterviewItemId(nestedRosterGroupId, new decimal[] {1}),
                                new[]
                                {
                                    new RosterSynchronizationDto(nestedRosterGroupId, new decimal[] {1}, 1, null, "level 2"),
                                }
                            },
                            {
                                new InterviewItemId(rosterGroupId, new decimal[] {}),
                                new[]
                                {
                                    new RosterSynchronizationDto(rosterGroupId, new decimal[] {}, 1, null, "level 1"),
                                }
                            }
                        },
                    wasCompleted: true
                    );

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => 
            interview.SynchronizeInterview(userId, interviewSynchronizationDto);

        It should_raise_InterviewSynchronized_event = () =>
            eventContext.ShouldContainEvent<InterviewSynchronized>(@event => @event.InterviewData==interviewSynchronizationDto);

        It should_add_first_roster_to_expression_state = () =>
            interviewExpressionStateMock.Verify(x => x.AddRoster(rosterGroupId, new decimal[0], 1, 0), Times.Once);

        It should_add_nested_roster_to_expression_state = () =>
            interviewExpressionStateMock.Verify(x => x.AddRoster(nestedRosterGroupId, new decimal[]{1}, 1, 0), Times.Once);

        It should_add_parent_roster_first = () =>
            rosterAddIndex[rosterGroupId].ShouldEqual(0);

        It should_add_nested_roster_second = () =>
           rosterAddIndex[nestedRosterGroupId].ShouldEqual(1);

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static Guid userId;
        private static Guid rosterGroupId;
        private static Guid nestedRosterGroupId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Dictionary<Guid, int> rosterAddIndex = new Dictionary<Guid, int>(); 
        private static Mock<ILatestInterviewExpressionState> interviewExpressionStateMock;
    }
}
