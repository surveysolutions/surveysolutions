using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class ChartStatisticsFactory : IChartStatisticsFactory
    {
        private class IntermediateCalculationsData
        {
            public IntermediateCalculationsData(int length)
            {
                this.Ticks = new string[length, 2];
                this.SupervisorAssigned = new int[length];
                this.InterviewerAssigned = new int[length];
                this.Completed = new int[length];
                this.RejectedBySupervisor = new int[length];
                this.ApprovedBySupervisor = new int[length];
                this.RejectedByHeadquarters = new int[length];
                this.ApprovedByHeadquarters = new int[length];
            }

            public string[,] Ticks { get; private set; }
            public int[] SupervisorAssigned { get; private set; }
            public int[] InterviewerAssigned { get; private set; }
            public int[] Completed { get; private set; }
            public int[] RejectedBySupervisor { get; private set; }
            public int[] ApprovedBySupervisor { get; private set; }
            public int[] RejectedByHeadquarters { get; private set; }
            public int[] ApprovedByHeadquarters { get; private set; }

            public void FillDayWithStatistics(int dayIndex, StatisticsLineGroupedByDateAndTemplate statistics, DateTime date)
            {
                this.Ticks[dayIndex, 0] = (dayIndex + 1).ToString(CultureInfo.InvariantCulture);
                this.Ticks[dayIndex, 1] = date.ToShortDateString();

                if (statistics != null)
                {
                    this.SupervisorAssigned[dayIndex] = statistics.SupervisorAssignedCount;
                    this.InterviewerAssigned[dayIndex] = statistics.InterviewerAssignedCount;
                    this.Completed[dayIndex] = statistics.CompletedCount;
                    this.RejectedBySupervisor[dayIndex] = statistics.RejectedBySupervisorCount;
                    this.ApprovedBySupervisor[dayIndex] = statistics.ApprovedBySupervisorCount;
                    this.RejectedByHeadquarters[dayIndex] = statistics.RejectedByHeadquartersCount;
                    this.ApprovedByHeadquarters[dayIndex] = statistics.ApprovedByHeadquartersCount;
                }
            }

            public void FillDayWithPreviousDayData(int dayIndex, DateTime date)
            {
                this.Ticks[dayIndex, 0] = (dayIndex + 1).ToString(CultureInfo.InvariantCulture);
                this.Ticks[dayIndex, 1] = date.ToShortDateString();

                if (dayIndex > 0)
                {
                    this.SupervisorAssigned[dayIndex] = this.SupervisorAssigned[dayIndex - 1];
                    this.InterviewerAssigned[dayIndex] = this.InterviewerAssigned[dayIndex - 1];
                    this.Completed[dayIndex] = this.Completed[dayIndex - 1];
                    this.RejectedBySupervisor[dayIndex] = this.RejectedBySupervisor[dayIndex - 1];
                    this.ApprovedBySupervisor[dayIndex] = this.ApprovedBySupervisor[dayIndex - 1];
                    this.RejectedByHeadquarters[dayIndex] = this.RejectedByHeadquarters[dayIndex - 1];
                    this.ApprovedByHeadquarters[dayIndex] = this.ApprovedByHeadquarters[dayIndex - 1];
                }
            }
        }

        private readonly IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> statisticsReader;

        public ChartStatisticsFactory(IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> statisticsReader)
        {
            this.statisticsReader = statisticsReader;
        }

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            List<StatisticsLineGroupedByDateAndTemplate> collectedStatistics = this.GetOrderedQuestionnaireStatistics(input.QuestionnaireId, input.QuestionnaireVersion);

            if (collectedStatistics.Count == 0)
                return new ChartStatisticsView { Ticks = new string[,] { }, Stats = new int[][] { } };

            DateTime firstDate = collectedStatistics.First().Date;
            DateTime lastDate = input.CurrentDate;
            int totalDaysCount = Convert.ToInt32((lastDate - firstDate).TotalDays) + 1;

            int filterRangeDaysCount = Convert.ToInt32((input.To - input.From).TotalDays) + 1;
            int resultDaysCount = Math.Min(totalDaysCount, filterRangeDaysCount);

            if (resultDaysCount <= 0)
                return new ChartStatisticsView { Ticks = new string[,] { }, Stats = new int[][] { } };

            var firstDayOnChart = Max(input.From, firstDate);


            var intermediateData = new IntermediateCalculationsData(resultDaysCount);

            FillFirstDayWithStatisticsCollectedUpToThatDay(intermediateData, firstDayOnChart, collectedStatistics);

            for (int dayIndex = 0; dayIndex < resultDaysCount; dayIndex++)
            {
                var date = firstDayOnChart.AddDays(dayIndex);

                FillDayWithStatisticsOrPreviousDayData(intermediateData, dayIndex, date, collectedStatistics);
            }


            return ToChartStatisticsView(intermediateData);
        }

        private static void FillDayWithStatisticsOrPreviousDayData(IntermediateCalculationsData intermediateData, int dayIndex, DateTime date, List<StatisticsLineGroupedByDateAndTemplate> questionnaireStats)
        {
            StatisticsLineGroupedByDateAndTemplate statistics = questionnaireStats.Find(_ => AreDayKeysEqual(_.Date, date));

            if (statistics != null)
            {
                intermediateData.FillDayWithStatistics(dayIndex, statistics, date);
            }
            else
            {
                intermediateData.FillDayWithPreviousDayData(dayIndex, date);
            }
        }

        private static void FillFirstDayWithStatisticsCollectedUpToThatDay(IntermediateCalculationsData intermediateData, DateTime day, List<StatisticsLineGroupedByDateAndTemplate> questionnaireStats)
        {
            var statistics = GetLastExistingStatisticsForDay(day, questionnaireStats);

            intermediateData.FillDayWithStatistics(0, statistics, day);
        }

        private static StatisticsLineGroupedByDateAndTemplate GetLastExistingStatisticsForDay(DateTime day, List<StatisticsLineGroupedByDateAndTemplate> questionnaireStats)
        {
            return questionnaireStats.LastOrDefault(stats => stats.Date < day || AreDayKeysEqual(stats.Date, day));
        }

        private List<StatisticsLineGroupedByDateAndTemplate> GetOrderedQuestionnaireStatistics(Guid questionnaireId, long? questionnaireVersion)
        {
            return this.statisticsReader.Query(_ => _
                .Where(s => s.QuestionnaireId == questionnaireId && s.QuestionnaireVersion == questionnaireVersion)
                .OrderBy(o => o.Date)
                .ToList()
            );
        }

        private static ChartStatisticsView ToChartStatisticsView(IntermediateCalculationsData intermediateData)
        {
            return new ChartStatisticsView
            {
                Stats = new[]
                {
                    intermediateData.SupervisorAssigned,
                    intermediateData.InterviewerAssigned,
                    intermediateData.Completed,
                    intermediateData.RejectedBySupervisor,
                    intermediateData.ApprovedBySupervisor,
                    intermediateData.RejectedByHeadquarters,
                    intermediateData.ApprovedByHeadquarters,
                },
                Ticks = intermediateData.Ticks,
            };
        }

        private static DateTime Max(DateTime first, DateTime second)
        {
            return first > second ? first : second;
        }

        private static bool AreDayKeysEqual(DateTime first, DateTime second)
        {
            return first.ToShortDateString() == second.ToShortDateString();
        }
    }
}