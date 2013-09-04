using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View.CompleteQuestionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class StatisticsDenormalizer : IEventHandler,
                                          IEventHandler<InterviewCreated>,
                                          IEventHandler<InterviewStatusChanged>,
                                          IEventHandler<SupervisorAssigned>,
                                          IEventHandler<InterviewDeleted>,
                                          IEventHandler<InterviewerAssigned>

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

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            var interviewBriefItem = new InterviewBrief
                {
                    InterviewId = evnt.EventSourceId,
                    IsDeleted = false,
                    ResponsibleId = evnt.Payload.UserId,
                    Status = InterviewStatus.Created,
                    QuestionnaireId = evnt.Payload.QuestionnaireId,
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                };

            var statistics = this.GetStatisticItem(interviewBriefItem);
            if (statistics == null)
            {
                statistics = CreateNewStatisticsLine(interviewBriefItem);
            }
            IncreaseByStatus(statistics, interviewBriefItem.Status);
            interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            var interviewBriefItem = interviewBriefStorage.GetById(evnt.EventSourceId);
            var statistics = this.GetStatisticItem(interviewBriefItem);
            DecreaseByStatus(statistics, interviewBriefItem.Status);

            interviewBriefItem.IsDeleted = true;
            interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interviewBriefItem = interviewBriefStorage.GetById(evnt.EventSourceId);
            var statistics = this.GetStatisticItem(interviewBriefItem);
            if (evnt.Payload.Status != InterviewStatus.Deleted)
            {
                DecreaseByStatus(statistics, interviewBriefItem.Status);
                IncreaseByStatus(statistics, evnt.Payload.Status);
            }
            interviewBriefItem.Status = evnt.Payload.Status;
            interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            this.AssignToOtherUser(evnt.EventSourceId, evnt.Payload.SupervisorId);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            this.AssignToOtherUser(evnt.EventSourceId, evnt.Payload.InterviewerId);
        }

        private void AssignToOtherUser(Guid interviewId, Guid interviewerId)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(interviewId);
            //update old statistics
            var oldStatistics = this.GetStatisticItem(interviewBriefItem);
            this.DecreaseByStatus(oldStatistics, interviewBriefItem.Status);
            this.StoreStatisticsItem(interviewBriefItem, oldStatistics);

            interviewBriefItem.ResponsibleId = interviewerId;
            var statistics = this.GetStatisticItem(interviewBriefItem);
            if (statistics == null)
            {
                statistics = this.CreateNewStatisticsLine(interviewBriefItem);
            }
            this.IncreaseByStatus(statistics, interviewBriefItem.Status);
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        private StatisticsLineGroupedByUserAndTemplate CreateNewStatisticsLine(InterviewBrief interviewBriefItem)
        {
            var questionnaireTitle =
                this.questionnaires.GetById(
                    interviewBriefItem.QuestionnaireId,interviewBriefItem.QuestionnaireVersion).Title;
            var responsible = users.GetById(interviewBriefItem.ResponsibleId);
            string responsibleName=responsible.UserName;
            Guid? teamLeadId = null;
            string teamLeadName = "";

            if (responsible.Roles.Contains(UserRoles.Supervisor))
            {
                teamLeadId = responsible.PublicKey;
                teamLeadName = responsibleName;
            }
            if (responsible.Supervisor != null)
            {
                teamLeadId = responsible.Supervisor.Id;
                teamLeadName = users.GetById(teamLeadId.Value).UserName;
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

        private void DecreaseByStatus(StatisticsLineGroupedByUserAndTemplate statistics, InterviewStatus status)
        {
            this.IncreaseByStatus(statistics, status, false);
        }

        private void IncreaseByStatus(StatisticsLineGroupedByUserAndTemplate summary, InterviewStatus status, bool isIncrease = true)
        {
            int incCount = isIncrease ? 1 : -1;

            summary.CreatedCount += status == InterviewStatus.Created ? incCount : 0;
            summary.SupervisorAssignedCount += status == InterviewStatus.SupervisorAssigned ? incCount : 0;
            summary.InterviewerAssignedCount += status == InterviewStatus.InterviewerAssigned ? incCount : 0;
            summary.SentToCapiCount += status == InterviewStatus.SentToCapi ? incCount : 0;
            summary.CompletedCount += status == InterviewStatus.Completed ? incCount : 0;
            summary.ApprovedBySupervisorCount += status == InterviewStatus.ApprovedBySupervisor ? incCount : 0;
            summary.RejectedBySupervisorCount += status == InterviewStatus.RejectedBySupervisor ? incCount : 0;
            summary.RestoredCount += status == InterviewStatus.Restored ? incCount : 0;
            summary.TotalCount += incCount;
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
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(UserDocument), typeof(QuestionnaireBrowseItem) }; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(StatisticsLineGroupedByUserAndTemplate), typeof(InterviewBrief) }; }
        }
    }
}
