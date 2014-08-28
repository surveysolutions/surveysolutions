using System;
using System.Globalization;
using System.Linq;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class InterviewsStatisticsReportFactory : IViewFactory<InterviewsStatisticsReportInputModel, InterviewsStatisticsReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> interviewSummaryReader;

        public InterviewsStatisticsReportFactory(
            IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public InterviewsStatisticsReportView Load(InterviewsStatisticsReportInputModel input)
        {
            int daysCount = 0;
            DateTime firstDate = DateTime.Now;

            var stat = this.interviewSummaryReader
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

            if (stat.Count != 0)
            {
                firstDate = stat.First().Date;
                var lastDate = input.CurrentDate;

                daysCount = Convert.ToInt32((lastDate - firstDate).TotalDays) + 1;
            }

            var filterRangeInDays = Convert.ToInt32((input.To - input.From).TotalDays) + 1;
            var resultArrayLength = daysCount >= filterRangeInDays ? filterRangeInDays : daysCount;

            var result = new InterviewsStatisticsReportView();

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
                
                var dayStats = stat.Find( _ => _.Date.ToShortDateString().Equals(internalDateKey));
                var rowNumber = (j + 1).ToString(CultureInfo.InvariantCulture);

                if (dayStats != null)
                {
                    ticks[j, 0] = rowNumber;
                    ticks[j, 1] = dayStats.Date.ToShortDateString();

                    supervisorAssignedData[j] = dayStats.SupervisorAssignedCount;
                    interviewerAssignedData[j] = dayStats.InterviewerAssignedCount;
                    completedData[j] = dayStats.CompletedCount;
                    rejectedBySupervisor[j] = dayStats.RejectedBySupervisorCount;
                    approvedBySupervisor[j] = dayStats.ApprovedBySupervisorCount;
                    rejectedByHeadquarters[j] = dayStats.RejectedByHeadquartersCount;
                    approvedByHeadquarters[j] = dayStats.ApprovedByHeadquartersCount;
                }
                else
                {
                    ticks[j, 0] = rowNumber;
                    ticks[j, 1] = internalDateKey;

                    if (j > 0)
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
            
            result.Stats = stats;
            result.Ticks = ticks;

            return result;
        }
    }
}