using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_new_broken_interview : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireMetadata = new InterviewMetaInfo()
            {
                Comments = "my comment",
                CreatedOnClient = false,
                PublicKey = Guid.NewGuid(),
                ResponsibleId = Guid.NewGuid(),
                Status = (int)InterviewStatus.InterviewerAssigned,
                TemplateId = Guid.NewGuid(),
                TemplateVersion = 6,
                Title = "my title",
                Valid = true,
                FeaturedQuestionsMeta = new FeaturedQuestionMeta[] { new FeaturedQuestionMeta(Guid.NewGuid(), "t1", "v1"), new FeaturedQuestionMeta(Guid.NewGuid(), "t2", "v2") }
            };

            syncItem = new InterviewSyncPackageDto
                       {
                           Content = "some content",
                           MetaInfo = "some metadata"
                       };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<InterviewMetaInfo>(syncItem.MetaInfo)).Returns(questionnaireMetadata);

            commandService = new Mock<ICommandService>();
            commandService.Setup(x => x.Execute(Moq.It.IsAny<ApplySynchronizationMetadata>(), null, false)).Throws<NullReferenceException>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            syncCacher = new Mock<ICapiSynchronizationCacheService>();
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, null,
                plainQuestionnaireRepositoryMock.Object, syncCacher.Object);
        };

        Because of = () => exception = Catch.Exception(() => capiDataSynchronizationService.ProcessDownloadedPackage(syncItem, SyncItemType.Interview, questionnaireMetadata.ResponsibleId));

        It should_call_ApplySynchronizationMetadata_once =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<ApplySynchronizationMetadata>(
                                param =>
                                    param.QuestionnaireId == questionnaireMetadata.TemplateId && param.QuestionnaireVersion == questionnaireMetadata.TemplateVersion && param.Id == questionnaireMetadata.PublicKey &&
                                    param.UserId == questionnaireMetadata.ResponsibleId && (int)param.InterviewStatus == questionnaireMetadata.Status &&
                                    param.Comments == "my comment" && param.Valid == true && param.CreatedOnClient == false && param.FeaturedQuestionsMeta.Length == 2), null, false),
                    Times.Once);

        It should_not_store_questionnaire_in_pline_storage =
            () =>
                syncCacher.Verify(
                    x => x.SaveItem(questionnaireMetadata.PublicKey, syncItem.Content),
                    Times.Never);

        It should_throw_NullReferenceException = () =>
            exception.ShouldBeOfType<NullReferenceException>();

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static InterviewSyncPackageDto syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ICapiSynchronizationCacheService> syncCacher;
        private static InterviewMetaInfo questionnaireMetadata;
        private static Exception exception;
    }
}
