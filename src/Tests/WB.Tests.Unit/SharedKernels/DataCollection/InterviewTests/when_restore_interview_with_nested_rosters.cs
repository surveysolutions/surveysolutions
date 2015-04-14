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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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
            interviewExpressionStateMock = new Mock<IInterviewExpressionStateV2>();
            interviewExpressionStateMock.Setup(
                x => x.AddRoster(Moq.It.IsAny<Guid>(), Moq.It.IsAny<decimal[]>(), Moq.It.IsAny<decimal>(), Moq.It.IsAny<int?>()))
                .Callback<Guid, decimal[], decimal, int?>(
                    (id, vector, rosterId, sort) =>
                    {
                        rosterAddIndex[id] = callOrder;
                        callOrder++;
                    });

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => 
                   _.HasGroup(rosterGroupId) == true
                && _.GetRosterLevelForGroup(rosterGroupId) == 1
                && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new Guid[] { rosterGroupId }

                 && _.HasGroup(nestedRosterGroupId) == true
                && _.GetRosterLevelForGroup(nestedRosterGroupId) == 2
                && _.GetRostersFromTopToSpecifiedGroup(nestedRosterGroupId) == new Guid[] { rosterGroupId, nestedRosterGroupId }
                );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            SetupInstanceToMockedServiceLocator(Mock.Of<IInterviewExpressionStatePrototypeProvider>(_ => _.GetExpressionState(questionnaireId, Moq.It.IsAny<long>()) == interviewExpressionStateMock.Object));

            interviewSynchronizationDto =new InterviewSynchronizationDto(interview.EventSourceId, InterviewStatus.RejectedBySupervisor, null, userId, questionnaireId,
                    questionnaire.Version,
                    new AnsweredQuestionSynchronizationDto[0],
                    new HashSet<InterviewItemId>(),
                    new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), null,
                    new Dictionary<InterviewItemId, RosterSynchronizationDto[]>
                    {
                        {
                            new InterviewItemId(nestedRosterGroupId, new decimal[] {1}),
                            new[]
                            {
                                new RosterSynchronizationDto(nestedRosterGroupId, new decimal[] {1}, 1, null, string.Empty),
                            }
                        },
                        {
                            new InterviewItemId(rosterGroupId, new decimal[] {}),
                            new[]
                            {
                                new RosterSynchronizationDto(rosterGroupId, new decimal[] {}, 1, null, string.Empty),
                            }
                        }
                    },
                    true);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
            SetupInstanceToMockedServiceLocator<IInterviewExpressionStatePrototypeProvider>(
               CreateInterviewExpressionStateProviderStub());
        };

        Because of = () => interview.SynchronizeInterview(userId, interviewSynchronizationDto);

        It should_raise_InterviewSynchronized_event = () =>
            eventContext.ShouldContainEvent<InterviewSynchronized>(@event
                => @event.InterviewData==interviewSynchronizationDto);

        It should_add_first_roster_to_expression_state = () =>
            interviewExpressionStateMock.Verify(x => x.AddRoster(rosterGroupId, new decimal[0], 1, null), Times.Once);

        It should_add_nested_roster_to_expression_state = () =>
            interviewExpressionStateMock.Verify(x => x.AddRoster(nestedRosterGroupId, new decimal[]{1}, 1, null), Times.Once);

        It should_add_parent_roster_first = () =>
            rosterAddIndex[rosterGroupId].ShouldEqual(0);

        It should_add_nested_roster_second = () =>
           rosterAddIndex[nestedRosterGroupId].ShouldEqual(1);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterGroupId;
        private static Guid nestedRosterGroupId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Dictionary<Guid, int> rosterAddIndex = new Dictionary<Guid, int>(); 
        private static Mock<IInterviewExpressionStateV2> interviewExpressionStateMock;
    }
}
