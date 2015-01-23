using Main.DenormalizerStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using it = Moq.It;

namespace WB.Tests.Integration.InterviewStatistics
{
    internal class InterviewChartDenormalizerTestContext
    {
        internal static InterviewsChartDenormalizer CreateInterviewsChartDenormalizer(
            IReadSideKeyValueStorage<InterviewDetailsForChart> interviewDetailsStorage = null,
            IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate> statisticsStorag = null)
        {
            return new InterviewsChartDenormalizer(
                interviewDetailsStorage ?? new InMemoryReadSideRepositoryAccessor<InterviewDetailsForChart>(),
                statisticsStorag ?? new InMemoryReadSideRepositoryAccessor<StatisticsGroupedByDateAndTemplate>());
        }
    }
}
