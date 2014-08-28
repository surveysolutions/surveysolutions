using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.InterviewsStatisticsReportFactoryTests
{
    internal class InterviewsStatisticsReportFactoryTestsContext
    {
        protected static InterviewsStatisticsReportFactory CreateInterviewsStatisticsReportFactory(IQueryable<StatisticsLineGroupedByDateAndTemplate> data)
        {
            var stats = Mock.Of<IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate>>();

            Mock.Get(stats)
                .Setup(
                    _ =>
                        _.Query(
                            Moq.It
                                .IsAny
                                <Func<IQueryable<StatisticsLineGroupedByDateAndTemplate>, List<StatisticsLineGroupedByDateAndTemplate>>>()))
                .Returns<Func<IQueryable<StatisticsLineGroupedByDateAndTemplate>, List<StatisticsLineGroupedByDateAndTemplate>>>(
                    query => query.Invoke(data)
                );

            return new InterviewsStatisticsReportFactory(stats);
        }
    }
}