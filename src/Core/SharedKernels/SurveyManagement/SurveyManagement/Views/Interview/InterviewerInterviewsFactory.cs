using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    internal class InterviewerInterviewsFactory : IInterviewInformationFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly InterviewSynchronizationDtoFactory synchronizationDtoFactory;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDataRepository;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> interviewStatusesFactory;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;

        public InterviewerInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> reader,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            InterviewSynchronizationDtoFactory synchronizationDtoFactory,
            IReadSideKeyValueStorage<InterviewData> interviewDataRepository,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> interviewStatusesFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue)
        {
            this.reader = reader;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.synchronizationDtoFactory = synchronizationDtoFactory;
            this.interviewDataRepository = interviewDataRepository;
            this.interviewStatusesFactory = interviewStatusesFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
        }

        public IEnumerable<InterviewInformation> GetInProgressInterviews(Guid interviewerId)
        {
            var inProgressInterviews =  this.reader.Query(interviews =>
                interviews.Where(interview => !interview.IsDeleted && (interview.ResponsibleId == interviewerId) && 
                    (interview.Status == InterviewStatus.InterviewerAssigned || interview.Status == InterviewStatus.RejectedBySupervisor)).ToList());

            var deletedQuestionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            }).Items.Where(questionnaire => questionnaire.IsDeleted);

            return inProgressInterviews.Where(
                interview => !deletedQuestionnaires.Any(deletedQuestionnaire => deletedQuestionnaire.QuestionnaireId == interview.QuestionnaireId && deletedQuestionnaire.Version == interview.QuestionnaireVersion)
                && !this.incomingSyncPackagesQueue.HasPackagesByInterviewId(interview.InterviewId))
                .Select(interview => new InterviewInformation()
                {
                    Id = interview.InterviewId,
                    QuestionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion),
                    IsRejected = interview.WasRejectedBySupervisor
                });
        }

        public IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds)
        {
            var filteredinterviews = this.reader.Query(
                interviews => interviews.Where(interview => interviewIds.Contains(interview.InterviewId)).ToList());

            return filteredinterviews.Select(interview => new InterviewInformation()
            {
                Id = interview.InterviewId,
                QuestionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion),
                IsRejected = interview.Status == InterviewStatus.RejectedBySupervisor
            });
        }

        public InterviewSynchronizationDto GetInterviewDetails(Guid interviewId)
        {
            var interviewData = this.interviewDataRepository.GetById(interviewId);

            var orderedInterviewStatuses = this.interviewStatusesFactory.Load(new ChangeStatusInputModel()
            {
                InterviewId = interviewId
            }).StatusHistory.Where(status =>
                status.Status == InterviewStatus.RejectedBySupervisor ||
                status.Status == InterviewStatus.InterviewerAssigned)
                .OrderBy(status => status.Date);

            var lastInterviewStatus = orderedInterviewStatuses.LastOrDefault();

            var lastInterviewerAssignedStatus = orderedInterviewStatuses.LastOrDefault(status => status.Status == InterviewStatus.InterviewerAssigned);
            var lastRejectedBySupervisorStatus = orderedInterviewStatuses.LastOrDefault(status => status.Status == InterviewStatus.RejectedBySupervisor);

            return this.synchronizationDtoFactory.BuildFrom(interviewData, interviewData.ResponsibleId,
                lastInterviewStatus.Status, lastInterviewStatus.Comment, lastRejectedBySupervisorStatus?.Date,
                lastInterviewerAssignedStatus?.Date);
        }
    }
}
