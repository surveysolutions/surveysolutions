using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_synchronizing_interview_with_no_failing_validation_conditions : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid integerQuestionId = Guid.Parse("00000000000000000000000000000001");
            RosterVector rosterVector = Create.Other.RosterVector(1m, 0m);

            IPlainQuestionnaireRepository questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.GetAnswerType(integerQuestionId) == AnswerType.Integer);

            interview = Create.Other.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            var answersDtos = new[]
            {
                CreateAnsweredQuestionSynchronizationDto(integerQuestionId, rosterVector, 1),
            };

            questionIdentity = new Identity(integerQuestionId, RosterVector.Empty);
            synchronizationDto = Create.Other.InterviewSynchronizationDto(questionnaireId: questionnaireId,
                answers: answersDtos);
        };

        Because of = () => interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto);

        It should_return_empty_failed_condition_indexes = () => interview.GetFailedValidationConditions(questionIdentity).Count.ShouldEqual(0);

        static InterviewSynchronizationDto synchronizationDto;
        static StatefulInterview interview;
        static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        static Identity questionIdentity;
    }
}