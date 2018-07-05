using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_restore_interview_with_nested_rosters : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {


            var questionnaireId = Guid.NewGuid();
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            nestedRosterGroupId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Roster(rosterGroupId, fixedRosterTitles: new [] { Create.Entity.FixedTitle(1, "level 1")}, children: new IComposite[]
                {
                    Create.Entity.Roster(nestedRosterGroupId, fixedRosterTitles: new [] { Create.Entity.FixedTitle(1, "level 2")})
                })
            );

            interview = SetupStatefullInterview(questionnaire);

            interviewSynchronizationDto =
                Create.Entity.InterviewSynchronizationDto(interviewId: interview.EventSourceId,
                    status: InterviewStatus.RejectedBySupervisor,
                    userId: userId,
                    questionnaireId: questionnaireId,
                    questionnaireVersion: 1,
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
                    wasCompleted: true);

            eventContext = new EventContext();

            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() => 
            interview.Synchronize(Create.Command.Synchronize(userId, interviewSynchronizationDto));

        [NUnit.Framework.Test] public void should_raise_InterviewSynchronized_event () =>
            eventContext.ShouldContainEvent<InterviewSynchronized>(@event => @event.InterviewData==interviewSynchronizationDto);

        [NUnit.Framework.Test] public void should_restore_2_rosters_from_the_sync_package () => 
            interview.GetAllInterviewNodes().Count(x => x is InterviewTreeRoster).Should().Be(2);

        private static EventContext eventContext;
        private static StatefulInterview interview;
        private static Guid userId;
        private static Guid rosterGroupId;
        private static Guid nestedRosterGroupId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Mock<ILatestInterviewExpressionState> interviewExpressionStateMock;
    }
}
