using System;
using System.Collections.Generic;
using System.Linq;
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

            //if (leftDate > rightDate)
            //    return new ChartStatisticsView { Lines = new object[0][][] };

            var lines = new List<object[][]>();

            var selectedRange = collectedStatistics.StatisticsByDate
               // .Where(x => x.Key > leftDate.Date && x.Key.Date < maxCollectedDate.Date)
                .OrderBy(x => x.Key)
                .ToList();

            lines.Add(selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.SupervisorAssignedCount }).ToArray());
            lines.Add(selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.InterviewerAssignedCount }).ToArray());
            lines.Add(selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.CompletedCount }).ToArray());
            lines.Add(selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.RejectedBySupervisorCount }).ToArray());
            lines.Add(selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.ApprovedBySupervisorCount }).ToArray());
            lines.Add(selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.RejectedByHeadquartersCount }).ToArray());
            lines.Add(selectedRange.Select(x => new object[] { FormatDate(x.Key), x.Value.ApprovedByHeadquartersCount }).ToArray());

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