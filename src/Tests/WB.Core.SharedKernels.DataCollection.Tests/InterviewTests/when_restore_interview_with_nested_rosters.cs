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
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_restore_interview_with_nested_rosters : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            nestedRosterGroupId = Guid.Parse("22222222222222222222222222222222");


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

            interviewSynchronizationDto =new InterviewSynchronizationDto(interview.EventSourceId, InterviewStatus.RejectedBySupervisor, null, userId, questionnaireId,
                    questionnaire.Version,
                    new AnsweredQuestionSynchronizationDto[0],
                    new HashSet<InterviewItemId>(),
                    new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), null,
                    new Dictionary<InterviewItemId, RosterSynchronizationDto[]>
                    {
                        {
                            new InterviewItemId(rosterGroupId, new decimal[] {}),
                            new[]
                            {
                                new RosterSynchronizationDto(rosterGroupId, new decimal[] {}, 1, null, string.Empty),
                            }
                        },
                        {
                            new InterviewItemId(nestedRosterGroupId, new decimal[] {1}),
                            new[]
                            {
                                new RosterSynchronizationDto(rosterGroupId, new decimal[] {1}, 1, null, string.Empty),
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
        };

        Because of = () => {
                               interview.SynchronizeInterview(userId, interviewSynchronizationDto);
                               interviewState = interview.CreateSnapshot();
        };

        It should_raise_InterviewSynchronized_event = () =>
            eventContext.ShouldContainEvent<InterviewSynchronized>(@event
                => @event.InterviewData==interviewSynchronizationDto);

        It should_contains_2_roster_instances_at_interview_state = () =>
            interviewState.RosterGroupInstanceIds.Count.ShouldEqual(2);

        It should_contain_first_roster_instance = () =>
            interviewState.RosterGroupInstanceIds["11111111111111111111111111111111[]"].ShouldEqual(new DistinctDecimalList(new decimal[]{1}));

        It should_contain_nested_roster_instance = () =>
            interviewState.RosterGroupInstanceIds["22222222222222222222222222222222[1]"].ShouldEqual(new DistinctDecimalList(new decimal[] { 1 }));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterGroupId;
        private static Guid nestedRosterGroupId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static InterviewState interviewState;
    }
}
