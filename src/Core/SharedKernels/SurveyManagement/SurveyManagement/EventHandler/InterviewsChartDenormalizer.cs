using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewsChartDenormalizer : BaseDenormalizer,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewOnClientCreated>,
        IEventHandler<InterviewStatusChanged>
    {
        private readonly IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate> statisticsStorage;
        private readonly IReadSideKeyValueStorage<InterviewDetailsForChart> chartItemsStorage;

        public InterviewsChartDenormalizer(
            IReadSideKeyValueStorage<InterviewDetailsForChart> chartItemsStorage,
            IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate> statisticsStorage)
        {
            this.chartItemsStorage = chartItemsStorage;
            this.statisticsStorage = statisticsStorage;
        }

        private void HandleCreation(Guid eventSourceId, Guid questionnaireId, long questionnaireVersion, DateTime date)
        {
            var interviewDetails = new InterviewDetailsForChart
            {
                InterviewId = eventSourceId,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Status = InterviewStatus.Created
            };
            this.chartItemsStorage.Store(interviewDetails, eventSourceId);

            var statisticsKey = this.GetStatisticsKey(questionnaireId, questionnaireVersion);

            var stat = this.statisticsStorage.GetById(statisticsKey) ?? new StatisticsGroupedByDateAndTemplate();

            if (!stat.StatisticsByDate.ContainsKey(date))
            {
                CreateStatisticsRecord(stat, date);
            }

            stat.StatisticsByDate
                .Where(x => x.Key.Date >= date)
                .ForEach(x => x.Value.CreatedCount++);

            this.statisticsStorage.Store(stat, statisticsKey);
        }

        private void DecreaseStatisticsByStatus(QuestionnaireStatisticsForChart statistics, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, status, -1);
        }

        private void IncreaseStatisticsByStatus(QuestionnaireStatisticsForChart statistics, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, status, +1);
        }

        private void ChangeByStatus(QuestionnaireStatisticsForChart statistics, InterviewStatus status, int delta)
        {
            switch (status)
            {
                case InterviewStatus.Created:
                    statistics.CreatedCount += delta;
                    break;

                case InterviewStatus.SupervisorAssigned:
                    statistics.SupervisorAssignedCount += delta;
                    break;

                case InterviewStatus.InterviewerAssigned:
                    statistics.InterviewerAssignedCount += delta;
                    break;

                case InterviewStatus.Completed:
                    statistics.CompletedCount += delta;
                    break;

                case InterviewStatus.ApprovedBySupervisor:
                    statistics.ApprovedBySupervisorCount += delta;
                    break;

                case InterviewStatus.RejectedBySupervisor:
                    statistics.RejectedBySupervisorCount += delta;
                    break;

                case InterviewStatus.ApprovedByHeadquarters:
                    statistics.ApprovedByHeadquartersCount += delta;
                    break;

                case InterviewStatus.RejectedByHeadquarters:
                    statistics.RejectedByHeadquartersCount += delta;
                    break;

                default:
                    statistics.OtherStatusesCount += delta;
                    break;
            }
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp.Date);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp.Date);
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion, evnt.EventTimeStamp.Date);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interviewDetails = this.chartItemsStorage.GetById(evnt.EventSourceId);
            var previousStatus = interviewDetails.Status;
            interviewDetails.Status = evnt.Payload.Status;
            this.chartItemsStorage.Store(interviewDetails, interviewDetails.InterviewId);

            var date = evnt.EventTimeStamp.Date;

            var statisticsKey = this.GetStatisticsKey(interviewDetails.QuestionnaireId, interviewDetails.QuestionnaireVersion);

            var stat = this.statisticsStorage.GetById(statisticsKey);

            if (!stat.StatisticsByDate.ContainsKey(date))
            {
                CreateStatisticsRecord(stat, date);
            }

            stat.StatisticsByDate
                .Where(x => x.Key.Date >= date.Date)
                .ForEach(x => DecreaseStatisticsByStatus(x.Value, previousStatus));

            stat.StatisticsByDate
                .Where(x => x.Key.Date >= date.Date)
                .ForEach(x => this.IncreaseStatisticsByStatus(x.Value, evnt.Payload.Status));


            this.statisticsStorage.Store(stat, statisticsKey);
        }

        private static void CreateStatisticsRecord(StatisticsGroupedByDateAndTemplate stat, DateTime date)
        {
            if (stat.StatisticsByDate.Keys.Any(x => x < date))
            {
                var closestDate = stat.StatisticsByDate.Keys.Where(x => x < date).Max();
                var statisticsToDublicate = stat.StatisticsByDate[closestDate];
                closestDate = closestDate.AddDays(1);
                while (closestDate <= date)
                {
                    stat.StatisticsByDate.Add(closestDate.Date, new QuestionnaireStatisticsForChart(statisticsToDublicate));
                    closestDate = closestDate.AddDays(1);
                }
            }
            else
            {
                // something wrong
                stat.StatisticsByDate.Add(date.Date, new QuestionnaireStatisticsForChart());
            }
        }

        private string GetStatisticsKey(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("{0}_{1}$",
                questionnaireId,
                questionnaireVersion.ToString().PadLeft(3, '_'));
        }

        public override object[] Writers
        {
            get { return new object[] { this.chartItemsStorage, this.statisticsStorage }; }
        }
    }

}