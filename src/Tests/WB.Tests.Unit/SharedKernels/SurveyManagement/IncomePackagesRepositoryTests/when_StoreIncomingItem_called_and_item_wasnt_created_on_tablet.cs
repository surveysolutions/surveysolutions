﻿using System;
using Machine.Specifications;
using Main.Core;
using Moq;

using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class when_StoreIncomingItem_called_and_item_wasnt_created_on_tablet : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            syncItem = new SyncItem()
            {
                Content = "some content",
                RootId = Guid.NewGuid(),
                MetaInfo = new ZipArchiveUtils(Mock.Of<IFileSystemAccessor>()).CompressString("some string")
            };

            jsonMock = new Mock<IJsonUtils>();
            jsonMock.Setup(x => x.Deserrialize<InterviewMetaInfo>(Moq.It.IsAny<string>()))
                .Returns(interviewMetaInfo);

            jsonMock.Setup(x => x.GetItemAsContent(syncItem)).Returns(contentOfSyncItem);

            commandServiceMock=new Mock<ICommandService>();

            fileSystemAccessorMock = CreateDefaultFileSystemAccessorMock();

            incomePackagesRepository = CreateIncomePackagesRepository(jsonMock.Object, fileSystemAccessorMock.Object, commandServiceMock.Object);
        };

        Because of = () =>
            incomePackagesRepository.StoreIncomingItem(syncItem);

        It should_write_text_file_to_sync_package_folder = () =>
          fileSystemAccessorMock.Verify(x => x.WriteAllText(GetPathToSynchItemInSyncPackageFolder(interviewMetaInfo.PublicKey), syncItem.Content), Times.Once);

        It should_call_ApplySynchronizationMetadata_command_with_metadata_arguments = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.Is<ApplySynchronizationMetadata>(
                            (passedCommand) =>
                                passedCommand.Id == interviewMetaInfo.PublicKey && passedCommand.UserId == interviewMetaInfo.ResponsibleId &&
                                    passedCommand.QuestionnaireId == interviewMetaInfo.TemplateId
                                    && passedCommand.InterviewStatus == InterviewStatus.Completed &&
                                    passedCommand.FeaturedQuestionsMeta == null && passedCommand.Comments == interviewMetaInfo.Comments
                                    && passedCommand.Valid == false && passedCommand.CreatedOnClient == false),
                        "capi-sync"), Times.Once);

        private static IncomePackagesRepository incomePackagesRepository;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<ICommandService> commandServiceMock;
        private static SyncItem syncItem;

        private static InterviewMetaInfo interviewMetaInfo = new InterviewMetaInfo()
        {
            CreatedOnClient = false,
            ResponsibleId = Guid.NewGuid(),
            Comments = "my comment",
            PublicKey = Guid.NewGuid(),
            Status = (int) InterviewStatus.Completed,
            TemplateId = Guid.NewGuid(),
            TemplateVersion = 2,
            Valid = false
        };
        private static Mock<IJsonUtils> jsonMock;
        private static string contentOfSyncItem = "content of sync item";
    }
}
