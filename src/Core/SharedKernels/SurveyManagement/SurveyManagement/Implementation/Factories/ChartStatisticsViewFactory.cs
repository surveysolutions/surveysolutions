using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Extensions;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    public class ChartStatisticsViewFactory : IChartStatisticsViewFactory
    {
        private readonly IReadSideRepositoryReader<StatisticsGroupedByDateAndTemplate> statisticsReader;

        public ChartStatisticsViewFactory(IReadSideRepositoryReader<StatisticsGroupedByDateAndTemplate> statisticsReader)
        {
            this.statisticsReader = statisticsReader;
        }

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            var collectedStatistics = statisticsReader.GetById(GetStatisticsKey(input.QuestionnaireId, input.QuestionnaireVersion));

            if (collectedStatistics.StatisticsByDate.Count == 0)
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
            }

            collectedStatistics.StatisticsByDate
             .Where(x => x.Key > leftDate.Date && x.Key.Date < maxCollectedDate.Date)
             .ForEach(x => selectedRange.Add(x.Key, x.Value));

            return ChartStatisticsView(selectedRange, minCollectedDate, maxCollectedDate);
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

            var lines = new List<object[][]>
            {
                selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.SupervisorAssignedCount }).ToArray(),
                selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.InterviewerAssignedCount }).ToArray(),
                selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.CompletedCount }).ToArray(),
                selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.RejectedBySupervisorCount }).ToArray(),
                selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.ApprovedBySupervisorCount }).ToArray(),
                selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.RejectedByHeadquartersCount }).ToArray(),
                selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.ApprovedByHeadquartersCount }).ToArray()
            };

            return new ChartStatisticsView
            {
                Lines = lines.ToArray(),
                From = FormatDate(minCollectedDate),
                To = FormatDate(maxCollectedDate)
            };
        }

        private static string FormatDate(DateTime x)
        {
            return x.ToString("MM/dd/yyyy");
        }

        private string GetStatisticsKey(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("{0}_{1}$",
                questionnaireId,
                questionnaireVersion.ToString().PadLeft(3, '_'));
        }
    }
}