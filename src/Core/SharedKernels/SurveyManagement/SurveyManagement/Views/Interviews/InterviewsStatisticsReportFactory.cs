using Main.Core.View;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class InterviewsStatisticsReportFactory : IViewFactory<InterviewsStatisticsReportInputModel, InterviewsStatisticsReportView>
    {
        public InterviewsStatisticsReportView Load(InterviewsStatisticsReportInputModel input)
        {
            int[] supervisorAssignedData = {30, 9, 5, 12, 14, 8, 7, 9, 6, 6, 3, 2, 0};
            int[] interviewerAssignedData = {0, 5, 5, 3, 6, 5, 3, 2, 6, 7, 4, 3, 0};
            int[] completedData = {0, 6, 5, 5, 2, 3, 4, 2, 1, 5, 7, 4, 0};
            int[] rejectedBySupervisor = {0, 6, 8, 3, 2, 3, 4, 2, 1, 4, 6, 5, 0};
            int[] approvedBySupervisor = {0, 6, 8, 3, 2, 3, 4, 3, 2, 5, 7, 6, 0};
            int[] rejectedByHeadquarters = {0, 6, 8, 3, 2, 3, 4, 3, 2, 5, 7, 6, 0};
            int[] approvedByHeadquarters = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 30};

            int[][] stats = {supervisorAssignedData, interviewerAssignedData, completedData, rejectedBySupervisor, approvedBySupervisor, rejectedByHeadquarters, approvedByHeadquarters};

            string[,] ticks = {{"1", "Aug 3"}, {"2", "Aug 4"}, {"3", "Aug 5"}, {"4", "Aug 6"}, {"5", "Aug 7"}, {"6", "Aug 8"}, {"7", "Aug 9"}, {"8", "Aug 10"}, {"9", "Aug 11"}, {"10", "Aug 12"}, {"11", "Aug 13"}, {"12", "Aug 14"}, {"13", "Aug 15"}};

            return new InterviewsStatisticsReportView() { Stats = stats, Ticks = ticks};
        }
    }
}