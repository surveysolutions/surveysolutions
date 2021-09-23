using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_creating_interview_from_snapshot_when_question_type_is_wrong : StatefulInterviewTestsContext
    {
        [Test]
        public void should_throw_exception()
        {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid integerQuestionId = Guid.Parse("00000000000000000000000000000001");
            RosterVector rosterVector = Create.Entity.RosterVector();
            questionIdentity = Create.Identity(integerQuestionId, rosterVector);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: Create.Entity.NumericIntegerQuestion(questionIdentity.Id));

            IQuestionnaireStorage questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.QuestionnaireIdentity(questionnaireId), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository, shouldBeInitialized: false
                );

            var answersDtos = new[]
            {
                CreateAnsweredQuestionSynchronizationDto(integerQuestionId, rosterVector, "text"),
            };

            var rosterInstances = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();
            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId,
                userId: userId, answers: answersDtos, rosterGroupInstances: rosterInstances);

            command = Create.Command.CreateInterviewFromSnapshot(userId, synchronizationDto);

            Assert.That((TestDelegate)(() => interview.CreateInterviewFromSnapshot(command)), Throws.Exception);
        }

        static InterviewSynchronizationDto synchronizationDto;
        static StatefulInterview interview;
        static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        static Identity questionIdentity;
        private static CreateInterviewFromSnapshotCommand command;
    }
}
