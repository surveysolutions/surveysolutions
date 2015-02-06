using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    [Subject(typeof(QuestionnaireSynchronizationDenormalizer))]
    internal class QuestionnaireSynchronizationDenormalizerTestsContext
    {
        protected static QuestionnaireSynchronizationDenormalizer CreateDenormalizer(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IJsonUtils jsonUtils = null,
            IReadSideRepositoryWriter<QuestionnaireSyncPackage> questionnairePackageStorageWriter = null,
            IQueryableReadSideRepositoryReader<QuestionnaireSyncPackage> questionnairePackageStorageReader = null)
        {
            var result = new QuestionnaireSynchronizationDenormalizer(
                questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                questionnairePackageStorageWriter ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireSyncPackage>>(),
                questionnairePackageStorageReader
                ?? Mock.Of<IQueryableReadSideRepositoryReader<QuestionnaireSyncPackage>>());

            return result;
        }
    }
}