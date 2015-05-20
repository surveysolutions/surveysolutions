using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    public class ChartStatisticsViewFactory : IChartStatisticsViewFactory
    {
        private readonly IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate> statisticsReader;

        public ChartStatisticsViewFactory(IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate> statisticsReader)
        {
            this.statisticsReader = statisticsReader;
        }

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            StatisticsGroupedByDateAndTemplate collectedStatistics = statisticsReader.GetById(GetStatisticsKey(input.QuestionnaireId, input.QuestionnaireVersion));

            if (collectedStatistics == null || collectedStatistics.StatisticsByDate.Count == 0)
                return new ChartStatisticsView { Lines = new object[0][][] };

            var minCollectedDate = collectedStatistics.StatisticsByDate.Keys.Min();
            var maxCollectedDate = collectedStatistics.StatisticsByDate.Keys.Max();

            var leftDate = minCollectedDate < input.From ? input.From : minCollectedDate;
            var rightDate = maxCollectedDate > input.To ? input.To : maxCollectedDate;

            var selectedRange = new Dictionary<DateTime, QuestionnaireStatisticsForChart>();

            if (leftDate > rightDate)
            {
                if (rightDate < input.From)
                {
                    var lastDay = collectedStatistics.StatisticsByDate.Keys.Max();
                    var statisticsToRepeat = collectedStatistics.StatisticsByDate[lastDay];
                    RepeatLastStatistics(selectedRange, input.From, input.To, statisticsToRepeat);

                    AddReadlStatistics(collectedStatistics, leftDate, maxCollectedDate, selectedRange);
                }
                else
                {
                    RepeatLastStatistics(selectedRange, input.From, input.To, new QuestionnaireStatisticsForChart());
                }
            }
            else
            {
                if (leftDate > input.From)
                {
                    RepeatLastStatistics(selectedRange, input.From, leftDate.AddDays(-1), new QuestionnaireStatisticsForChart());
                }

                if (rightDate < input.To)
                {
                    RepeatLastStatistics(selectedRange, rightDate.AddDays(1), input.To, collectedStatistics.StatisticsByDate[rightDate]);
                }

                AddReadlStatistics(collectedStatistics, leftDate, maxCollectedDate, selectedRange);
            }

           

            return ChartStatisticsView(selectedRange, input.From, input.To);
        }

        private static void AddReadlStatistics(StatisticsGroupedByDateAndTemplate collectedStatistics, DateTime leftDate, DateTime maxCollectedDate,
            Dictionary<DateTime, QuestionnaireStatisticsForChart> selectedRange)
        {
            collectedStatistics.StatisticsByDate
                .Where(x => x.Key >= leftDate.Date && x.Key.Date <= maxCollectedDate.Date)
                .ForEach(x => selectedRange.Add(x.Key, x.Value));
        }

        private void RepeatLastStatistics(Dictionary<DateTime, QuestionnaireStatisticsForChart> selectedRange1, DateTime from, DateTime to, QuestionnaireStatisticsForChart statisticsToRepeat)
        {
            var date = from.Date;
            while (date <= to)
            {
                selectedRange1.Add(date, new QuestionnaireStatisticsForChart(statisticsToRepeat));
                date = date.AddDays(1);
            }
        }

        private static ChartStatisticsView ChartStatisticsView(Dictionary<DateTime, QuestionnaireStatisticsForChart> range, DateTime minCollectedDate, DateTime maxCollectedDate)
        {
            var selectedRange = range
                .OrderBy(x => x.Key)
                .ToList();

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
                From = FormatDate(minCollectedDate),
                To = FormatDate(maxCollectedDate)
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

        private string GetStatisticsKey(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("{0}_{1}$",
                questionnaireId,
                questionnaireVersion.ToString().PadLeft(3, '_'));
        }
    }
}