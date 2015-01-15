using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationTests.SchedulersTests.InterviewDetailsDataSchedulerTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationTests.SchedulersTests.InterviewDetailsDataProcessorTests
{
    internal class when_process_capi_package_and_throws_unhandled_exception : InterviewDetailsDataSchedulerTestsContext
    {
        Establish context = () =>
        {
            var interviewDetailsDataProcessorContextMock =
                new Mock<InterviewDetailsDataProcessorContext>(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>());
            interviewDetailsDataProcessorContextMock.Setup(_ => _.PushMessage(Moq.It.IsAny<string>()))
                .Callback((string message) => lastContextMessage = message);

            var syncSettingsMock = new Mock<SyncSettings>(false, appdataDirectoryPath, incomingPackageDirectoryName,
                incomingPackageWithErrorsDirectoryName,
                incomingPackageFileExtension);

            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            fileSystemAccessorMock.Setup(_ => _.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns(new[] {GetFullPathToPackage(packageFileName)});

            fileSystemAccessorMock.Setup(_ => _.GetFileName(GetFullPathToPackage(packageFileName)))
                .Returns(string.Format("{0}.{1}", packageFileName, incomingPackageFileExtension));

            fileSystemAccessorMock.Setup(_ => _.GetFileNameWithoutExtension(GetFullPathToPackage(packageFileName)))
                .Returns(packageFileName);

            var interviewDataRepositoryReaderMock = new Mock<IReadSideKeyValueStorage<InterviewData>>();
            interviewDataRepositoryReaderMock.Setup(_ => _.GetById(packageFileName))
                .Throws(new Exception(unhandledExceptionMessage));

            interviewDetailsDataProcessorContext = interviewDetailsDataProcessorContextMock.Object;

            interviewDetailsDataProcessor =
                Create.InterviewDetailsDataProcessor(
                    interviewDetailsDataProcessorContext: interviewDetailsDataProcessorContext,
                    fileSystemAccessor: fileSystemAccessorMock.Object, syncSettings: syncSettingsMock.Object,
                    interviewDataRepositoryReader: interviewDataRepositoryReaderMock.Object);
        };

        private static string GetFullPathToPackage(string packageName)
        {
            return string.Format(@"{0}\{1}\{2}.{3}", appdataDirectoryPath, incomingPackageDirectoryName, packageName,
                incomingPackageFileExtension);
        }

        Because of = () =>
            interviewDetailsDataProcessor.Process();

        It should_messages_of_interview_details_data_processor_context_not_be_empty = () =>
            lastContextMessage.ShouldNotBeNull();

        It should_last_message_of_interview_details_data_processor_context_contains___unhandledExceptionMessage = () =>
            lastContextMessage.ShouldContain(unhandledExceptionMessage);

        private static InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext;
        private static IInterviewDetailsDataProcessor interviewDetailsDataProcessor;

        private static string lastContextMessage;
        private static string packageFileName = "11111111-1111-1111-1111-111111111111";
        private static string unhandledExceptionMessage = "unhandled exception";

        private const string appdataDirectoryPath = "AppData";
        private const string incomingPackageDirectoryName = "IncomingData";
        private const string incomingPackageWithErrorsDirectoryName = "IncomingDataWithErrors";
        private const string incomingPackageFileExtension = "sync";
    }
}