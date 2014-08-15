using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.IncomePackagesRepositoryTests
{
    internal class when_StoreIncomingItem_called_and_item_was_created_on_tablet_and_store_command_rise_exception : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            jsonMock = new Mock<IJsonUtils>();
            jsonMock.Setup(x => x.Deserrialize<InterviewMetaInfo>(Moq.It.IsAny<string>()))
                .Returns(interviewMetaInfo);

            jsonMock.Setup(x => x.GetItemAsContent(syncItem)).Returns(contentOfSyncItem);

            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(x => x.Execute(Moq.It.IsAny<CreateInterviewCreatedOnClientCommand>(), null))
                .Throws<NullReferenceException>();

            fileSystemAccessorMock = CreateDefaultFileSystemAccessorMock();

            incomePackagesRepository = CreateIncomePackagesRepository(jsonMock.Object, fileSystemAccessorMock.Object, commandServiceMock.Object);
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
        private static Mock<ICommandService> commandServiceMock;

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
    }
}
