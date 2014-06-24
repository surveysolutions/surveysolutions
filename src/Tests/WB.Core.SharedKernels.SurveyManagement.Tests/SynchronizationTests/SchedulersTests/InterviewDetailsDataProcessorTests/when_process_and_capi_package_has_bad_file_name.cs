using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests.InterviewDetailsDataSchedulerTests;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests.InterviewDetailsDataProcessorTests
{
    internal class when_process_and_capi_package_has_bad_file_name : InterviewDetailsDataSchedulerTestsContext
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
                .Returns(new[] {GetFullPathToPackage(badPackageFileName)});

            fileSystemAccessorMock.Setup(_ => _.GetFileName(GetFullPathToPackage(badPackageFileName)))
                .Returns(string.Format("{0}.{1}", badPackageFileName, incomingPackageFileExtension));

            fileSystemAccessorMock.Setup(_ => _.GetFileNameWithoutExtension(GetFullPathToPackage(badPackageFileName)))
                .Returns(badPackageFileName);

            interviewDetailsDataProcessorContext = interviewDetailsDataProcessorContextMock.Object;

            interviewDetailsDataProcessor =
                Create.InterviewDetailsDataProcessor(
                    interviewDetailsDataProcessorContext: interviewDetailsDataProcessorContext,
                    fileSystemAccessor: fileSystemAccessorMock.Object, syncSettings: syncSettingsMock.Object);
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

        It should_last_message_of_interview_details_data_processor_context_contains___bad_package_name = () =>
            lastContextMessage.ShouldContain("bad package name");

        private static InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext;
        private static IInterviewDetailsDataProcessor interviewDetailsDataProcessor;
        private static string lastContextMessage;
        private static string badPackageFileName = "bad-package-file-name";

        private const string appdataDirectoryPath = "AppData";
        private const string incomingPackageDirectoryName = "IncomingData";
        private const string incomingPackageWithErrorsDirectoryName = "IncomingDataWithErrors";
        private const string incomingPackageFileExtension = "sync";
    }
}