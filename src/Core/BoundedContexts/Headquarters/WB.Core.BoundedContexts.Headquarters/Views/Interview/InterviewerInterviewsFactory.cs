using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    internal class InterviewerInterviewsFactory : IInterviewInformationFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewSynchronizationDtoFactory synchronizationDtoFactory;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDataRepository;
        private readonly IChangeStatusFactory interviewStatusesFactory;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;

        public InterviewerInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> reader,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewSynchronizationDtoFactory synchronizationDtoFactory,
            IReadSideKeyValueStorage<InterviewData> interviewDataRepository,
            IChangeStatusFactory interviewStatusesFactory,
            IInterviewPackagesService incomingSyncPackagesQueue)
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
            var processigPackages = this.incomingSyncPackagesQueue.GetAllPackagesInterviewIds();

            var inProgressInterviews =  this.reader.Query(interviews =>
                interviews.Where(interview => !interview.IsDeleted && (interview.ResponsibleId == interviewerId) && 
                                              (interview.Status == InterviewStatus.InterviewerAssigned || interview.Status == InterviewStatus.RejectedBySupervisor))
                    .Select(x => new {x.InterviewId, x.QuestionnaireId, x.QuestionnaireVersion, x.WasRejectedBySupervisor})
                    .ToList());

            var deletedQuestionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            }).Items.Where(questionnaire => questionnaire.IsDeleted);

            return inProgressInterviews.Where(
                interview => !deletedQuestionnaires.Any(deletedQuestionnaire => deletedQuestionnaire.QuestionnaireId == interview.QuestionnaireId && deletedQuestionnaire.Version == interview.QuestionnaireVersion)
                && !processigPackages.Any(filename => filename.Contains(interview.InterviewId.FormatGuid())))
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

        public InterviewSynchronizationDto GetInProgressInterviewDetails(Guid interviewId)
        {
            var interviewData = this.interviewDataRepository.GetById(interviewId);

#warning do not sort status history by date! Status timestamp is taken from event timestamp and occasionally timestamp of an earlier event could be greater then timestamp of the latest events. StatusHistory is ordered list and the order of statuses is preserved by db.
            var fullStatusHistory = this
                .interviewStatusesFactory
                .Load(new ChangeStatusInputModel { InterviewId = interviewId })
                .StatusHistory
                .ToList();

            var lastInterviewStatus = fullStatusHistory.Last();

            var isInterviewerAcceptedStatus = lastInterviewStatus.Status == InterviewStatus.InterviewerAssigned
                                           || lastInterviewStatus.Status == InterviewStatus.RejectedBySupervisor;
            if (!isInterviewerAcceptedStatus)
                return null;

            var lastInterviewerAssignedStatus = fullStatusHistory.LastOrDefault(status => status.Status == InterviewStatus.InterviewerAssigned);

            var lastCompleteStatus = fullStatusHistory.LastOrDefault(x => x.Status == InterviewStatus.Completed);

            var statusHistoryStartingWithLastComplete =
                lastCompleteStatus != null
                    ? fullStatusHistory.SkipWhile(status => status != lastCompleteStatus).ToList()
                    : fullStatusHistory;

            var orderedInterviewStatuses = statusHistoryStartingWithLastComplete
                .Where(status => status.Status == InterviewStatus.RejectedBySupervisor)
                .ToList();

            var lastRejectedBySupervisorStatus =
                orderedInterviewStatuses.LastOrDefault(status => status.Status == InterviewStatus.RejectedBySupervisor);

            var interviewStatus = lastRejectedBySupervisorStatus?.Status ?? lastInterviewStatus.Status;

            return this.synchronizationDtoFactory.BuildFrom(interviewData, interviewData.ResponsibleId,
                interviewStatus,
                lastRejectedBySupervisorStatus?.Comment,
                lastRejectedBySupervisorStatus?.Date,
                lastInterviewerAssignedStatus?.Date);
        }

        public IList<QuestionnaireIdentity> GetQuestionnairesWithAssignments(Guid interviewerId)
        {
            var inProgressQuestionnaires = this.reader.Query(interviews =>
                                                         interviews.Where(interview => !interview.IsDeleted && (interview.ResponsibleId == interviewerId) && 
                                                                                       (interview.Status == InterviewStatus.InterviewerAssigned || interview.Status == InterviewStatus.RejectedBySupervisor))
                                                             .Select(x => new { x.QuestionnaireId, x.QuestionnaireVersion})
                                                             .Distinct()
                                                             .ToList());

            return inProgressQuestionnaires.Select(x => new QuestionnaireIdentity(x.QuestionnaireId, x.QuestionnaireVersion)).ToList();
        }
    }
}
