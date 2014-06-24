using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Core.Synchronization.Tests.IncomePackagesRepositoryTests
{
    internal class when_StoreIncomingItem_called_and_item_was_created_on_tablet : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            syncItem = new SyncItem()
            {
                Content = "some content",
                Id = Guid.NewGuid(),
                MetaInfo = PackageHelper.CompressString("some string")
            };

            jsonMock = new Mock<IJsonUtils>();
            jsonMock.Setup(x => x.Deserrialize<InterviewMetaInfo>(Moq.It.IsAny<string>()))
                .Returns(interviewMetaInfo);

            jsonMock.Setup(x => x.GetItemAsContent(syncItem)).Returns(contentOfSyncItem);

            commandServiceMock = new Mock<ICommandService>();

            fileSystemAccessorMock = CreateDefaultFileSystemAccessorMock();

            incomePackagesRepository = CreateIncomePackagesRepository(jsonMock.Object, fileSystemAccessorMock.Object, commandServiceMock.Object);
        };

        Because of = () =>
            incomePackagesRepository.StoreIncomingItem(syncItem);

        It should_write_text_file_to_sync_package_folder = () =>
          fileSystemAccessorMock.Verify(x => x.WriteAllText(GetPathToSynchItemInSyncPackageFolder(interviewMetaInfo.PublicKey), syncItem.Content), Times.Once);

        It should_call_CreateInterviewCreatedOnClientCommand_command_with_metadata_arguments = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.Is<CreateInterviewCreatedOnClientCommand>(passedCommand => passedCommand.Id == interviewMetaInfo.PublicKey && passedCommand.UserId == interviewMetaInfo.ResponsibleId &&
                passedCommand.QuestionnaireId == interviewMetaInfo.TemplateId
                && passedCommand.InterviewStatus == InterviewStatus.Completed && passedCommand.IsValid == interviewMetaInfo.Valid
                && passedCommand.FeaturedQuestionsMeta.Length == 2
                                   /* && passedCommand.Valid == true*/),
                        null), Times.Once);

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
            Status = (int) InterviewStatus.Completed,
            TemplateId = Guid.NewGuid(),
            TemplateVersion = 2,
            FeaturedQuestionsMeta = new[]
            {
                new FeaturedQuestionMeta(Guid.NewGuid(), "1", "a"),
                new FeaturedQuestionMeta(Guid.NewGuid(), "2", "b")
            },
            Valid = true
        };
        private static Mock<IJsonUtils> jsonMock;
        private static string contentOfSyncItem = "content of sync item";
    }
}
