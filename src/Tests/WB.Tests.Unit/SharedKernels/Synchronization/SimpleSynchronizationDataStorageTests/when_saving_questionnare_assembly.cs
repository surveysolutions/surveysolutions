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
                Moq.It.Is<QuestionnaireSyncPackageMeta>(s => s.ItemType == SyncItemType.QuestionnaireAssembly && s.QuestionnaireId == qId && s.QuestionnaireVersion == version),
                Moq.It.IsAny<string>()), 
                Times.Once);

        static QuestionnaireSynchronizationDenormalizer denormalizer;
        static Guid qId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static long version = 3;
        static Mock<IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta>> questionnairePackageStorageWriter = new Mock<IReadSideRepositoryWriter<QuestionnaireSyncPackageMeta>>();
    }
}
