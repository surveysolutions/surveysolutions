using System;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly IPlainStorageAccessor<InterviewEntity> entitiesRepository;
        private readonly IReadSideRepositoryReader<InterviewSummary> summaryRepository;

        public InterviewFactory(IPlainStorageAccessor<InterviewEntity> interviewEntitiesRepository, 
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository)
        {
            this.entitiesRepository = interviewEntitiesRepository;
            this.summaryRepository = interviewSummaryRepository;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId) => this.entitiesRepository.Query(_
            => _.Where(x => x.InterviewId == interviewId && x.HasFlag).Select(x => x.QuestionIdentity).ToArray());

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity)
        {
            this.ThrowIfInterviewDeletedOrReadOnly(interviewId);

            var flaggedQuestion = this.entitiesRepository.Query(_ => _.FirstOrDefault(x => x.InterviewId == interviewId && x.QuestionIdentity == questionIdentity)) ??
                                  new InterviewEntity {InterviewId = interviewId, QuestionIdentity = questionIdentity};

            ThrowIfQuestionNotFound(flaggedQuestion);

            flaggedQuestion.HasFlag = true;

            this.entitiesRepository.Store(flaggedQuestion, null);
        }

        public void RemoveFlagFromQuestion(Guid interviewId, Identity questionIdentity)
        {
            this.ThrowIfInterviewDeletedOrReadOnly(interviewId);

            var flaggedQuestion = this.entitiesRepository.Query(_ =>
                _.FirstOrDefault(x => x.InterviewId == interviewId && x.QuestionIdentity == questionIdentity));

            ThrowIfQuestionNotFound(flaggedQuestion);

            flaggedQuestion.HasFlag = false;

            this.entitiesRepository.Store(flaggedQuestion, null);
        }

        public void RemoveInterview(Guid interviewId)
        {
            var removedEntities = this.entitiesRepository.Query(x => x.Where(y => y.InterviewId == interviewId));
            this.entitiesRepository.Remove(removedEntities);
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

        private static void ThrowIfQuestionNotFound(InterviewEntity question)
        {
            if (question == null)
                throw new InterviewException(
                    $"Question is missing. Question Id: {question.QuestionIdentity}. Interview Id: {question.InterviewId}.");
        }
    }
}