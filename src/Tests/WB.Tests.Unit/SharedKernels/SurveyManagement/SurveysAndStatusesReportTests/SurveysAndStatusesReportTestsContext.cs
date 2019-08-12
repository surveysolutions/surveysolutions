using Moq;
using NHibernate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    [NUnit.Framework.TestOf(typeof(SurveysAndStatusesReport))]
    internal class SurveysAndStatusesReportTestsContext
    {
        protected static SurveysAndStatusesReport CreateSurveysAndStatusesReport(INativeReadSideStorage<InterviewSummary> summariesRepository = null)
        {
            var mock = new Mock<IUnitOfWork>();
            mock.DefaultValueProvider = DefaultValueProvider.Mock;
            return new SurveysAndStatusesReport(summariesRepository ?? Stub.ReadSideRepository<InterviewSummary>(),
                mock.Object);
        }
    }
}
