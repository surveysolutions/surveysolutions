using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.Core.Synchronization
{
    [Subject(typeof (SyncManager))]
    internal class SyncManagerTestContext
    {
        protected static SyncManager CreateSyncManager(
            IReadSideRepositoryReader<TabletDocument> devices = null,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IReadSideRepositoryIndexAccessor indexAccessor = null,
            IQueryableReadSideRepositoryReader<UserSyncPackage> userPackageStorage = null,
            IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore = null,
            IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore = null,
            ISyncLogger syncLogger = null)
        {
            return new SyncManager(
                devices ?? Mock.Of<IReadSideRepositoryReader<TabletDocument>>(),
                incomingSyncPackagesQueue ?? Mock.Of<IIncomingSyncPackagesQueue>(),
                commandService ?? Mock.Of<ICommandService>(),
                indexAccessor ?? Mock.Of<IReadSideRepositoryIndexAccessor>(),
                userPackageStorage ?? Mock.Of<IQueryableReadSideRepositoryReader<UserSyncPackage>>(),
                interviewPackageContentStore ?? Mock.Of<IReadSideKeyValueStorage<InterviewSyncPackageContent>>(),
                questionnaireSyncPackageContentStore ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireSyncPackageContent>>(),
                syncLogger ?? Mock.Of<ISyncLogger>());
        }

        protected static ClientIdentifier CreateClientIdentifier(Guid userId, string androidId, string appVersion)
        {
            return new ClientIdentifier
            {
                ClientInstanceKey = Guid.NewGuid(),
                AndroidId = androidId,
                AppVersion = appVersion,
                UserId = userId
            };
        }

        protected static TabletDocument CreateTabletDocument(Guid deviceId, string androidId)
        {
            return new TabletDocument
                   {
                       DeviceId = deviceId,
                       AndroidId = androidId
                   };
        }

        protected static QuestionnaireSyncPackageMetaInformation CreateQuestionnaireSyncPackageMetaInformation(Guid questionnaireId, long questionnaireVersion, long sortIndex, DateTime? timestamp = null, string itemType = null, int contentSize = 20, int metaInfoSize = 10)
        {
            return new QuestionnaireSyncPackageMetaInformation(
                questionnaireId,
                questionnaireVersion,
                sortIndex,
                timestamp ?? DateTime.Now,
                itemType ?? SyncItemType.Questionnaire,
                contentSize,
                metaInfoSize);
        }

        protected static UserSyncPackage CreateUserSyncPackage(Guid userId, int sortIndex)
        {
            return new UserSyncPackage(userId, "content", DateTime.Now, sortIndex);
        }

        protected static InterviewSyncPackageMetaInformation CreateInterviewSyncPackageMetaInformation(Guid interviewId, int sortIndex, string itemType, Guid userId)
        {
            return new InterviewSyncPackageMetaInformation(interviewId, Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 1, DateTime.Now, userId, sortIndex, itemType, 10, 5);
        }

        protected static InterviewSyncPackageContent CreateInterviewSyncPackageContent(string someSyncPackageId, string someSyncPackageContent, string someSyncPackageMeta)
        {
            return new InterviewSyncPackageContent(someSyncPackageId, someSyncPackageContent, someSyncPackageMeta);
        }

        protected static QuestionnaireSyncPackageContent CreateQuestionnaireSyncPackageContent(string someSyncPackageId, string someSyncPackageContent, string someSyncPackageMeta)
        {
            return new QuestionnaireSyncPackageContent(someSyncPackageId, someSyncPackageContent, someSyncPackageMeta);
        }

        protected static UserSyncPackage CreateUserSyncPackageDto(Guid userId, string someSyncPackageContent, long sortIndex)
        {
            return new UserSyncPackage(userId, someSyncPackageContent, DateTime.Now, sortIndex);
        }
    }
}
