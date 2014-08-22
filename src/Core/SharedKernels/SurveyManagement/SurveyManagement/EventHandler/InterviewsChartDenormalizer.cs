using System;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewsChartDenormalizer : IEventHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewOnClientCreated>,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewerAssigned>
    {
        private readonly IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage;
        private readonly IReadSideRepositoryWriter<InterviewDetailsForChart> interviewBriefStorage;

        public InterviewsChartDenormalizer(
            IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage,
            IReadSideRepositoryWriter<InterviewDetailsForChart> interviewBriefStorage)
        {
            this.statisticsStorage = statisticsStorage;
            this.interviewBriefStorage = interviewBriefStorage;
        }

        private void HandleCreation(Guid eventSourceId, Guid questionnaireId, long questionnaireVersion, DateTime dateTime)
        {
            var statisticsLineGroupedByDateAndTemplate = CreateNewStatisticsLine(questionnaireId, questionnaireVersion, dateTime.Date);
            var interviewDetailsForChart = new InterviewDetailsForChart
            {
                InterviewId = eventSourceId,
                QuestionnaireId =  questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Status = InterviewStatus.Created
            };

            this.interviewBriefStorage.Store(interviewDetailsForChart, eventSourceId);
            this.statisticsStorage.Store(statisticsLineGroupedByDateAndTemplate, GetCombinedStatisticKey(statisticsLineGroupedByDateAndTemplate));
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

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var key = GetCombinedStatisticKey(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            var statistics = this.statisticsStorage.GetById(key);

            if (statistics != null)
            {
                this.DecreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            }
            else
            {
                statistics = this.CreateNewStatisticsLine(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            }

            this.IncreaseStatisticsByStatus(statistics, evnt.Payload.Status);

            interviewBriefItem.Status = evnt.Payload.Status;
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(statistics);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var key = GetCombinedStatisticKey(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            var statistics = this.statisticsStorage.GetById(key);

            if (statistics != null)
            {
                this.DecreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            }
            else
            {
                statistics = this.CreateNewStatisticsLine(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            }

            this.IncreaseStatisticsByStatus(statistics, InterviewStatus.SupervisorAssigned);

            interviewBriefItem.Status = InterviewStatus.SupervisorAssigned;
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(statistics);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interviewBriefItem = this.interviewBriefStorage.GetById(evnt.EventSourceId);
            var key = GetCombinedStatisticKey(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            var statistics = this.statisticsStorage.GetById(key);

            if (statistics != null)
            {
                this.DecreaseStatisticsByStatus(statistics, interviewBriefItem.Status);
            }
            else
            {
                statistics = this.CreateNewStatisticsLine(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            }

            this.IncreaseStatisticsByStatus(statistics, InterviewStatus.InterviewerAssigned);

            interviewBriefItem.Status = InterviewStatus.InterviewerAssigned;
            this.interviewBriefStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(statistics);
        }

        private StatisticsLineGroupedByDateAndTemplate GetStatisticItem(StatisticsLineGroupedByDateAndTemplate interviewBriefItem)
        {
            var key = GetCombinedStatisticKey(interviewBriefItem);
            return this.statisticsStorage.GetById(key);
        }
        
        private void StoreStatisticsItem(StatisticsLineGroupedByDateAndTemplate statistics)
        {
            var key = GetCombinedStatisticKey(statistics);
            this.statisticsStorage.Store(statistics, key);
        }

        private static string GetCombinedStatisticKey(StatisticsLineGroupedByDateAndTemplate statisticsLineGroupedByDateAndTemplate)
        {
            return String.Format("{0}${1}${2}-{3}-{4}",
                statisticsLineGroupedByDateAndTemplate.QuestionnaireId,
                statisticsLineGroupedByDateAndTemplate.QuestionnaireVersion,
                statisticsLineGroupedByDateAndTemplate.Date.Date.Year,
                statisticsLineGroupedByDateAndTemplate.Date.Date.Month,
                statisticsLineGroupedByDateAndTemplate.Date.Date.Day);
        }

        private static string GetCombinedStatisticKey(Guid questionnaireId, long questionnaireVersion, DateTime dateTime)
        {
            return String.Format("{0}${1}${2}-{3}-{4}",
                questionnaireId,
                questionnaireVersion,
                dateTime.Date.Year,
                dateTime.Date.Month,
                dateTime.Date.Day);
        }

        private StatisticsLineGroupedByDateAndTemplate CreateNewStatisticsLine(Guid questionnaireId, long questionnaireVersion, DateTime dateTime)
        {
            return new StatisticsLineGroupedByDateAndTemplate
            {
                Date = dateTime.Date,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion
            };
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] { typeof(UserDocument), typeof(InterviewDetailsForChart) }; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof(StatisticsLineGroupedByDateAndTemplate), typeof(InterviewDetailsForChart) }; }
        }
    }
}