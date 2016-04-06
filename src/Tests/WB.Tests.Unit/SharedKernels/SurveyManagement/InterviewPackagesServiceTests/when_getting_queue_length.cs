using System.IO;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    internal class when_getting_queue_length : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            var serializer =
                Mock.Of<ISerializer>(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>()) == new SyncItem() &&
                                          x.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>()) == new InterviewMetaInfo { Status = 0});
            var archiver = Mock.Of<IArchiveUtils>(x => x.IsZipStream(Moq.It.IsAny<Stream>()) == true);

            fileSystemAccessor = new FileSystemIOAccessor();
            packagesStorage = new TestPlainStorage<InterviewPackage>();

            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessor,
                archiver: archiver, serializer: serializer, interviewPackageStorage: packagesStorage);
        };

        Because of = () =>
        {
            for (int i = 1; i <= 1000; i++)
            {
                incomingSyncPackagesQueue.StorePackage($"{i}");
                Thread.Sleep(10);
            }
        };

        It should_store_1000_elements = () => incomingSyncPackagesQueue.QueueLength.ShouldEqual(1000);

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static FileSystemIOAccessor fileSystemAccessor;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}