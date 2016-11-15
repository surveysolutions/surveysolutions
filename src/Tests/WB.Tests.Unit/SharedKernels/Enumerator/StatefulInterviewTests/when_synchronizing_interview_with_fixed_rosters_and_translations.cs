using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("KP-8159")]
    internal class when_synchronizing_interview_with_fixed_rosters: StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid substitutedQuestionId = Guid.Parse("00000000000000000000000000000001");
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id : questionnaireId,
                children: Create.Entity.FixedRoster(
                    rosterId: rosterId, 
                    fixedTitles: new[] {"item1"},
                    children: new IComposite[]
                    {
                        Create.Entity.Question(questionId: substitutedQuestionId,
                            title: "uses %rostertitle%")
                    }));

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.QuestionnaireIdentity(questionnaireId), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, 
                questionnaireRepository: questionnaireRepository
                );

            var rosterSynchronizationDtoses = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();
            var rosters = new List<RosterSynchronizationDto> {Create.Entity.RosterSynchronizationDto(rosterId)};
            rosterSynchronizationDtoses.Add(Create.Entity.InterviewItemId(rosterId, Create.Entity.RosterVector()), rosters.ToArray());

            syncDto = Create.Entity.InterviewSynchronizationDto(questionnaireId, rosterGroupInstances: rosterSynchronizationDtoses);

            eventContext = new EventContext();
        };

        Because of = () => interview.RestoreInterviewStateFromSyncPackage(Guid.Empty, syncDto);

        It should_recalculate_roster_titles = () => eventContext.ShouldContainEvent<RosterInstancesTitleChanged>();

        Cleanup things = () => eventContext.Dispose();

        static StatefulInterview interview;
        static EventContext eventContext;
        static InterviewSynchronizationDto syncDto;
    }
}