using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_restoring_interview_state_from_sync_package_and_disabled_or_valid_or_invalid_static_texts_are_null : StatefulInterviewTestsContext
    {
        [Test] public void should_not_throw () {
            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId));

            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId, userId: userId, answers: new AnsweredQuestionSynchronizationDto[0]);
            Assert.DoesNotThrow(() =>
                interview.Synchronize(Create.Command.Synchronize(userId, synchronizationDto)));
        }

        private static InterviewSynchronizationDto synchronizationDto;
        private static StatefulInterview interview;
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        private static Exception exception;
    }
}
