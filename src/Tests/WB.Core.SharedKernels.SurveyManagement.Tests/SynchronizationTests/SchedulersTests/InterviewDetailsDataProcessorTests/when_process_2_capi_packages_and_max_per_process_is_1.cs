using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests.InterviewDetailsDataSchedulerTests;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.SynchronizationTests.SchedulersTests.InterviewDetailsDataProcessorTests
{
    internal class when_process_2_capi_packages_and_max_per_process_is_1 : InterviewDetailsDataSchedulerTestsContext
    {
        Establish context = () =>
        {
            var interviewDetailsDataProcessorContextMock =
                new Mock<InterviewDetailsDataProcessorContext>(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>());
            
            var syncSettingsMock = new Mock<SyncSettings>(false, appdataDirectoryPath, incomingPackageDirectoryName,
                incomingPackageWithErrorsDirectoryName,
                incomingPackageFileExtension);

            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            fileSystemAccessorMock.Setup(_ => _.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns(packageFileNames.Select(GetFullPathToPackage).ToArray());

            foreach (var packageFileName in packageFileNames)
            {
                fileSystemAccessorMock.Setup(_ => _.GetFileName(GetFullPathToPackage(packageFileName)))
                   .Returns(GetFileName(packageFileName));

                fileSystemAccessorMock.Setup(_ => _.GetFileNameWithoutExtension(GetFullPathToPackage(packageFileName)))
                    .Returns(packageFileName);   
            }
            
            interviewDetailsDataProcessor =
                Create.InterviewDetailsDataProcessor(
                    interviewDetailsDataProcessorContext: interviewDetailsDataProcessorContextMock.Object,
                    fileSystemAccessor: fileSystemAccessorMock.Object, syncSettings: syncSettingsMock.Object,
                    interviewDataRepositoryReader: interviewDataRepositoryReaderMock.Object);
        };

        private static string GetFileName(string packageFileName)
        {
            return string.Format("{0}.{1}", packageFileName, incomingPackageFileExtension);
        }

        private static string GetFullPathToPackage(string packageName)
        {
            return string.Format(@"{0}\{1}\{2}.{3}", appdataDirectoryPath, incomingPackageDirectoryName, packageName,
                incomingPackageFileExtension);
        }

        Because of = () =>
            interviewDetailsDataProcessor.Process();

        It should_call_GetById_method_of_interview_data_repository_reader_only_once = () =>
            interviewDataRepositoryReaderMock.Verify(_ => _.GetById(Moq.It.IsAny<string>()), Times.Once);

        private static IInterviewDetailsDataProcessor interviewDetailsDataProcessor;

        private static Mock<IReadSideRepositoryReader<InterviewData>> interviewDataRepositoryReaderMock =
            new Mock<IReadSideRepositoryReader<InterviewData>>();

        private static List<string> packageFileNames = new List<string>(){"11111111-1111-1111-1111-111111111111", 
                                                                          "22222222-2222-2222-2222-222222222222"};

        private const string appdataDirectoryPath = "AppData";
        private const string incomingPackageDirectoryName = "IncomingData";
        private const string incomingPackageWithErrorsDirectoryName = "IncomingDataWithErrors";
        private const string incomingPackageFileExtension = "sync";
    }
}