using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class when_StoreIncomingItem_called_but_one_package_for_the_interview_is_present : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            syncItem = new SyncItem()
            {
                Content = "some content",
                RootId = interviewMetaInfo.PublicKey,
                MetaInfo = new ZipArchiveUtils(Mock.Of<IFileSystemAccessor>()).CompressString("some string")
            };

            jsonMock = new Mock<IJsonUtils>();
            jsonMock.Setup(x => x.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>()))
                .Returns(interviewMetaInfo);

            jsonMock.Setup(x => x.Serialize(syncItem)).Returns(contentOfSyncItem);

            commandServiceMock = new Mock<ICommandService>();

            fileSystemAccessorMock = CreateDefaultFileSystemAccessorMock();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(string.Format(@"App_Data\IncomingData\{0}.sync", syncItem.RootId))).Returns(true);
            incomePackagesRepository = CreateIncomePackagesRepository(jsonMock.Object, fileSystemAccessorMock.Object, commandServiceMock.Object);
        };

        Because of = () =>
            incomePackagesRepository.StoreIncomingItem(syncItem);

        It should_write_text_file_to_sync_package_folder = () =>
          fileSystemAccessorMock.Verify(x => x.WriteAllText(GetPathToSynchItemInErrorFolder(interviewMetaInfo.PublicKey), contentOfSyncItem), Times.Once);

        It should_never_call_CreateInterviewCreatedOnClientCommand_command_with_metadata_arguments = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.IsAny<CreateInterviewCreatedOnClientCommand>(),
                        "capi-sync"), Times.Never);

        private static IncomePackagesRepository incomePackagesRepository;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<ICommandService> commandServiceMock;
        private static SyncItem syncItem;

        private static InterviewMetaInfo interviewMetaInfo = new InterviewMetaInfo()
        {
            CreatedOnClient = true,
            ResponsibleId = Guid.NewGuid(),
            Comments = "my comment",
            PublicKey = Guid.NewGuid(),
            Status = (int)InterviewStatus.Completed,
            TemplateId = Guid.NewGuid(),
            TemplateVersion = 2,
            Valid = true
        };
        private static Mock<IJsonUtils> jsonMock;
        private static string contentOfSyncItem = "content of sync item";
    }
}
