using System;
using System.Collections.Generic;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    public class ChartStatisticsViewFactoryTestsContext
    {
        protected static ChartStatisticsViewFactory CreateChartStatisticsViewFactory(StatisticsGroupedByDateAndTemplate statistics = null)
            => new ChartStatisticsViewFactory(
                Mock.Of<IOldschoolChartStatisticsDataProvider>(_
                    => _.GetStatisticsInOldFormat(It.IsAny<Guid>(), It.IsAny<long>()) == (statistics ?? new StatisticsGroupedByDateAndTemplate())));

        protected static QuestionnaireStatisticsForChart CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(int count)
        {
            return CreateQuestionnaireStatisticsForChart(count, count, count, count, count, count, count, count);
        }

        protected static QuestionnaireStatisticsForChart CreateQuestionnaireStatisticsForChart(
            int supervisorAssigned, int interviewerAssigned, int restarted, int completed,
            int rejectedBySupervisor, int approvedBySupervisor, int rejectedByHeadquarters, int approvedByHeadquarters)
        {
            return new QuestionnaireStatisticsForChart
            {
                ApprovedByHeadquartersCount = approvedByHeadquarters,
                ApprovedBySupervisorCount = approvedBySupervisor,
                RestartedCount = restarted,
                CompletedCount = completed,
                InterviewerAssignedCount = interviewerAssigned,
                RejectedByHeadquartersCount = rejectedByHeadquarters,
                RejectedBySupervisorCount = rejectedBySupervisor,
                SupervisorAssignedCount = supervisorAssigned,
            };
        }

        protected static StatisticsGroupedByDateAndTemplate CreateStatisticsGroupedByDateAndTemplate(
            Dictionary<DateTime, QuestionnaireStatisticsForChart> statistics)
        {
            return new StatisticsGroupedByDateAndTemplate { StatisticsByDate = statistics };
        }
    }
}
