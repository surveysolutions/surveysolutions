using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class when_DeQueue_called_and_package_cant_be_opened : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>())).Throws<NullReferenceException>();

            fileSystemAccessorMock=new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(new[] {interviewId.FormatGuid()});
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            incomingPackagesQueue = CreateIncomePackagesRepository(fileSystemAccessor: fileSystemAccessorMock.Object, jsonUtils: jsonUtilsMock.Object,
                interviewSummaryStorage:
                    Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(_ => _.GetById(interviewId.FormatGuid()) == new InterviewSummary()));
        };

        Because of = () =>
            incomingPackagesQueue.DeQueue();

        It should_delete_package = () =>
           fileSystemAccessorMock.Verify(x => x.DeleteFile(interviewId.FormatGuid()), Times.Once);

        It should_copy_package_to_error_folder = () =>
           fileSystemAccessorMock.Verify(x => x.CopyFileOrDirectory(interviewId.FormatGuid(), @"App_Data\IncomingDataWithErrors"), Times.Once);

        private static IncomingPackagesQueue incomingPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
