using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("KP-8159")]
    internal class when_synchronizing_interview_with_fixed_rosters: StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            substitutedQuestionId = Guid.Parse("00000000000000000000000000000001");
            rosterTitle = "item1";
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id : questionnaireId,
                children: Create.Entity.FixedRoster(
                    rosterId: rosterId, 
                    fixedRosterTitles: new[] {new FixedRosterTitle(0, rosterTitle) },
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: substitutedQuestionId,
                            text: "uses %rostertitle%")
                    }));

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.QuestionnaireIdentity(questionnaireId), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, 
                questionnaireRepository: questionnaireRepository, shouldApplyOnClientCreatedEvent: false
                );

            var rosterSynchronizationDtoses = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();
            var rosters = new List<RosterSynchronizationDto> {Create.Entity.RosterSynchronizationDto(rosterId, RosterVector.Empty, 0, rosterTitle: rosterTitle)};
            rosterSynchronizationDtoses.Add(Create.Entity.InterviewItemId(rosterId, Create.Entity.RosterVector(0)), rosters.ToArray());

            syncDto = Create.Entity.InterviewSynchronizationDto(questionnaireId, rosterGroupInstances: rosterSynchronizationDtoses);

            eventContext = new EventContext();
        };

        Because of = () => interview.RestoreInterviewStateFromSyncPackage(Guid.Empty, syncDto);

        It should_recalculate_roster_titles = () => interview.GetTitleText(Identity.Create(substitutedQuestionId, Create.Entity.RosterVector(0))).ShouldEqual($"uses {rosterTitle}");

        Cleanup things = () => eventContext.Dispose();

        static StatefulInterview interview;
        static EventContext eventContext;
        static InterviewSynchronizationDto syncDto;
        static Guid substitutedQuestionId;
        static string rosterTitle;
    }
}