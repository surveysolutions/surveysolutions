using System;
using Machine.Specifications;
using Moq;

using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class when_PushSyncItem_called_and_item_content_is_not_empty : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = CreateDefaultFileSystemAccessorMock();

            incomingPackagesQueue = CreateIncomePackagesRepository(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            incomingPackagesQueue.PushSyncItem(contentOfSyncItem);

        It should_write_text_file_to_error_folder = () =>
          fileSystemAccessorMock.Verify(x => x.WriteAllText(Moq.It.IsAny<string>(), contentOfSyncItem), Times.Once);

        private static IncomingPackagesQueue incomingPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static string contentOfSyncItem = "content of sync item";
    }
}
