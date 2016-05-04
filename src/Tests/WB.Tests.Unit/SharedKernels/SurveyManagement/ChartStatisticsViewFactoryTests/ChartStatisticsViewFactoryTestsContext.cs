using System;
using System.Collections.Generic;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class ChartStatisticsViewFactoryTestsContext
    {
        protected static ChartStatisticsViewFactory CreateChartStatisticsViewFactory(StatisticsGroupedByDateAndTemplate statistics = null)
            => new ChartStatisticsViewFactory(
                Mock.Of<IOldschoolChartStatisticsDataProvider>(_
                    => _.GetStatisticsInOldFormat(It.IsAny<Guid>(), It.IsAny<long>()) == (statistics ?? new StatisticsGroupedByDateAndTemplate())));

        protected static QuestionnaireStatisticsForChart CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(int count)
        {
            return CreateQuestionnaireStatisticsForChart(count, count, count, count, count, count, count);
        }

        protected static QuestionnaireStatisticsForChart CreateQuestionnaireStatisticsForChart(
            int supervisorAssigned, int interviewerAssigned, int completed,
            int rejectedBySupervisor, int approvedBySupervisor, int rejectedByHeadquarters, int approvedByHeadquarters)
        {
            return new QuestionnaireStatisticsForChart
            {
                ApprovedByHeadquartersCount = approvedByHeadquarters,
                ApprovedBySupervisorCount = approvedBySupervisor,
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
