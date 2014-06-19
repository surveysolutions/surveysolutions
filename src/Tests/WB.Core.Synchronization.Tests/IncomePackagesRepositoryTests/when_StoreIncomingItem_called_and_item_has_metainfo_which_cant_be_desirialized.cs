using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Core.Synchronization.Tests.IncomePackagesRepositoryTests
{
    internal class when_StoreIncomingItem_called_and_item_has_metainfo_which_cant_be_desirialized : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            jsonMock = new Mock<IJsonUtils>();
            jsonMock.Setup(x => x.Deserrialize<InterviewMetaInfo>(Moq.It.IsAny<string>())).Throws(new Exception("random exception during depersonalization"));
            jsonMock.Setup(x => x.GetItemAsContent(syncItem)).Returns(contentOfSyncItem);

            fileSystemAccessorMock=CreateDefaultFileSystemAccessorMock();

            incomePackagesRepository = CreateIncomePackagesRepository(jsonMock.Object, fileSystemAccessorMock.Object);
        };

        Because of = () =>
            incomePackagesRepository.StoreIncomingItem(syncItem);

        It should_write_text_file_to_error_folder = () =>
          fileSystemAccessorMock.Verify(x => x.WriteAllText(GetPathToSynchItemInErrorFolder(syncItem.Id), contentOfSyncItem), Times.Once);

        private static IncomePackagesRepository incomePackagesRepository;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static SyncItem syncItem = new SyncItem() { Content = "some content", Id = Guid.NewGuid() };
        private static Mock<IJsonUtils> jsonMock;
        private static string contentOfSyncItem = "content of sync item";
    }
}
