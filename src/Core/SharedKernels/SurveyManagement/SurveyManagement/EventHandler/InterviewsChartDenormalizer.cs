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
            this.ChangeByStatus(statistics, questionnaireDetails, status, -1);
        }

        private void IncreaseStatisticsByStatus(StatisticsLineGroupedByDateAndTemplate statistics, QuestionnaireDetailsForChart questionnaireDetails, InterviewStatus status)
        {
            this.ChangeByStatus(statistics, questionnaireDetails, status, +1);
        }

        private void ChangeByStatus(StatisticsLineGroupedByDateAndTemplate summary, QuestionnaireDetailsForChart questionnaireDetails, InterviewStatus status, int delta)
        {
            switch (status)
            {
                case InterviewStatus.SupervisorAssigned:
                    questionnaireDetails.SupervisorAssignedCount = summary.SupervisorAssignedCount += delta;
                    break;

                case InterviewStatus.InterviewerAssigned:
                    questionnaireDetails.InterviewerAssignedCount = summary.InterviewerAssignedCount += delta;
                    break;

                case InterviewStatus.Completed:
                    questionnaireDetails.CompletedCount = summary.CompletedCount += delta;
                    break;

                case InterviewStatus.ApprovedBySupervisor:
                    questionnaireDetails.ApprovedBySupervisorCount = summary.ApprovedBySupervisorCount += delta;
                    break;

                case InterviewStatus.RejectedBySupervisor:
                    questionnaireDetails.RejectedBySupervisorCount = summary.RejectedBySupervisorCount += delta;
                    break;

                case InterviewStatus.ApprovedByHeadquarters:
                    questionnaireDetails.ApprovedByHeadquartersCount = summary.ApprovedByHeadquartersCount += delta;
                    break;

                case InterviewStatus.RejectedByHeadquarters:
                    questionnaireDetails.RejectedByHeadquartersCount = summary.RejectedByHeadquartersCount += delta;
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
            get { return new Type[]{}; }
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