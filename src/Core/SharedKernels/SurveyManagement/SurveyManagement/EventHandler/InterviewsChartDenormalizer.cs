using System;
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
        IEventHandler<InterviewStatusChanged>
    {
        private readonly IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage;
        private readonly IReadSideRepositoryWriter<InterviewDetailsForChart> interviewDetailsStorage;
        private readonly IReadSideRepositoryWriter<QuestionnaireDetailsForChart> questionnaireDetailsForChart;

        public InterviewsChartDenormalizer(
            IReadSideRepositoryWriter<StatisticsLineGroupedByDateAndTemplate> statisticsStorage,
            IReadSideRepositoryWriter<InterviewDetailsForChart> interviewDetailsStorage,
            IReadSideRepositoryWriter<QuestionnaireDetailsForChart> questionnaireDetailsForChart)
        {
            this.statisticsStorage = statisticsStorage;
            this.interviewDetailsStorage = interviewDetailsStorage;
            this.questionnaireDetailsForChart = questionnaireDetailsForChart;
        }

        private void HandleCreation(Guid eventSourceId, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewDetails = new InterviewDetailsForChart
            {
                InterviewId = eventSourceId,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Status = InterviewStatus.Created
            };
            this.interviewDetailsStorage.Store(interviewDetails, eventSourceId);

            var detailsForChart = new QuestionnaireDetailsForChart
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
            };
            this.questionnaireDetailsForChart.Store(detailsForChart, GetCombinedQKey(questionnaireId, questionnaireVersion));
        }

        private void DecreaseStatisticsByStatus(StatisticsLineGroupedByDateAndTemplate statistics, QuestionnaireDetailsForChart questionnaireDetails, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, questionnaireDetails, status, false);
        }

        private void IncreaseStatisticsByStatus(StatisticsLineGroupedByDateAndTemplate statistics, QuestionnaireDetailsForChart questionnaireDetails, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, questionnaireDetails, status, true);
        }

        private void ChangeByStatus(StatisticsLineGroupedByDateAndTemplate summary, QuestionnaireDetailsForChart questionnaireDetails, InterviewStatus status, bool isIncrease)
        {
            var incCount = isIncrease ? 1 : -1;

            switch (status)
            {
                case InterviewStatus.SupervisorAssigned:
                    summary.SupervisorAssignedCount += incCount;
                    questionnaireDetails.SupervisorAssignedCount = summary.SupervisorAssignedCount;
                    break;

                case InterviewStatus.InterviewerAssigned:
                    summary.InterviewerAssignedCount += incCount;
                    questionnaireDetails.InterviewerAssignedCount = summary.InterviewerAssignedCount;
                    break;

                case InterviewStatus.Completed:
                    summary.CompletedCount += incCount;
                    questionnaireDetails.CompletedCount = summary.CompletedCount;
                    break;

                case InterviewStatus.ApprovedBySupervisor:
                    summary.ApprovedBySupervisorCount += incCount;
                    questionnaireDetails.ApprovedBySupervisorCount = summary.ApprovedBySupervisorCount;
                    break;

                case InterviewStatus.RejectedBySupervisor:
                    summary.RejectedBySupervisorCount += incCount;
                    questionnaireDetails.RejectedBySupervisorCount = summary.RejectedBySupervisorCount;
                    break;

                case InterviewStatus.ApprovedByHeadquarters:
                    summary.ApprovedByHeadquartersCount += incCount;
                    questionnaireDetails.ApprovedByHeadquartersCount = summary.ApprovedByHeadquartersCount;
                    break;

                case InterviewStatus.RejectedByHeadquarters:
                    summary.RejectedByHeadquartersCount += incCount;
                    questionnaireDetails.RejectedByHeadquartersCount = summary.RejectedByHeadquartersCount;
                    break;
            }
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.HandleCreation(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interviewDetails = this.interviewDetailsStorage.GetById(evnt.EventSourceId);
            var questionnaireDetails = this.questionnaireDetailsForChart.GetById(this.GetCombinedQKey(interviewDetails.QuestionnaireId, interviewDetails.QuestionnaireVersion));

            var statisticKey = GetCombinedStatisticKey(interviewDetails.QuestionnaireId, interviewDetails.QuestionnaireVersion, evnt.EventTimeStamp.Date);
            var statistics = this.statisticsStorage.GetById(statisticKey) ?? this.CreateNewStatisticsLine(questionnaireDetails, evnt.EventTimeStamp.Date);

            this.DecreaseStatisticsByStatus(statistics, questionnaireDetails, interviewDetails.Status);
            this.IncreaseStatisticsByStatus(statistics, questionnaireDetails, evnt.Payload.Status);
            this.StoreStatisticsItem(statistics);
            this.questionnaireDetailsForChart.Store(questionnaireDetails, this.GetCombinedQKey(questionnaireDetails.QuestionnaireId, questionnaireDetails.QuestionnaireVersion));

            interviewDetails.Status = evnt.Payload.Status;
            this.interviewDetailsStorage.Store(interviewDetails, interviewDetails.InterviewId);
        }

        private void StoreStatisticsItem(StatisticsLineGroupedByDateAndTemplate statistics)
        {
            var key = GetCombinedStatisticKey(statistics);
            this.statisticsStorage.Store(statistics, key);
        }

        private string GetCombinedStatisticKey(StatisticsLineGroupedByDateAndTemplate statisticsLineGroupedByDateAndTemplate)
        {
            return String.Format("{0}${1}${2}-{3}-{4}",
                statisticsLineGroupedByDateAndTemplate.QuestionnaireId,
                statisticsLineGroupedByDateAndTemplate.QuestionnaireVersion,
                statisticsLineGroupedByDateAndTemplate.Date.Date.Year,
                statisticsLineGroupedByDateAndTemplate.Date.Date.Month,
                statisticsLineGroupedByDateAndTemplate.Date.Date.Day);
        }

        private string GetCombinedStatisticKey(Guid questionnaireId, long questionnaireVersion, DateTime dateTime)
        {
            return String.Format("{0}${1}${2}-{3}-{4}",
                questionnaireId,
                questionnaireVersion,
                dateTime.Date.Year,
                dateTime.Date.Month,
                dateTime.Date.Day);
        }

        private string GetCombinedQKey(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("{0}${1}$",
                questionnaireId,
                questionnaireVersion);
        }

        private StatisticsLineGroupedByDateAndTemplate CreateEmptyStatisticsLine(Guid questionnaireId, long questionnaireVersion,
            DateTime dateTime)
        {
            return new StatisticsLineGroupedByDateAndTemplate
            {
                Date = dateTime.Date,
                DateTicks = dateTime.Date.Ticks,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion
            };
        }

        private StatisticsLineGroupedByDateAndTemplate CreateNewStatisticsLine(QuestionnaireDetailsForChart questionnaireDetails, DateTime date)
        {
            var statisticsLine = this.CreateEmptyStatisticsLine(
                questionnaireDetails.QuestionnaireId,
                questionnaireDetails.QuestionnaireVersion, 
                date);

            statisticsLine.SupervisorAssignedCount = questionnaireDetails.SupervisorAssignedCount;
            statisticsLine.InterviewerAssignedCount = questionnaireDetails.InterviewerAssignedCount;
            statisticsLine.CompletedCount = questionnaireDetails.CompletedCount;
            statisticsLine.ApprovedBySupervisorCount = questionnaireDetails.ApprovedBySupervisorCount;
            statisticsLine.RejectedBySupervisorCount = questionnaireDetails.RejectedBySupervisorCount;
            statisticsLine.ApprovedByHeadquartersCount = questionnaireDetails.ApprovedByHeadquartersCount;
            statisticsLine.RejectedByHeadquartersCount = questionnaireDetails.RejectedByHeadquartersCount;

            return statisticsLine;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] { typeof (InterviewDetailsForChart) }; }
        }

        public Type[] BuildsViews
        {
            get
            {
                return new[]
                {
                    typeof (StatisticsLineGroupedByDateAndTemplate), 
                    typeof (InterviewDetailsForChart),
                    typeof (QuestionnaireDetailsForChart)
                };
            }
        }
    }
}