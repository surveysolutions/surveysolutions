using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_saving_questionnare_assembly : QuestionnaireSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            denormalizer = CreateDenormalizer(questionnairePackageStorageWriter: questionnairePackageStorageWriter.Object);
        };

        private Because of = () => denormalizer.Handle(Create.QuestionnaireAssemblyImported(qId, version));

        It should_store_chunck = () =>
            questionnairePackageStorageWriter.Verify(x => x.Store(
                Moq.It.IsAny<QuestionnaireSyncPackageContent>(),
                Moq.It.Is<QuestionnaireSyncPackageMeta>(s => s.ItemType == SyncItemType.QuestionnaireAssembly && s.QuestionnaireId == qId && s.QuestionnaireVersion == version),
                Moq.It.IsAny<string>(),
                CounterId), 
                Times.Once);

        private static QuestionnaireSynchronizationDenormalizer denormalizer;
        private static Guid qId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static long version = 3;
        private static Mock<IOrderableSyncPackageWriter<QuestionnaireSyncPackageMeta, QuestionnaireSyncPackageContent>> questionnairePackageStorageWriter = new Mock<IOrderableSyncPackageWriter<QuestionnaireSyncPackageMeta, QuestionnaireSyncPackageContent>>();
    }
}
