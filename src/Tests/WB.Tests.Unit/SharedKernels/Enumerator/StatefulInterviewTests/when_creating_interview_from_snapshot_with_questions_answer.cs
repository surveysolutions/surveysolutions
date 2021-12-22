using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_creating_interview_from_snapshot_with_questions_answer : StatefulInterviewTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            Guid questionnaireId = Id.gC;
            Guid integerQuestionId = Id.g10;
            RosterVector rosterVector = Create.Entity.RosterVector(1m, 0m);
            fixedRosterIdentity = Identity.Create(Id.g1, Create.Entity.RosterVector(1));
            fixedNestedRosterIdentity = Identity.Create(Id.g2, Create.Entity.RosterVector(1,0));
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

            IQuestionnaireStorage questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.QuestionnaireIdentity(questionnaireId), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository, shouldBeInitialized: false
                );

            var answersDtos = new[]
            {
                CreateAnsweredQuestionSynchronizationDto(integerQuestionId, rosterVector, 1),
            };

            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId,
                userId: userId, answers: answersDtos);

            command = Create.Command.CreateInterviewFromSnapshot(userId, synchronizationDto);

            Because();
        }

        public void Because()
        {
            this.eventContext = new EventContext();
            interview.CreateInterviewFromSnapshot(command);
        }

        [Test]
        public void It_should_return_empty_failed_condition_messages() => 
            interview.GetFailedValidationMessages(questionIdentity, "Error").Count().Should().Be(0);

        [Test]
        public void It_should_create_roster_instance() => 
            interview.GetRoster(fixedRosterIdentity).Should().NotBeNull();

        [Test]
        public void It_should_create_nested_roster_instance() => 
            interview.GetRoster(fixedNestedRosterIdentity).Should().NotBeNull();

        [Test]
        public void It_should_set_answer() => 
            interview.GetQuestion(questionIdentity).GetAsInterviewTreeIntegerQuestion().GetAnswer().Value.Should().Be(1);

        [Test]
        public void It_should_not_switch_translation() =>
            Assert.IsEmpty(this.eventContext.GetEvents<TranslationSwitched>());

        static InterviewSynchronizationDto synchronizationDto;
        static StatefulInterview interview;
        static readonly Guid userId = Id.g9;
        static Identity questionIdentity;
        static Identity fixedRosterIdentity;
        static Identity fixedNestedRosterIdentity;
        private CreateInterviewFromSnapshotCommand command;
        private EventContext eventContext;
    }
}
