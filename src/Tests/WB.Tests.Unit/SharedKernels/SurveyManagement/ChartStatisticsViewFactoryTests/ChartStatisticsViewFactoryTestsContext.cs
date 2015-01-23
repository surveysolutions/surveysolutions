using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class ChartStatisticsViewFactoryTestsContext
    {
        protected static ChartStatisticsViewFactory CreateChartStatisticsViewFactory(IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate> statisticsReader = null)
        {
            return new ChartStatisticsViewFactory(
                statisticsReader ?? Stub<IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate>>.WithNotEmptyValues);
        }

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
