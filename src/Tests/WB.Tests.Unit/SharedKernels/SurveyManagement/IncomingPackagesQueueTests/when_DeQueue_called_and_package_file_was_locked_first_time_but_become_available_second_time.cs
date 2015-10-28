﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_DeQueue_called_and_package_file_was_locked_first_time_but_become_available_second_time : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            var jsonUtilsMock = new Mock<ISerializer>();
            jsonUtilsMock.Setup(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>())).Returns(new SyncItem() { RootId = interviewId });
            jsonUtilsMock.Setup(x => x.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>())).Returns(new InterviewMetaInfo());
            jsonUtilsMock.Setup(x => x.Deserialize<AggregateRootEvent[]>(Moq.It.IsAny<string>())).Returns(new[] { new AggregateRootEvent() });

            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            var isFirstCall = true;
            fileSystemAccessorMock.Setup(x => x.ReadAllText(Moq.It.IsAny<string>()))
                .Returns("")
                .Callback(() =>
                {
                    if (isFirstCall)
                    {
                        isFirstCall = false;
                        throw new Win32Exception("emulate file lock");
                    }
                });

            firstFileInFolder = interviewId.FormatGuid();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(new[] { firstFileInFolder });
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object,
                serializer: jsonUtilsMock.Object);
        };

        Because of = () =>
               result = incomingSyncPackagesQueue.DeQueue();

        It should_return_first_file_in_folder = () =>
            result.PathToPackage.ShouldEqual(firstFileInFolder);

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static IncomingSyncPackage result;
        private static string firstFileInFolder;
    }
}
