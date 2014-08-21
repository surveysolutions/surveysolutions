using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.InterviewsStatisticsReportFactoryTests
{
    internal class InterviewsStatisticsReportFactoryTestsContext
    {
        protected static InterviewsStatisticsReportFactory CreateInterviewsStatisticsReportFactory(
            IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate> queryableReadSideRepositoryReader = null)
        {
            return
                new InterviewsStatisticsReportFactory(queryableReadSideRepositoryReader ??
                    Mock.Of<IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate>>());
        }
    }
}