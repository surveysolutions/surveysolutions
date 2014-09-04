using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class ChartStatisticsViewFactoryTestsContext
    {
        protected static ChartStatisticsViewFactory CreateChartStatisticsViewFactory(IQueryable<StatisticsLineGroupedByDateAndTemplate> data)
        {
            var stats = Mock.Of<IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate>>();

            Mock.Get(stats)
                .Setup( _ => 
                    _.Query(Moq.It.IsAny<Func<IQueryable<StatisticsLineGroupedByDateAndTemplate>, List<StatisticsLineGroupedByDateAndTemplate>>>())
                )
                .Returns<Func<IQueryable<StatisticsLineGroupedByDateAndTemplate>, List<StatisticsLineGroupedByDateAndTemplate>>>(
                    query => query.Invoke(data)
                );

            return new ChartStatisticsViewFactory(stats);
        }
    }
}