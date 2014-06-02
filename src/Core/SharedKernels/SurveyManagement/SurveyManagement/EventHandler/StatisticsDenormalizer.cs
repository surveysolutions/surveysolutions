using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class StatisticsDenormalizer : IEventHandler,
                                          IEventHandler<InterviewCreated>,
                                          IEventHandler<InterviewFromPreloadedDataCreated>,
                                          IEventHandler<InterviewStatusChanged>,
                                          IEventHandler<SupervisorAssigned>,
                                          IEventHandler<InterviewDeleted>,
                                          IEventHandler<InterviewRestored>,
                                          IEventHandler<InterviewerAssigned>,
                                          IEventHandler<InterviewOnClientCreated>

    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate> statisticsStorage;
        private readonly IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires;

        public StatisticsDenormalizer(
            IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate> statisticsStorage,
            IReadSideRepositoryWriter<UserDocument> users, 
            IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage,
            IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires)
        {
            this.statisticsStorage = statisticsStorage;
            this.users = users;
            this.interviewBriefStorage = interviewBriefStorage;
            this.questionnaires = questionnaires;
        }
        
        private void HandleCreation(Guid eventSourceId, Guid responsibleId, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(eventSourceId);

            if (interviewBriefItem != null)
            {
                this.DecreaseStatisticsByStatus(this.GetStatisticItem(interviewBriefItem), interviewBriefItem.Status);
            }
            else
            {
                interviewBriefItem = new InterviewBrief
                {
                    InterviewId = eventSourceId,
                    IsDeleted = false,
                    ResponsibleId = responsibleId,
                    Status = InterviewStatus.Created,
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion
                };

            }

            var statistics = this.GetStatisticItem(interviewBriefItem) ?? this.CreateNewStatisticsLine(interviewBriefItem);

            this.IncreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var statistics = this.GetStatisticItem(interviewBriefItem);
            
            interviewBriefItem.IsDeleted = true;
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var statistics = this.GetStatisticItem(interviewBriefItem);

            interviewBriefItem.IsDeleted = false;
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var statistics = this.GetStatisticItem(interviewBriefItem);
            
            this.DecreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            this.IncreaseStatisticsByStatus(statistics, evnt.Payload.Status);
            
            interviewBriefItem.Status = evnt.Payload.Status;
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            this.AssignToOtherUser(evnt.EventSourceId, evnt.Payload.SupervisorId);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            this.AssignToOtherUser(evnt.EventSourceId, evnt.Payload.InterviewerId);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
            this.SetInterviewValidity(evnt.EventSourceId, false);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt)
        {
            this.SetInterviewValidity(evnt.EventSourceId, true);
        }

        private void AssignToOtherUser(Guid interviewId, Guid interviewerId)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(interviewId);
            //update old statistics
            var oldStatistics = this.GetStatisticItem(interviewBriefItem);
            this.DecreaseStatisticsByStatus(oldStatistics, interviewBriefItem.Status);
            
            this.StoreStatisticsItem(interviewBriefItem, oldStatistics);

            interviewBriefItem.ResponsibleId = interviewerId;
            var statistics = this.GetStatisticItem(interviewBriefItem) ?? this.CreateNewStatisticsLine(interviewBriefItem);
            this.IncreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        private StatisticsLineGroupedByUserAndTemplate CreateNewStatisticsLine(InterviewBrief interviewBriefItem)
        {
            QuestionnaireBrowseItem questionnaire = this.questionnaires.GetById(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion);
            var questionnaireTitle = questionnaire != null ? questionnaire.Title : "<UNKNOWN QUESTIONNAIRE>";
            var responsible = this.users.GetById(interviewBriefItem.ResponsibleId);
            string responsibleName = responsible != null ? responsible.UserName : "<UNKNOWN USER>";
            Guid? teamLeadId = null;
            string teamLeadName = "";

            if (responsible != null && responsible.Roles.Contains(UserRoles.Supervisor))
            {
                teamLeadId = responsible.PublicKey;
                teamLeadName = responsibleName;
            }
            if (responsible != null && responsible.Supervisor != null)
            {
                teamLeadId = responsible.Supervisor.Id;
                teamLeadName = this.users.GetById(teamLeadId.Value).UserName;
            }

            return new StatisticsLineGroupedByUserAndTemplate
                {
                    QuestionnaireId = interviewBriefItem.QuestionnaireId,
                    QuestionnaireVersion = interviewBriefItem.QuestionnaireVersion,
                    QuestionnaireTitle = questionnaireTitle,
                    ResponsibleId = interviewBriefItem.ResponsibleId,
                    ResponsibleName = responsibleName,
                    TeamLeadId = teamLeadId,
                    TeamLeadName = teamLeadName,
                };
        }

        private void DecreaseStatisticsByStatus(StatisticsLineGroupedByUserAndTemplate statistics, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, status, false);
        }

        private void IncreaseStatisticsByStatus(StatisticsLineGroupedByUserAndTemplate statistics, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, status, true);
        }

        private void ChangeByStatus(StatisticsLineGroupedByUserAndTemplate summary, InterviewStatus status, bool isIncrease)
        {
            int incCount = isIncrease ? 1 : -1;

            summary.CreatedCount += status == InterviewStatus.Created ? incCount : 0;
            summary.SupervisorAssignedCount += status == InterviewStatus.SupervisorAssigned ? incCount : 0;
            summary.InterviewerAssignedCount += status == InterviewStatus.InterviewerAssigned ? incCount : 0;
            summary.SentToCapiCount += status == InterviewStatus.SentToCapi ? incCount : 0;
            summary.CompletedCount += status == InterviewStatus.Completed ? incCount : 0;
            summary.ApprovedBySupervisorCount += status == InterviewStatus.ApprovedBySupervisor ? incCount : 0;
            summary.RejectedBySupervisorCount += status == InterviewStatus.RejectedBySupervisor ? incCount : 0;

            summary.ApprovedByHeadquartersCount += status == InterviewStatus.ApprovedByHeadquarters ? incCount : 0;
            summary.RejectedByHeadquartersCount += status == InterviewStatus.RejectedByHeadquarters ? incCount : 0;

            summary.RestoredCount += status == InterviewStatus.Restored ? incCount : 0;

            if (status != InterviewStatus.Deleted)
            {
                summary.TotalCount += incCount;
            }
            
        }

        private StatisticsLineGroupedByUserAndTemplate GetStatisticItem(InterviewBrief interviewBriefItem)
        {
            var key = GetCombinedStatisticKey(interviewBriefItem);
            return this.statisticsStorage.GetById(key);
        }

        private static Guid GetCombinedStatisticKey(InterviewBrief interviewBriefItem)
        {
            return interviewBriefItem.QuestionnaireId
                .Combine(interviewBriefItem.QuestionnaireVersion)
                .Combine(interviewBriefItem.ResponsibleId);
        }

        private void StoreStatisticsItem(InterviewBrief interviewBriefItem, StatisticsLineGroupedByUserAndTemplate statistics)
        {
            var key = GetCombinedStatisticKey(interviewBriefItem);
            this.statisticsStorage.Store(statistics, key);
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(UserDocument), typeof(QuestionnaireBrowseItem) }; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(StatisticsLineGroupedByUserAndTemplate), typeof(InterviewBrief) }; }
        }

        private void SetInterviewValidity(Guid interviewId, bool isValid)
        {
            var interview = this.interviewBriefStorage.GetById(interviewId);

            interview.HasErrors = !isValid;

            this.interviewBriefStorage.Store(interview, interview.InterviewId);
        }
    }
}
