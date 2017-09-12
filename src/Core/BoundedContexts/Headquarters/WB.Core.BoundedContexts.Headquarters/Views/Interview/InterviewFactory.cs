using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly IPlainStorageAccessor<InterviewDbEntity> entitiesRepository;
        private readonly IReadSideRepositoryReader<InterviewSummary> summaryRepository;

        public InterviewFactory(IPlainStorageAccessor<InterviewDbEntity> interviewEntitiesRepository, 
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository)
        {
            this.entitiesRepository = interviewEntitiesRepository;
            this.summaryRepository = interviewSummaryRepository;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId) => this.entitiesRepository.Query(_
            => _.Where(x => x.InterviewId == interviewId && x.HasFlag).Select(x => x.Identity).ToArray());

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity)
        {
            this.ThrowIfInterviewDeletedOrReadOnly(interviewId);

            var flaggedQuestion = this.entitiesRepository.Query(
                _ => _.FirstOrDefault(x => x.InterviewId == interviewId && x.Identity == questionIdentity));

            ThrowIfQuestionNotFound(flaggedQuestion);

            flaggedQuestion.HasFlag = true;

            this.entitiesRepository.Store(flaggedQuestion, null);
        }

        public void RemoveFlagFromQuestion(Guid interviewId, Identity questionIdentity)
        {
            this.ThrowIfInterviewDeletedOrReadOnly(interviewId);

            var flaggedQuestion = this.entitiesRepository.Query(_ =>
                _.FirstOrDefault(x => x.InterviewId == interviewId && x.Identity == questionIdentity));

            ThrowIfQuestionNotFound(flaggedQuestion);

            flaggedQuestion.HasFlag = false;

            this.entitiesRepository.Store(flaggedQuestion, null);
        }

        public void RemoveInterview(Guid interviewId)
        {
            var removedEntities = this.entitiesRepository.Query(x => x.Where(y => y.InterviewId == interviewId));
            this.entitiesRepository.Remove(removedEntities);
        }

        public void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer)
        {
            throw new NotImplementedException();
        }

        public void MakeEntitiesValid(Guid interviewId, Identity[] entityIds)
        {
            throw new NotImplementedException();
        }

        public void MakeEntitiesInvalid(Guid interviewId, IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> entityIds)
        {
            throw new NotImplementedException();
        }

        public void EnableEntities(Guid interviewId, Identity[] entityIds)
        {
            throw new NotImplementedException();
        }

        public void DisableEntities(Guid interviewId, Identity[] entityIds)
        {
            throw new NotImplementedException();
        }

        public void UpdateVariables(Guid interviewId, ChangedVariable[] variables)
        {
            throw new NotImplementedException();
        }

        public void MarkQuestionsAsReadOnly(Guid interviewId, Identity[] questionIds)
        {
            throw new NotImplementedException();
        }

        public void AddRosters(Guid interviewId, Identity[] rosterIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveRosters(Guid interviewId, Identity[] rosterIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveAnswers(Guid interviewId, Identity[] questionIds)
        {
            throw new NotImplementedException();
        }

        public InterviewDbEntity GetQuestion(Guid interviewId, Identity questionIdentity)
        {
            throw new NotImplementedException();
        }

        public InterviewDbEntity CreateQuestion(Guid interviewId, Identity questionIdentity)
        {
            throw new NotImplementedException();
        }

        public void UpdateQuestion(InterviewDbEntity entity)
        {
            throw new NotImplementedException();
        }

        private void ThrowIfInterviewDeletedOrReadOnly(Guid interviewId)
        {
            var interview = this.summaryRepository.GetById(interviewId);

            if (interview == null)
                throw new InterviewException($"Interview {interviewId} not found.");

            ThrowIfInterviewApprovedByHq(interview);
            ThrowIfInterviewReceivedByInterviewer(interview);
        }

        private static void ThrowIfInterviewReceivedByInterviewer(InterviewSummary interview)
        {
            if (interview.ReceivedByInterviewer)
                throw new InterviewException($"Can't modify Interview {interview.InterviewId} on server, because it received by interviewer.");
        }

        private static void ThrowIfInterviewApprovedByHq(InterviewSummary interview)
        {
            if (interview.Status == InterviewStatus.ApprovedByHeadquarters)
                throw new InterviewException($"Interview was approved by Headquarters and cannot be edited. InterviewId: {interview.InterviewId}");
        }

        private static void ThrowIfQuestionNotFound(InterviewDbEntity question)
        {
            if (question == null)
                throw new InterviewException(
                    $"Question is missing. Question Id: {question.Identity}. Interview Id: {question.InterviewId}.");
        }
    }
}