using Moq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationTests.SchedulersTests
{
    internal static class Create
    {
        public static IInterviewDetailsDataLoader InterviewDetailsDataLoader(IInterviewDetailsDataProcessor interviewDetailsDataProcessor = null,
            InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext = null)
        {
            return
                new InterviewDetailsDataLoader(
                    interviewDetailsDataProcessor:
                        interviewDetailsDataProcessor ?? Mock.Of<IInterviewDetailsDataProcessor>(),
                    interviewDetailsDataProcessorContext:
                        interviewDetailsDataProcessorContext ??
                        new InterviewDetailsDataProcessorContext(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>()));
        }
    }
}
