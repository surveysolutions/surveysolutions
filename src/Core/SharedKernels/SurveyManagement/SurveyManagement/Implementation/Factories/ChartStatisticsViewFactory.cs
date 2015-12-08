using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    public class ChartStatisticsViewFactory : IChartStatisticsViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;

        public ChartStatisticsViewFactory(IQueryableReadSideRepositoryReader<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage)
        {
            this.cumulativeReportStatusChangeStorage = cumulativeReportStatusChangeStorage;
        }

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            StatisticsGroupedByDateAndTemplate collectedStatistics = this.GetOldSchoolStats(input.QuestionnaireId, input.QuestionnaireVersion);

            if (collectedStatistics == null || collectedStatistics.StatisticsByDate.Count == 0)
                return new ChartStatisticsView { Lines = new object[0][][] };

            var minCollectedDate = collectedStatistics.StatisticsByDate.Keys.Min();
            var maxCollectedDate = collectedStatistics.StatisticsByDate.Keys.Max();

            var leftEdge = new[] { input.From ?? minCollectedDate.AddDays(-1), input.To ?? minCollectedDate.AddDays(-1) }.Min();
            var rightEdge = new[] { input.From ?? maxCollectedDate, input.To ?? maxCollectedDate }.Max();

            return CreateChartStatisticsView(collectedStatistics.StatisticsByDate, leftEdge, rightEdge, minCollectedDate);
        }

        private StatisticsGroupedByDateAndTemplate GetOldSchoolStats(Guid questionnaireId, long questionnaireVersion)
        {
            var count = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Count());

            if (count == 0)
                return null;

            var minDate = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Min(change => change.Date));

            var maxDate = this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Max(change => change.Date));

            var result = new StatisticsGroupedByDateAndTemplate();

            DateTime date = minDate;
            while (date <= maxDate)
            {
                result.StatisticsByDate.Add(date,
                    this.GetOldSchoolStatsForDate(questionnaireId, questionnaireVersion, date));

                date = date.AddDays(1);
            }

            return result;
        }

        private QuestionnaireStatisticsForChart GetOldSchoolStatsForDate(Guid questionnaireId, long questionnaireVersion, DateTime date)
        {
            return new QuestionnaireStatisticsForChart
            {
                CreatedCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.Created),
                SupervisorAssignedCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.SupervisorAssigned),
                InterviewerAssignedCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.InterviewerAssigned),
                CompletedCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.Completed),
                ApprovedBySupervisorCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.ApprovedBySupervisor),
                RejectedBySupervisorCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.RejectedBySupervisor),
                ApprovedByHeadquartersCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.ApprovedByHeadquarters),
                RejectedByHeadquartersCount = this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.RejectedByHeadquarters),
                OtherStatusesCount = 0, // TODO: this.GetOldSchoolCountForDateAndStatus(questionnaireId, questionnaireVersion, date, InterviewStatus.),
            };
        }

        private int GetOldSchoolCountForDateAndStatus(Guid questionnaireId, long questionnaireVersion, DateTime date, InterviewStatus status)
        {
            return this.cumulativeReportStatusChangeStorage.Query(_ => _
                .Where(change => change.QuestionnaireId == questionnaireId)
                .Where(change => change.QuestionnaireVersion == questionnaireVersion)
                .Where(change => change.Date <= date)
                .Where(change => change.Status == status)
                .Sum(change => (int?) change.ChangeValue) ?? 0); // https://nhibernate.jira.com/browse/NH-2130
        }

        private static ChartStatisticsView CreateChartStatisticsView(Dictionary<DateTime, QuestionnaireStatisticsForChart> range, DateTime start, DateTime stop, DateTime dataStart)
        {
            if (start > stop)
                new ChartStatisticsView { Lines = new object[0][][] };

            var startValueToFill = range.LastOrDefault(x => x.Key <= start).Value ?? new QuestionnaireStatisticsForChart();
            var stopValueToFill = range.LastOrDefault(x => x.Key <= stop).Value ?? new QuestionnaireStatisticsForChart();

            var currentDate = start.Date;
            Dictionary<DateTime, QuestionnaireStatisticsForChart> selectedRange = new Dictionary<DateTime, QuestionnaireStatisticsForChart>();

            var pointToBeSetGraphLooksOk = dataStart.AddDays(-1);

            selectedRange.Add(currentDate, range.ContainsKey(currentDate) ? range[currentDate] : startValueToFill);
            currentDate = currentDate.AddDays(1);

            while (currentDate < stop)
            {
                if (range.ContainsKey(currentDate))
                    selectedRange.Add(currentDate, range[currentDate]);
                else if (currentDate == pointToBeSetGraphLooksOk)
                    selectedRange.Add(currentDate, new QuestionnaireStatisticsForChart());

                currentDate = currentDate.AddDays(1);
            }

            if (start != stop)
                selectedRange.Add(stop, range.ContainsKey(stop) ? range[stop] : stopValueToFill);

            IEnumerable<IEnumerable<Tuple<DateTime, int>>> rangeLines = new[]
            {
                selectedRange.Select(x => Tuple.Create(x.Key, x.Value.SupervisorAssignedCount)),
                selectedRange.Select(x => Tuple.Create(x.Key, x.Value.InterviewerAssignedCount)),
                selectedRange.Select(x => Tuple.Create(x.Key, x.Value.CompletedCount)),
                selectedRange.Select(x => Tuple.Create(x.Key, x.Value.RejectedBySupervisorCount)),
                selectedRange.Select(x => Tuple.Create(x.Key, x.Value.ApprovedBySupervisorCount)),
                selectedRange.Select(x => Tuple.Create(x.Key, x.Value.RejectedByHeadquartersCount)),
                selectedRange.Select(x => Tuple.Create(x.Key, x.Value.ApprovedByHeadquartersCount)),
            };

            rangeLines = RemoveEmptyEndingLines(rangeLines);

            var chartLines = rangeLines.Select(ToChartLine).ToArray();

            return new ChartStatisticsView
            {
                Lines = chartLines,
                From = FormatDate(start),
                To = FormatDate(stop)
            };
        }

        private static IEnumerable<IEnumerable<Tuple<DateTime, int>>> RemoveEmptyEndingLines(
            IEnumerable<IEnumerable<Tuple<DateTime, int>>> rangeLines)
        {
            return rangeLines
                .Reverse()
                .SkipWhile(line => line.All(point => point.Item2 == 0))
                .Reverse();
        }

        private static object[][] ToChartLine(IEnumerable<Tuple<DateTime, int>> rangeLine)
        {
            return rangeLine
                .Select(x => new object[] { FormatDate(x.Item1), x.Item2 })
                .ToArray();
        }

        private static string FormatDate(DateTime x)
        {
            return x.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        }
    }
}