using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    [Subject(typeof(QuestionnaireSynchronizationDenormalizer))]
    internal class QuestionnaireSynchronizationDenormalizerTestsContext
    {
        protected const string CounterId = "QuestionnaireSyncPackageСounter";

        protected static QuestionnaireSynchronizationDenormalizer CreateDenormalizer(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IJsonUtils jsonUtils = null,
            IOrderableSyncPackageWriter<QuestionnaireSyncPackageMeta, QuestionnaireSyncPackageContent> questionnairePackageStorageWriter = null,
            IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStorage=null)
        {
            var result = new QuestionnaireSynchronizationDenormalizer(
                questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                questionnairePackageStorageWriter ?? Mock.Of<IOrderableSyncPackageWriter<QuestionnaireSyncPackageMeta, QuestionnaireSyncPackageContent>>());

            return result;
        }
    }
}