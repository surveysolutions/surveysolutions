using Moq;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests
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

        public static IInterviewDetailsDataProcessor InterviewDetailsDataProcessor(
            InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext = null,
            IFileSystemAccessor fileSystemAccessor = null, SyncSettings syncSettings = null,
            IReadSideRepositoryReader<InterviewData> interviewDataRepositoryReader = null)
        {
            return
                new InterviewDetailsDataProcessor(
                    interviewDetailsDataProcessorContext:
                        interviewDetailsDataProcessorContext ??
                        new InterviewDetailsDataProcessorContext(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>()),
                    logger: Mock.Of<ILogger>(),
                    interviewDetailsDataLoaderSettings: new Mock<InterviewDetailsDataLoaderSettings>(true, 1, 1).Object,
                    syncSettings:
                        syncSettings ??
                        new Mock<SyncSettings>(false, "AppData", "IncomingData", "IncomingDataWithErrors", "sync")
                            .Object,
                    interviewDetailsReader:
                        interviewDataRepositoryReader ?? Mock.Of<IReadSideRepositoryReader<InterviewData>>(),
                    fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());
        }
    }
}
