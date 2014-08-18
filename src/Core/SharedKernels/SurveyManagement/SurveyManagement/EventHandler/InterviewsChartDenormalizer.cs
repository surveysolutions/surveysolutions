using System;
using Main.Core.Documents;
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
    public class InterviewsChartDenormalizer : IEventHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewOnClientCreated>,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewRestored>,
        IEventHandler<InterviewerAssigned>
    {
        private readonly IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage;
        private readonly IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires;

        public InterviewsChartDenormalizer(
            IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage,
            IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage,
            IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires)
        {
            this.statisticsStorage = statisticsStorage;
            this.interviewBriefStorage = interviewBriefStorage;
            this.questionnaires = questionnaires;
        }

        private void HandleCreation(Guid eventSourceId, Guid responsibleId, Guid questionnaireId, long questionnaireVersion, DateTime dateTime)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(eventSourceId);

            if (interviewBriefItem != null)
            {
                this.DecreaseStatisticsByStatus(this.GetStatisticItem(interviewBriefItem, dateTime), interviewBriefItem.Status);
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

            var statistics = this.GetStatisticItem(interviewBriefItem, dateTime) ?? this.CreateNewStatisticsLine(interviewBriefItem, dateTime);

            this.IncreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            //this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics, dateTime);
        }

        private StatisticsLineGroupedByDateAndTemplate GetStatisticItem(InterviewBrief interviewBriefItem, DateTime dateTime)
        {
            var key = GetCombinedStatisticKey(interviewBriefItem, dateTime);
            return this.statisticsStorage.GetById(key);
        }

        private StatisticsLineGroupedByDateAndTemplate CreateNewStatisticsLine(InterviewBrief interviewBriefItem, DateTime dateTime)
        {
            var questionnaire = this.questionnaires.GetById(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion);
            var questionnaireTitle = questionnaire != null ? questionnaire.Title : "<UNKNOWN QUESTIONNAIRE>";

            return new StatisticsLineGroupedByDateAndTemplate
            {
                QuestionnaireId = interviewBriefItem.QuestionnaireId,
                QuestionnaireVersion = interviewBriefItem.QuestionnaireVersion,
                QuestionnaireTitle = questionnaireTitle,
                Date = dateTime
            };
        }

        private void StoreStatisticsItem(InterviewBrief interviewBriefItem, StatisticsLineGroupedByDateAndTemplate statistics, DateTime dateTime)
        {
            var key = GetCombinedStatisticKey(interviewBriefItem, dateTime);
            this.statisticsStorage.Store(statistics, key);
        }

        private static Guid GetCombinedStatisticKey(InterviewBrief interviewBriefItem, DateTime dateTime)
        {
            return interviewBriefItem.QuestionnaireId
                .Combine(interviewBriefItem.QuestionnaireVersion)
                .Combine(dateTime.Date.Ticks);
        }

        private void DecreaseStatisticsByStatus(StatisticsLineGroupedByDateAndTemplate statistics, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, status, false);
        }

        private void IncreaseStatisticsByStatus(StatisticsLineGroupedByDateAndTemplate statistics, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, status, true);
        }

        private void ChangeByStatus(StatisticsLineGroupedByDateAndTemplate summary, InterviewStatus status, bool isIncrease)
        {
            var incCount = isIncrease ? 1 : -1;

            summary.SupervisorAssignedCount += status == InterviewStatus.SupervisorAssigned ? incCount : 0;
            summary.InterviewerAssignedCount += status == InterviewStatus.InterviewerAssigned ? incCount : 0;
            summary.CompletedCount += status == InterviewStatus.Completed ? incCount : 0;
            summary.ApprovedBySupervisorCount += status == InterviewStatus.ApprovedBySupervisor ? incCount : 0;
            summary.RejectedBySupervisorCount += status == InterviewStatus.RejectedBySupervisor ? incCount : 0;

            summary.ApprovedByHeadquartersCount += status == InterviewStatus.ApprovedByHeadquarters ? incCount : 0;
            summary.RejectedByHeadquartersCount += status == InterviewStatus.RejectedByHeadquarters ? incCount : 0;
        }

        private void AssignToOtherUser(Guid interviewId, Guid interviewerId)
        {
            //var interviewBriefItem = this.interviewBriefStorage.GetById(interviewId);
            ////update old statistics
            //var oldStatistics = this.GetStatisticItem(interviewBriefItem);
            //this.DecreaseStatisticsByStatus(oldStatistics, interviewBriefItem.Status);

            //this.StoreStatisticsItem(interviewBriefItem, oldStatistics);

            //interviewBriefItem.ResponsibleId = interviewerId;
            //var statistics = this.GetStatisticItem(interviewBriefItem) ?? this.CreateNewStatisticsLine(interviewBriefItem);
            //this.IncreaseStatisticsByStatus(statistics, interviewBriefItem.Status);

            ////this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            //this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            //var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            //var statistics = this.GetStatisticItem(interviewBriefItem);

            //this.DecreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            //this.IncreaseStatisticsByStatus(statistics, evnt.Payload.Status);

            //interviewBriefItem.Status = evnt.Payload.Status;
            ////this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            //this.StoreStatisticsItem(interviewBriefItem, statistics);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            this.AssignToOtherUser(evnt.EventSourceId, evnt.Payload.SupervisorId);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var statistics = this.GetStatisticItem(interviewBriefItem, evnt.EventTimeStamp);

            interviewBriefItem.IsDeleted = true;
            //this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            //this.interviewBriefStorage.Remove(evnt.EventSourceId);

            var statistics = this.GetStatisticItem(interviewBriefItem, evnt.EventTimeStamp);
            this.DecreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            this.DecreaseStatisticsByStatus(statistics, InterviewStatus.Deleted);
            this.StoreStatisticsItem(interviewBriefItem, statistics, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var statistics = this.GetStatisticItem(interviewBriefItem, evnt.EventTimeStamp);

            interviewBriefItem.IsDeleted = false;
            //this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(interviewBriefItem, statistics, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            this.AssignToOtherUser(evnt.EventSourceId, evnt.Payload.InterviewerId);
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] { typeof (UserDocument), typeof (QuestionnaireBrowseItem), typeof (InterviewBrief) }; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof (StatisticsLineGroupedByDateAndTemplate) }; }
        }
    }
}