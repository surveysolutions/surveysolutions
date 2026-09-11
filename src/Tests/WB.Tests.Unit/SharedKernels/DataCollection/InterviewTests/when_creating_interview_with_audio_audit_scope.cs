using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    internal class when_creating_interview_with_audio_audit_scope : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            eventContext = new EventContext();

            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            questionnaireVersion = 18;
            audioAuditScope = new[] { "household", "members_roster" };

            var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.Version == questionnaireVersion);

            this.command = Create.Command.CreateInterview(questionnaireId: this.questionnaireId,
                questionnaireVersion: this.questionnaireVersion,
                responsibleSupervisorId: this.responsibleSupervisorId,
                answersToFeaturedQuestions: new List<InterviewAnswer>(),
                userId: this.userId,
                audioAuditScope: this.audioAuditScope);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

            //Act
            interview.CreateInterview(command);
        }

        [Test]
        public void should_provide_copied_audio_audit_scope_in_InterviewCreated_event() =>
            eventContext.GetEvent<InterviewCreated>()
                .AudioAuditScope.Should().BeEquivalentTo(audioAuditScope);

        [OneTimeTearDown]
        public void TearDown()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private EventContext eventContext;
        private Guid questionnaireId;
        private long questionnaireVersion;
        private Guid userId;
        private Guid responsibleSupervisorId;
        private string[] audioAuditScope;
        private Interview interview;
        private CreateInterview command;
    }
}
