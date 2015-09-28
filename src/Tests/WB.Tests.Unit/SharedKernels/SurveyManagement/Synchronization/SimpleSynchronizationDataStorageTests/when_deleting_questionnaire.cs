using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_deleting_questionnaire : QuestionnaireSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer(questionnairePackageStorageWriter: questionnairePackageStorageWriter.Object);
        };

        private Because of = () => denormalizer.Handle(Create.QuestionaireDeleted(questionnaireId, version));

        It should_store_delete_package_meta_information = () =>
            questionnairePackageStorageWriter.Verify(
                x => x.Store(
                    Moq.It.Is<QuestionnaireSyncPackageMeta>(s => s.ItemType == SyncItemType.DeleteQuestionnaire),
                    Moq.It.IsAny<string>()),
                Times.Once);
    
        private static QuestionnaireSynchronizationDenormalizer denormalizer;
        private static Guid questionnaireId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static long version = 4;
        private static Mock<IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta>> questionnairePackageStorageWriter = new Mock<IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta>>();
    }
}
