using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    internal class InterviewerInterviewsFactory : IInterviewInformationFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        private readonly IHeadquartersEventStore eventStore;

        public InterviewerInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> reader,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewPackagesService incomingSyncPackagesQueue,
            IHeadquartersEventStore eventStore)
        {
            this.reader = reader;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;            
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.eventStore = eventStore;
        }
        
        public List<InterviewInformation> GetInProgressInterviewsForInterviewer(Guid interviewerId)
        {
            var processingPackages = this.incomingSyncPackagesQueue.GetAllPackagesInterviewIds();

            var inProgressInterviews =  this.reader.Query(interviews =>
                interviews
                    .Where(ForInterviewer)
                    .Where(interview => interview.ResponsibleId == interviewerId)
                    .Select(x => new 
                    {
                        x.InterviewId, 
                        x.QuestionnaireIdentity, 
                        x.WasRejectedBySupervisor, 
                        ReceivedByInterviewer = x.ReceivedByInterviewerAtUtc.HasValue,
                        x.ResponsibleId})
                    .ToList());

            var deletedQuestionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            }).Items.Where(questionnaire => questionnaire.IsDeleted).Select(x => x.Identity().ToString());

            var filteredInterviews = inProgressInterviews.Where(
                    interview => !deletedQuestionnaires.Any(deletedQuestionnaire => deletedQuestionnaire.Equals(interview.QuestionnaireIdentity))
                                 && !processingPackages.Any(filename => filename.Contains(interview.InterviewId.FormatGuid())))
                .Select(interview => new InterviewInformation
                {
                    Id = interview.InterviewId,
                    QuestionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireIdentity),
                    IsRejected = interview.WasRejectedBySupervisor,
                    ResponsibleId = interview.ResponsibleId,
                    IsReceivedByInterviewer = interview.ReceivedByInterviewer,
                    LastEventSequence = eventStore.GetMaxEventSequenceWithAnyOfSpecifiedTypes(interview.InterviewId, EventsThatAssignInterviewToResponsibleProvider.GetTypeNames()),
                    LastEventId = eventStore.GetLastEventId(interview.InterviewId),
                }).ToList();
            
            return filteredInterviews;
        }

        private readonly Expression<Func<InterviewSummary, bool>> ForInterviewer =
            summary =>
                summary.InterviewMode != InterviewMode.CAWI &&
                (summary.Status == InterviewStatus.InterviewerAssigned
                 || summary.Status == InterviewStatus.RejectedBySupervisor);

        public bool HasAnyInterviewsInProgressWithResolvedCommentsForInterviewer(Guid authorizedUserId)
        {
            var summary = this.reader.Query(interviews =>
                interviews
                    .Where(ForInterviewer)
                    .Where(interview => interview.ResponsibleId == authorizedUserId
                                        && interview.HasResolvedComments)
                    .Select(x => x.SummaryId)
                    .FirstOrDefault());
            return summary != null;
        }

        public bool HasAnyInterviewsInProgressWithResolvedCommentsForSupervisor(Guid authorizedUserId)
        {
            var summary = this.reader.Query(interviews =>
                interviews.Where(interview => ( // assigned on supervisor
                                                  interview.ResponsibleId == authorizedUserId &&
                                                  (interview.Status == InterviewStatus.SupervisorAssigned || interview.Status == InterviewStatus.RejectedByHeadquarters)
                                              ) ||
                                              ( // assigned on interviewers on his team
                                                  interview.SupervisorId == authorizedUserId &&
                                                  (interview.Status == InterviewStatus.InterviewerAssigned || interview.Status == InterviewStatus.RejectedBySupervisor || interview.Status == InterviewStatus.RejectedByHeadquarters)
                                              )
                                              && interview.HasResolvedComments)
                    .Select(x => x.SummaryId)
                    .FirstOrDefault());
            return summary != null;
        }

        public bool HasAnySmallSubstitutionEvent(Guid interviewId)
        {
            var interviewSummary = reader.GetById(interviewId.FormatGuid());
            return interviewSummary.HasSmallSubstitutions;
        }

        public List<InterviewInformation> GetInProgressInterviewsForSupervisor(Guid supervisorId)
        {
            var processigPackages = this.incomingSyncPackagesQueue.GetAllPackagesInterviewIds();

            var inProgressInterviews = this.reader.Query(interviews =>
                interviews
                    .Where(interview => interview.InterviewMode != InterviewMode.CAWI)
                    .Where(interview =>
                        ( // assigned on supervisor
                            interview.ResponsibleId == supervisorId &&
                            (interview.Status == InterviewStatus.SupervisorAssigned || interview.Status == InterviewStatus.RejectedByHeadquarters)
                        ) ||
                        ( // assigned on interviewers on his team
                            interview.SupervisorId == supervisorId &&
                            (interview.Status == InterviewStatus.InterviewerAssigned || interview.Status == InterviewStatus.RejectedBySupervisor || interview.Status == InterviewStatus.RejectedByHeadquarters)
                        )
                    )
                    .Select(x => new
                    {
                        x.InterviewId,
                        x.QuestionnaireId,
                        x.QuestionnaireVersion,
                        x.WasRejectedBySupervisor,
                        x.ResponsibleId,
                        ReceivedByInterviewer = x.ReceivedByInterviewerAtUtc.HasValue
                    })
                    .ToList());

            var deletedQuestionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            }).Items.Where(questionnaire => questionnaire.IsDeleted);

            List<InterviewInformation> filteredInterviews = inProgressInterviews.Where(
                    interview => !deletedQuestionnaires.Any(deletedQuestionnaire => deletedQuestionnaire.QuestionnaireId == interview.QuestionnaireId 
                                                                                    && deletedQuestionnaire.Version == interview.QuestionnaireVersion)
                                 && !processigPackages.Any(filename => filename.Contains(interview.InterviewId.FormatGuid())))
                .Select(interview => new InterviewInformation
                {
                    Id = interview.InterviewId,
                    QuestionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion),
                    IsRejected = interview.WasRejectedBySupervisor,
                    ResponsibleId = interview.ResponsibleId,
                    IsReceivedByInterviewer = interview.ReceivedByInterviewer,
                    LastEventSequence = eventStore.GetMaxEventSequenceWithAnyOfSpecifiedTypes(interview.InterviewId, EventsThatAssignInterviewToResponsibleProvider.GetTypeNames()),
                    LastEventId = eventStore.GetLastEventId(interview.InterviewId),
                }).ToList();

            return filteredInterviews;
        }

        public IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds)
        {
            var filteredinterviews = this.reader.Query(
                interviews => interviews.Where(interview => interviewIds.Contains(interview.InterviewId)).ToList());

            return filteredinterviews.Select(interview => new InterviewInformation
            {
                Id = interview.InterviewId,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireIdentity),
                IsRejected = interview.Status == InterviewStatus.RejectedBySupervisor,
                ResponsibleId = interview.ResponsibleId,
            });
        }
    }
}
