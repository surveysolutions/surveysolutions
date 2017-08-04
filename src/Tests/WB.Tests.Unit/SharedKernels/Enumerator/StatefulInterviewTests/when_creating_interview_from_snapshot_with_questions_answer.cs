using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_creating_interview_from_snapshot_with_questions_answer : StatefulInterviewTestsContext
    {
        private Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid integerQuestionId = Guid.Parse("00000000000000000000000000000001");
            RosterVector rosterVector = Create.Entity.RosterVector(1m, 0m);
            fixedRosterIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), Create.Entity.RosterVector(1));
            fixedNestedRosterIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"), Create.Entity.RosterVector(1,0));
            questionIdentity = Create.Identity(integerQuestionId, rosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: Create.Entity.FixedRoster(
                    rosterId: fixedRosterIdentity.Id,
                    fixedTitles: new[] {new FixedRosterTitle(1, "fixed")},
                    children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(
                            rosterId: fixedNestedRosterIdentity.Id,
                            fixedTitles: new[] {new FixedRosterTitle(0, "nested fixed")},
                            children: new IComposite[]
                            {
                                Create.Entity.NumericIntegerQuestion(questionIdentity.Id)
                            })
                    }));

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.QuestionnaireIdentity(questionnaireId), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository, shouldBeInitialized: false
                );

            var answersDtos = new[]
            {
                CreateAnsweredQuestionSynchronizationDto(integerQuestionId, rosterVector, 1),
            };

            var rosterInstances = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>
            {
                {
                    Create.Entity.InterviewItemId(fixedRosterIdentity.Id, fixedRosterIdentity.RosterVector),
                    new[] {Create.Entity.RosterSynchronizationDto(fixedRosterIdentity.Id, fixedRosterIdentity.RosterVector.Shrink(), fixedRosterIdentity.RosterVector.Last())}
                },
                {
                    Create.Entity.InterviewItemId(fixedNestedRosterIdentity.Id, fixedNestedRosterIdentity.RosterVector),
                    new[] {Create.Entity.RosterSynchronizationDto(fixedNestedRosterIdentity.Id, fixedNestedRosterIdentity.RosterVector.Shrink(), fixedNestedRosterIdentity.RosterVector.Last())}
                }
            };
            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId,
                userId: userId, answers: answersDtos, rosterGroupInstances: rosterInstances);

            command = Create.Command.CreateInterviewFromSnapshot(userId, synchronizationDto);
        };

        Because of = () => interview.CreateInterviewFromSnapshot(command);

        It should_return_empty_failed_condition_messages = () => 
            interview.GetFailedValidationMessages(questionIdentity, "Error").Count().ShouldEqual(0);

        It should_create_roster_instance = () => 
            interview.GetRoster(fixedRosterIdentity).ShouldNotBeNull();

        It should_create_nested_roster_instance = () => 
            interview.GetRoster(fixedNestedRosterIdentity).ShouldNotBeNull();

        It should_set_answer = () => 
            interview.GetQuestion(questionIdentity).AsInteger.GetAnswer().Value.ShouldEqual(1);

        static InterviewSynchronizationDto synchronizationDto;
        static StatefulInterview interview;
        static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        static Identity questionIdentity;
        static Identity fixedRosterIdentity;
        static Identity fixedNestedRosterIdentity;
        private static CreateInterviewFromSnapshotCommand command;
    }
}