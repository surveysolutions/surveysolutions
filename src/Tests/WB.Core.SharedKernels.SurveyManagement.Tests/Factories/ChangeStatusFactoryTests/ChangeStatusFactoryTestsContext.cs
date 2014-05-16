using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.ChangeStatusFactoryTests
{
    [Subject(typeof(ChangeStatusFactory))]
    internal class ChangeStatusFactoryTestsContext
    {
        public static ChangeStatusFactory CreateFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviews = null)
        {
            return new ChangeStatusFactory(interviews ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>());
        }
    }
}
