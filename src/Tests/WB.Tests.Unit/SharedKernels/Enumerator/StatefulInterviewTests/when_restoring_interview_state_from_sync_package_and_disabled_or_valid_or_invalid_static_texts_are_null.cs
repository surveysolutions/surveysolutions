using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("KP-8159")]
    internal class when_restoring_interview_state_from_sync_package_and_disabled_or_valid_or_invalid_static_texts_are_null : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId));

            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId, userId: userId, answers: new AnsweredQuestionSynchronizationDto[0]);
        };

        Because of = () => exception = Catch.Exception(()=>interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto));

        It should_not_throw_an_exception = () =>
             exception.ShouldBeNull();
        
        private static InterviewSynchronizationDto synchronizationDto;
        private static StatefulInterview interview;
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        private static Exception exception;
    }
}