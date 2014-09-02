using System;
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
        private readonly IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> statisticsReader;

        public ChartStatisticsFactory(IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> statisticsReader)
        {
            this.statisticsReader = statisticsReader;
        }

        public ChartStatisticsView Load(ChartStatisticsInputModel input)
        {
            var daysCount = 0;
            var firstDate = DateTime.Now;
            var statisticsView = new ChartStatisticsView();

            var questionnaireStats = this.statisticsReader
                .Query(
                _ => _.Where(
                        s => (
                            s.QuestionnaireId == input.QuestionnaireId &&
                            s.QuestionnaireVersion == input.QuestionnaireVersion
                        )
                    )
                    .OrderBy(o => o.Date)
                    .ToList()
            );

            if (questionnaireStats.Count != 0)
            {
                firstDate = questionnaireStats.First().Date;
                var lastDate = input.CurrentDate;

                daysCount = Convert.ToInt32((lastDate - firstDate).TotalDays) + 1;
            }

            var filterRangeInDays = Convert.ToInt32((input.To - input.From).TotalDays) + 1;
            var resultArrayLength = daysCount >= filterRangeInDays ? filterRangeInDays : daysCount;

            if (resultArrayLength <= 0)
                return new ChartStatisticsView { Ticks = new string[,]{}, Stats = new int[][]{}};

            var supervisorAssignedData = new int[resultArrayLength];
            var interviewerAssignedData = new int[resultArrayLength];
            var completedData = new int[resultArrayLength];
            var rejectedBySupervisor = new int[resultArrayLength];
            var approvedBySupervisor = new int[resultArrayLength];
            var rejectedByHeadquarters = new int[resultArrayLength];
            var approvedByHeadquarters = new int[resultArrayLength];

            int[][] stats = {supervisorAssignedData, interviewerAssignedData, completedData, rejectedBySupervisor, approvedBySupervisor, rejectedByHeadquarters, approvedByHeadquarters};
            var ticks = new string[resultArrayLength, 2];

            var j = 0;
            for (var i = 0; i < daysCount; i++)
            {
                var internalDate = firstDate.AddDays(i);
                var internalDateKey = internalDate.ToShortDateString();
                
                var dayStats = questionnaireStats.Find( _ => _.Date.ToShortDateString().Equals(internalDateKey));
                var rowNumber = (j + 1).ToString(CultureInfo.InvariantCulture);

                if (dayStats != null)
                {
                    ticks[j, 0] = rowNumber;
                    ticks[j, 1] = dayStats.Date.ToShortDateString();

                    CopyStats(j, supervisorAssignedData, dayStats, interviewerAssignedData, completedData, rejectedBySupervisor, approvedBySupervisor, rejectedByHeadquarters, approvedByHeadquarters);
                }
                else
                {
                    ticks[j, 0] = rowNumber;
                    ticks[j, 1] = internalDateKey;

                    if (j > 0)
                    {
                        CopyStatsFromPreviousDay(j, supervisorAssignedData, interviewerAssignedData, completedData, rejectedBySupervisor, approvedBySupervisor, rejectedByHeadquarters, approvedByHeadquarters);
                    }
                }

                if (internalDate < input.From)
                {
                    continue;
                }

                if (input.To < internalDate)
                {
                    break;
                }

                j++;
                
                if (j >= resultArrayLength)
                {
                    break;
                }
            }
            
            statisticsView.Stats = stats;
            statisticsView.Ticks = ticks;

            return statisticsView;
        }

        private static void CopyStats(int j, int[] supervisorAssignedData, StatisticsLineGroupedByDateAndTemplate dayStats,
            int[] interviewerAssignedData, int[] completedData, int[] rejectedBySupervisor, int[] approvedBySupervisor, int[] rejectedByHeadquarters,
            int[] approvedByHeadquarters)
        {
            supervisorAssignedData[j] = dayStats.SupervisorAssignedCount;
            interviewerAssignedData[j] = dayStats.InterviewerAssignedCount;
            completedData[j] = dayStats.CompletedCount;
            rejectedBySupervisor[j] = dayStats.RejectedBySupervisorCount;
            approvedBySupervisor[j] = dayStats.ApprovedBySupervisorCount;
            rejectedByHeadquarters[j] = dayStats.RejectedByHeadquartersCount;
            approvedByHeadquarters[j] = dayStats.ApprovedByHeadquartersCount;
        }

        private static void CopyStatsFromPreviousDay(int j, int[] supervisorAssignedData, int[] interviewerAssignedData, int[] completedData,
            int[] rejectedBySupervisor, int[] approvedBySupervisor, int[] rejectedByHeadquarters, int[] approvedByHeadquarters)
        {
            supervisorAssignedData[j] = supervisorAssignedData[j - 1];
            interviewerAssignedData[j] = interviewerAssignedData[j - 1];
            completedData[j] = completedData[j - 1];
            rejectedBySupervisor[j] = rejectedBySupervisor[j - 1];
            approvedBySupervisor[j] = approvedBySupervisor[j - 1];
            rejectedByHeadquarters[j] = rejectedByHeadquarters[j - 1];
            approvedByHeadquarters[j] = approvedByHeadquarters[j - 1];
        }
    }
}