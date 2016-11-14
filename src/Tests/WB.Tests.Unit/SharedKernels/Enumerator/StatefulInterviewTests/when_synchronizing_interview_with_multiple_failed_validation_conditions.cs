using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_synchronizing_interview_with_multiple_failed_validation_conditions : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid integerQuestionId = Guid.Parse("00000000000000000000000000000001");
            RosterVector rosterVector = Create.Entity.RosterVector(1m, 0m);

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.GetAnswerType(integerQuestionId) == AnswerType.Integer);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            var answersDtos = new[]
            {
                CreateAnsweredQuestionSynchronizationDto(integerQuestionId, rosterVector, 1),
            };

            questionIdentity = new Identity(integerQuestionId, RosterVector.Empty);
            FailedCondtionsFromSync = new List<FailedValidationCondition>
            {
                new FailedValidationCondition(0),
                new FailedValidationCondition(2)
            };
            var failedValidationConditions = new Dictionary<Identity, IList<FailedValidationCondition>>
            {
                {
                    questionIdentity,
                    FailedCondtionsFromSync
                }
            };

            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId, 
                answers: answersDtos,
                failedValidationConditions: failedValidationConditions);
        };

        Because of = () => interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto);

        It should_return_failed_condition_indexes = () => interview.GetFailedValidationMessages(questionIdentity).Count.ShouldEqual(2);

        It should_return_failed_condition_indexes_from_events = () => 
            interview.GetFailedValidationMessages(questionIdentity).ShouldEachConformTo(c => FailedCondtionsFromSync.Contains(c));

        static InterviewSynchronizationDto synchronizationDto;
        static StatefulInterview interview;
        static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
        static Identity questionIdentity;
        static List<FailedValidationCondition> FailedCondtionsFromSync;
    }
}