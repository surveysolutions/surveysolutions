using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.OrderableSyncPackageWriterTests
{
    internal class OrderableSyncPackageWriterTestContext<TMeta, TContent>
        where TMeta : class, IReadSideRepositoryEntity, IOrderableSyncPackage
        where TContent : class, IReadSideRepositoryEntity, ISyncPackage
    {
        protected static OrderableSyncPackageWriter<TMeta, TContent> CreateOrderableSyncPackageWriter(
            IReadSideRepositoryWriter<TMeta> packageMetaWriter = null, 
            IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage = null, 
            IReadSideKeyValueStorage<TContent> packageContentWriter = null)
        {
            return new OrderableSyncPackageWriter<TMeta, TContent>(
                packageMetaWriter ?? Mock.Of < IReadSideRepositoryWriter<TMeta>>(),
                counterStorage ?? Mock.Of<IReadSideKeyValueStorage<SynchronizationDeltasCounter>>(),
                packageContentWriter ?? Mock.Of<IReadSideKeyValueStorage<TContent>>());
        }

        protected static InterviewSyncPackageMeta CreateInterviewSyncPackageMeta() 
        {
            return new InterviewSyncPackageMeta();
        }

        protected static InterviewSyncPackageContent CreateInterviewSyncPackageContent()
        {
            return new InterviewSyncPackageContent();
        }

        protected static QuestionnaireSyncPackageMeta CreateQuestionnaireSyncPackageMeta()
        {
            return new QuestionnaireSyncPackageMeta();
        }

        protected static QuestionnaireSyncPackageContent CreateQuestionnaireSyncPackageContent()
        {
            return new QuestionnaireSyncPackageContent();
        }

        protected static UserSyncPackageMeta CreateUserSyncPackageMeta()
        {
            return new UserSyncPackageMeta();
        }

        protected static UserSyncPackageContent CreateUserSyncPackageContent()
        {
            return new UserSyncPackageContent();
        }

        protected static SynchronizationDeltasCounter CreateSynchronizationDeltasCounter(int sortIndex)
        {
            return new SynchronizationDeltasCounter(sortIndex);
        }
    }
}
