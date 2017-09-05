using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class ChartStatisticsViewFactory : IChartStatisticsViewFactory
    {
        private readonly IOldschoolChartStatisticsDataProvider oldschoolChartStatisticsDataProvider;

        public ChartStatisticsViewFactory(IOldschoolChartStatisticsDataProvider oldschoolChartStatisticsDataProvider)
        {
            this.oldschoolChartStatisticsDataProvider = oldschoolChartStatisticsDataProvider;
        }

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            StatisticsGroupedByDateAndTemplate collectedStatistics = this.oldschoolChartStatisticsDataProvider.GetStatisticsInOldFormat(input.QuestionnaireId, input.QuestionnaireVersion);

            if (collectedStatistics == null || collectedStatistics.StatisticsByDate.Count == 0)
                return new ChartStatisticsView { Lines = new object[0][][] };

            var minCollectedDate = collectedStatistics.StatisticsByDate.Keys.Min();
            var maxCollectedDate = collectedStatistics.StatisticsByDate.Keys.Max();

            var leftEdge = new[] { input.From ?? minCollectedDate.AddDays(-1), input.To ?? minCollectedDate.AddDays(-1) }.Min();
            var rightEdge = new[] { input.From ?? maxCollectedDate, input.To ?? maxCollectedDate }.Max();

            return CreateChartStatisticsView(collectedStatistics.StatisticsByDate, leftEdge, rightEdge, minCollectedDate);
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

            IEnumerable<IEnumerable<(DateTime date, int count)>> rangeLines = new[]
            {
                selectedRange.Select(x => (x.Key, x.Value.SupervisorAssignedCount)),
                selectedRange.Select(x => (x.Key, x.Value.InterviewerAssignedCount)),
                selectedRange.Select(x => (x.Key, x.Value.CompletedCount)),
                selectedRange.Select(x => (x.Key, x.Value.RejectedBySupervisorCount)),
                selectedRange.Select(x => (x.Key, x.Value.ApprovedBySupervisorCount)),
                selectedRange.Select(x => (x.Key, x.Value.RejectedByHeadquartersCount)),
                selectedRange.Select(x => (x.Key, x.Value.ApprovedByHeadquartersCount)),
            };

            rangeLines = RemoveEmptyEndingLines(rangeLines);

            var chartLines = rangeLines.Select(ToChartLine).ToArray();

            return new ChartStatisticsView
            {
                Lines = chartLines,
                From = FormatDate(start),
                To = FormatDate(stop),
                StartDate = FormatDate(pointToBeSetGraphLooksOk)
            };
        }

        private static IEnumerable<IEnumerable<(DateTime date, int count)>> RemoveEmptyEndingLines(
            IEnumerable<IEnumerable<(DateTime date, int count)>> rangeLines)
        {
            return rangeLines
                .Reverse()
                .SkipWhile(line => line.All(point => point.count == 0))
                .Reverse();
        }

        private static object[][] ToChartLine(IEnumerable<(DateTime date, int count)> rangeLine)
        {
            return rangeLine
                .Select(x => new object[] { FormatDate(x.date), x.count })
                .ToArray();
        }

        private static string FormatDate(DateTime x)
        {
            return x.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}