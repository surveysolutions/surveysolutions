using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    [Ignore("temp parallelism test")]
    internal class when_enqueue_1000_files_and_then_get_length : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            //var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            //fileSystemAccessorMock.Setup(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
            //    .Callback<string, string>((filename, content) => filenames.Add(filename));

            fileSystemAccessor = new FileSystemIOAccessor();
            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessor);
        };

        Because of = () =>
        {
            for (int i = 1; i <= 1000; i++)
            {
                incomingSyncPackagesQueue.Enqueue(Guid.NewGuid(), $"{i}");
                Thread.Sleep(10);
            }
        };

        It should_store_1000_elements = () =>
        {
            List<string> fileNames = incomingSyncPackagesQueue.GetTopSyncItemsAsFileNames(1000).ToList();

            for (int i = 0; i < 1000; i++)
            {
                var filename = fileNames[i];
                var readAllText = fileSystemAccessor.ReadAllText(filename);
                readAllText.ShouldEqual((i+1).ToString());
            }
        };

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        //private static Mock<IFileSystemAccessor> fileSystemAccessorMock;

        private static List<string> result = new List<string>();
        //private static Guid interviewId = Guid.NewGuid();
        private static List<string> filenames = new List<string>();
        private static FileSystemIOAccessor fileSystemAccessor;
    }
}