using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Pull;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.SyncCacher;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.PullDataProcessorTests
{
    internal class when_sync_package_contains_information_about_new_interview : PullDataProcessorTestContext
    {
        Establish context = () =>
        {
            questionnaireMetadata = new InterviewMetaInfo()
            {
                Comments = "my comment",
                CreatedOnClient = false,
                PublicKey = Guid.NewGuid(),
                ResponsibleId = Guid.NewGuid(),
                Status = (int) InterviewStatus.InterviewerAssigned,
                TemplateId = Guid.NewGuid(),
                TemplateVersion = 1,
                Title = "my title",
                Valid = true,
                FeaturedQuestionsMeta = new FeaturedQuestionMeta[] { new FeaturedQuestionMeta(Guid.NewGuid(), "t1", "v1"), new FeaturedQuestionMeta(Guid.NewGuid(), "t2", "v2") }
            };

            syncItem = new SyncItem() { ItemType = SyncItemType.Questionnare, IsCompressed = true, Content = "some content", MetaInfo = "some metadata", Id = Guid.NewGuid() };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<InterviewMetaInfo>(syncItem.MetaInfo)).Returns(questionnaireMetadata);

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            syncCacher = new Mock<ISyncCacher>();
            pullDataProcessor = CreatePullDataProcessor(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, null,
                plainQuestionnaireRepositoryMock.Object, syncCacher.Object);
        };

        Because of = () => pullDataProcessor.Process(syncItem);

        It should_call_ApplySynchronizationMetadata_once =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<ApplySynchronizationMetadata>(
                                param =>
                                    param.QuestionnaireId == questionnaireMetadata.TemplateId && param.Id == questionnaireMetadata.PublicKey &&
                                    param.UserId == questionnaireMetadata.ResponsibleId && (int)param.InterviewStatus == questionnaireMetadata.Status &&
                                    param.Comments=="" && param.Valid==true && param.CreatedOnClient==false && param.FeaturedQuestionsMeta.Length==2), null),
                    Times.Once);

        It should_store_interview_content_in_sync_cacher_once =
            () =>
                syncCacher.Verify(
                    x => x.SaveItem(questionnaireMetadata.PublicKey,syncItem.Content),
                    Times.Once);

        It should_create_public_record_in_change_log_for_sync_item_once =
        () =>
            changeLogManipulator.Verify(
                x =>
                    x.CreatePublicRecord(syncItem.Id),
                Times.Once);

        private static PullDataProcessor pullDataProcessor;
        private static SyncItem syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ISyncCacher> syncCacher;
        private static InterviewMetaInfo questionnaireMetadata;
    }
}
