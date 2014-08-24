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
        private readonly IReadSideRepositoryWriter<InterviewDetailsForChart> interviewDetailsStorage;

        public InterviewsChartDenormalizer(
            IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage,
            IReadSideRepositoryWriter<InterviewDetailsForChart> interviewDetailsStorage)
        {
            this.statisticsStorage = statisticsStorage;
            this.interviewDetailsStorage = interviewDetailsStorage;
        }

        private void HandleCreation(Guid eventSourceId, Guid questionnaireId, long questionnaireVersion, DateTime dateTime)
        {
            var statisticsLine = this.CreateEmptyStatisticsLine(questionnaireId, questionnaireVersion, dateTime.Date);
            var interviewDetailsForChart = new InterviewDetailsForChart
            {
                InterviewId = eventSourceId,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Status = InterviewStatus.Created
            };

            this.interviewDetailsStorage.Store(interviewDetailsForChart, eventSourceId);
            this.statisticsStorage.Store(statisticsLine, GetCombinedStatisticKey(statisticsLine));
        }

        private void DecreaseStatisticsByStatus(StatisticsLineGroupedByDateAndTemplate statistics, InterviewDetailsForChart detailsForChart, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, detailsForChart, status, false);
        }

        private void IncreaseStatisticsByStatus(StatisticsLineGroupedByDateAndTemplate statistics, InterviewDetailsForChart detailsForChart, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, detailsForChart, status, true);
        }

        private void ChangeByStatus(StatisticsLineGroupedByDateAndTemplate summary, InterviewDetailsForChart detailsForChart, InterviewStatus status, bool isIncrease)
        {
            var incCount = isIncrease ? 1 : -1;

            detailsForChart.SupervisorAssignedCount = summary.SupervisorAssignedCount += status == InterviewStatus.SupervisorAssigned ? incCount : 0;
            detailsForChart.InterviewerAssignedCount = summary.InterviewerAssignedCount += status == InterviewStatus.InterviewerAssigned ? incCount : 0;
            detailsForChart.CompletedCount = summary.CompletedCount += status == InterviewStatus.Completed ? incCount : 0;
            detailsForChart.ApprovedBySupervisorCount = summary.ApprovedBySupervisorCount += status == InterviewStatus.ApprovedBySupervisor ? incCount : 0;
            detailsForChart.RejectedBySupervisorCount = summary.RejectedBySupervisorCount += status == InterviewStatus.RejectedBySupervisor ? incCount : 0;
            detailsForChart.ApprovedByHeadquartersCount = summary.ApprovedByHeadquartersCount += status == InterviewStatus.ApprovedByHeadquarters ? incCount : 0;
            detailsForChart.RejectedByHeadquartersCount = summary.RejectedByHeadquartersCount += status == InterviewStatus.RejectedByHeadquarters ? incCount : 0;
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
            var details = this.interviewDetailsStorage.GetById(evnt.EventSourceId);
            var key = GetCombinedStatisticKey(details.QuestionnaireId, details.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            var statistics = this.statisticsStorage.GetById(key) ?? this.CreateNewStatisticsLine(details, evnt.EventTimeStamp.Date);

            this.DecreaseStatisticsByStatus(statistics, details, details.Status);
            this.IncreaseStatisticsByStatus(statistics, details, evnt.Payload.Status);

            details.Status = evnt.Payload.Status;
            this.interviewDetailsStorage.Store(details, details.InterviewId);
            this.StoreStatisticsItem(statistics);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interviewBriefItem = this.interviewDetailsStorage.GetById(evnt.EventSourceId);
            var key = GetCombinedStatisticKey(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion,
                evnt.EventTimeStamp.Date);
            var statistics = this.statisticsStorage.GetById(key);

            if (statistics != null)
            {
                //this.DecreaseStatisticsByStatus(statistics, interviewBriefItem, interviewBriefItem.Status);
            }
            else
            {
                statistics = this.CreateNewStatisticsLine(interviewBriefItem, evnt.EventTimeStamp.Date);
            }

            this.IncreaseStatisticsByStatus(statistics, interviewBriefItem, InterviewStatus.SupervisorAssigned);

            interviewBriefItem.Status = InterviewStatus.SupervisorAssigned;
            this.interviewDetailsStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(statistics);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interviewBriefItem = this.interviewDetailsStorage.GetById(evnt.EventSourceId);
            var key = GetCombinedStatisticKey(interviewBriefItem.QuestionnaireId, interviewBriefItem.QuestionnaireVersion,
                evnt.EventTimeStamp.Date);
            var statistics = this.statisticsStorage.GetById(key);

            if (statistics != null)
            {
                //this.DecreaseStatisticsByStatus(statistics, interviewBriefItem, interviewBriefItem.Status);
            }
            else
            {
                statistics = this.CreateNewStatisticsLine(interviewBriefItem, evnt.EventTimeStamp.Date);
            }

            this.IncreaseStatisticsByStatus(statistics, interviewBriefItem, InterviewStatus.InterviewerAssigned);

            interviewBriefItem.Status = InterviewStatus.InterviewerAssigned;
            this.interviewDetailsStorage.Store(interviewBriefItem, interviewBriefItem.InterviewId);
            this.StoreStatisticsItem(statistics);
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

        private StatisticsLineGroupedByDateAndTemplate CreateEmptyStatisticsLine(Guid questionnaireId, long questionnaireVersion, DateTime dateTime)
        {
            return new StatisticsLineGroupedByDateAndTemplate
            {
                Date = dateTime.Date,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion
            };
        }

        private StatisticsLineGroupedByDateAndTemplate CreateNewStatisticsLine(InterviewDetailsForChart interviewDetailsForChart, DateTime date)
        {
            var statisticsLine = this.CreateEmptyStatisticsLine(interviewDetailsForChart.QuestionnaireId, interviewDetailsForChart.QuestionnaireVersion, date);
            
            statisticsLine.SupervisorAssignedCount = interviewDetailsForChart.SupervisorAssignedCount;
            statisticsLine.InterviewerAssignedCount = interviewDetailsForChart.InterviewerAssignedCount;
            statisticsLine.CompletedCount = interviewDetailsForChart.CompletedCount;
            statisticsLine.ApprovedBySupervisorCount = interviewDetailsForChart.ApprovedBySupervisorCount;
            statisticsLine.RejectedBySupervisorCount = interviewDetailsForChart.RejectedBySupervisorCount;
            statisticsLine.ApprovedByHeadquartersCount = interviewDetailsForChart.ApprovedByHeadquartersCount;
            statisticsLine.RejectedByHeadquartersCount = interviewDetailsForChart.RejectedByHeadquartersCount;

            return statisticsLine;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] { typeof (UserDocument), typeof (InterviewDetailsForChart) }; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof (StatisticsLineGroupedByDateAndTemplate), typeof (InterviewDetailsForChart) }; }
        }
    }
}