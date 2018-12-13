using System;
using System.Collections.Generic;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    public class ChartStatisticsViewFactoryTestsContext
    {
        protected static ChartStatisticsViewFactory CreateChartStatisticsViewFactory(StatisticsGroupedByDateAndTemplate statistics = null)
            => new ChartStatisticsViewFactory(
                Mock.Of<IOldschoolChartStatisticsDataProvider>(_
                    => _.GetStatisticsInOldFormat(It.IsAny<QuestionnaireIdentity>()) == (statistics ?? new StatisticsGroupedByDateAndTemplate())));

        protected static QuestionnaireStatisticsForChart CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(int count)
        {
            return CreateQuestionnaireStatisticsForChart(count, count, count, count, count, count);
        }

        protected static QuestionnaireStatisticsForChart CreateQuestionnaireStatisticsForChart(
            int restarted, int completed, int rejectedBySupervisor, int approvedBySupervisor, 
            int rejectedByHeadquarters, int approvedByHeadquarters)
        {
            return new QuestionnaireStatisticsForChart
            {
                ApprovedByHeadquartersCount = approvedByHeadquarters,
                ApprovedBySupervisorCount = approvedBySupervisor,
                CompletedCount = completed,
                RejectedByHeadquartersCount = rejectedByHeadquarters,
                RejectedBySupervisorCount = rejectedBySupervisor,
            };
        }

        protected static StatisticsGroupedByDateAndTemplate CreateStatisticsGroupedByDateAndTemplate(
            Dictionary<DateTime, QuestionnaireStatisticsForChart> statistics)
        {
            return new StatisticsGroupedByDateAndTemplate { StatisticsByDate = statistics };
        }
    }
}
