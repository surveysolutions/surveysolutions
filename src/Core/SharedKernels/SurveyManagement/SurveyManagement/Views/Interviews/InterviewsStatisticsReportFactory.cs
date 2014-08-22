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
            var stat = this.interviewSummaryReader.Query(
                _ => _.Where(s => (s.QuestionnaireId == input.QuestionnaireId && s.QuestionnaireVersion == input.QuestionnaireVersion))
                    .OrderBy(o => o.Date)
                    .ToList()
            );

            var result = new InterviewsStatisticsReportView();

            var firstDate = stat.First().Date;
            var lastDate = input.CurrentDate;

            var daysCount = Convert.ToInt32((lastDate - firstDate).TotalDays) + 1;

            result.Ticks = new string[daysCount, 2];

            var supervisorAssignedData = new int[daysCount];
            var interviewerAssignedData = new int[daysCount];
            var completedData = new int[daysCount];
            var rejectedBySupervisor = new int[daysCount];
            var approvedBySupervisor = new int[daysCount];
            var rejectedByHeadquarters = new int[daysCount];
            var approvedByHeadquarters = new int[daysCount];

            int[][] stats = {supervisorAssignedData, interviewerAssignedData, completedData, rejectedBySupervisor, approvedBySupervisor, rejectedByHeadquarters, approvedByHeadquarters};
            
            for (var i = 0; i < daysCount; i++)
            {
                var internalDate = firstDate.AddDays(i).ToShortDateString();
                var dayStats = stat.Find( _ => _.Date.ToShortDateString().Equals(internalDate));
                var rowNumber = (i + 1).ToString(CultureInfo.InvariantCulture);

                if (dayStats != null)
                {
                    result.Ticks[i, 0] = rowNumber;
                    result.Ticks[i, 1] = dayStats.Date.ToShortDateString();

                    supervisorAssignedData[i] = dayStats.SupervisorAssignedCount;
                    interviewerAssignedData[i] = dayStats.InterviewerAssignedCount;
                    completedData[i] = dayStats.CompletedCount;
                    rejectedBySupervisor[i] = dayStats.RejectedBySupervisorCount;
                    approvedBySupervisor[i] = dayStats.ApprovedBySupervisorCount;
                    rejectedByHeadquarters[i] = dayStats.RejectedByHeadquartersCount;
                    approvedByHeadquarters[i] = dayStats.ApprovedByHeadquartersCount;
                }
                else
                {
                    result.Ticks[i, 0] = rowNumber;
                    result.Ticks[i, 1] = internalDate;

                    supervisorAssignedData[i] = supervisorAssignedData[i - 1];
                    interviewerAssignedData[i] = interviewerAssignedData[i - 1];
                    completedData[i] = completedData[i - 1];
                    rejectedBySupervisor[i] = rejectedBySupervisor[i - 1];
                    approvedBySupervisor[i] = approvedBySupervisor[i - 1];
                    rejectedByHeadquarters[i] = rejectedByHeadquarters[i - 1];
                    approvedByHeadquarters[i] = approvedByHeadquarters[i - 1];
                }
            }

            result.Stats = stats;

            return result;
        }
    }
}