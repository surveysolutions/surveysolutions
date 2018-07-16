using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    internal class InterviewerInterviewsFactory : IInterviewInformationFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;

        public InterviewerInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> reader,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IStatefulInterviewRepository statefulInterviewRepository,
            IInterviewPackagesService incomingSyncPackagesQueue)
        {
            this.reader = reader;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
        }

        public IEnumerable<InterviewInformation> GetInProgressInterviewsForInterviewer(Guid interviewerId)
        {
            var processigPackages = this.incomingSyncPackagesQueue.GetAllPackagesInterviewIds();

            var inProgressInterviews =  this.reader.Query(interviews =>
                interviews.Where(interview => interview.ResponsibleId == interviewerId && 
                                              (interview.Status == InterviewStatus.InterviewerAssigned || interview.Status == InterviewStatus.RejectedBySupervisor))
                    .Select(x => new {x.InterviewId, x.QuestionnaireId, x.QuestionnaireVersion, x.WasRejectedBySupervisor, x.ResponsibleId})
                    .ToList());

            var deletedQuestionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            }).Items.Where(questionnaire => questionnaire.IsDeleted);

            return inProgressInterviews.Where(
                interview => !deletedQuestionnaires.Any(deletedQuestionnaire => deletedQuestionnaire.QuestionnaireId == interview.QuestionnaireId && deletedQuestionnaire.Version == interview.QuestionnaireVersion)
                && !processigPackages.Any(filename => filename.Contains(interview.InterviewId.FormatGuid())))
                .Select(interview => new InterviewInformation
                {
                    Id = interview.InterviewId,
                    QuestionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion),
                    IsRejected = interview.WasRejectedBySupervisor,
                    ResponsibleId = interview.ResponsibleId
                });
        }

        public IEnumerable<InterviewInformation> GetInProgressInterviewsForSupervisor(Guid supervisorId)
        {
            var processigPackages = this.incomingSyncPackagesQueue.GetAllPackagesInterviewIds();

            var inProgressInterviews = this.reader.Query(interviews =>
                interviews.Where(interview => 
                        ( // assigned on supervisor
                            interview.ResponsibleId == supervisorId &&
                            (interview.Status == InterviewStatus.SupervisorAssigned || interview.Status == InterviewStatus.RejectedByHeadquarters)
                        ) ||
                        ( // assigned on interviewers on his team
                            interview.TeamLeadId == supervisorId &&
                            (interview.Status == InterviewStatus.InterviewerAssigned || interview.Status == InterviewStatus.RejectedBySupervisor || interview.Status == InterviewStatus.RejectedByHeadquarters)
                        )
                    )
                    .Select(x => new
                    {
                        x.InterviewId,
                        x.QuestionnaireId,
                        x.QuestionnaireVersion,
                        x.WasRejectedBySupervisor,
                        x.ResponsibleId
                    })
                    .ToList());

            var deletedQuestionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            }).Items.Where(questionnaire => questionnaire.IsDeleted);

            return inProgressInterviews.Where(
                    interview => !deletedQuestionnaires.Any(deletedQuestionnaire => deletedQuestionnaire.QuestionnaireId == interview.QuestionnaireId && deletedQuestionnaire.Version == interview.QuestionnaireVersion)
                                 && !processigPackages.Any(filename => filename.Contains(interview.InterviewId.FormatGuid())))
                .Select(interview => new InterviewInformation
                {
                    Id = interview.InterviewId,
                    QuestionnaireIdentity = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion),
                    IsRejected = interview.WasRejectedBySupervisor,
                    ResponsibleId = interview.ResponsibleId
                });
        }

        public IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds)
        {
            var filteredinterviews = this.reader.Query(
                interviews => interviews.Where(interview => interviewIds.Contains(interview.InterviewId)).ToList());

            return filteredinterviews.Select(interview => new InterviewInformation
            {
                Id = interview.InterviewId,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireIdentity),
                IsRejected = interview.Status == InterviewStatus.RejectedBySupervisor
            });
        }

        public InterviewSynchronizationDto GetInProgressInterviewDetails(Guid interviewId)
        {
            var statefulInterview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            var isInterviewerAcceptedStatus = statefulInterview.Status == InterviewStatus.InterviewerAssigned
                                              || statefulInterview.Status == InterviewStatus.RejectedBySupervisor;
            if (!isInterviewerAcceptedStatus)
                return null;

            return statefulInterview.GetSynchronizationDto();
        }
    }
}
