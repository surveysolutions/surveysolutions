﻿using System;
using Machine.Specifications;
using Main.DenormalizerStorage;
using Moq;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.Synchronization;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

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
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> interviewSyncPackageReader  = null,
            IQueryableReadSideRepositoryReader<QuestionnaireSyncPackageMeta> questionnairesReader = null,
            IQueryableReadSideRepositoryReader<UserSyncPackageMeta> usersReader = null,
            ISyncLogger syncLogger = null)
        {
            return new SyncManager(
                devices ?? Mock.Of<IReadSideRepositoryReader<TabletDocument>>(),
                incomingSyncPackagesQueue ?? Mock.Of<IIncomingSyncPackagesQueue>(),
                commandService ?? Mock.Of<ICommandService>(),
                interviewSyncPackageReader ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta>>(),
                usersReader ?? Stub.ReadSideRepository<UserSyncPackageMeta>(),
                questionnairesReader ?? Stub.ReadSideRepository<QuestionnaireSyncPackageMeta>(),
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

        protected static QuestionnaireSyncPackageMeta CreateQuestionnaireSyncPackageMetaInformation(Guid questionnaireId, long questionnaireVersion, long sortIndex, DateTime? timestamp = null, string itemType = null, int contentSize = 20, int metaInfoSize = 10)
        {
            return new QuestionnaireSyncPackageMeta(
                questionnaireId,
                questionnaireVersion,
                timestamp ?? DateTime.Now,
                itemType ?? SyncItemType.Questionnaire,
                contentSize,
                metaInfoSize)
                   {
                       SortIndex = sortIndex,
                       PackageId = string.Format("{0}_{1}${2}", questionnaireId.FormatGuid(), questionnaireVersion, sortIndex)
                   };
        }

        protected static UserSyncPackageMeta CreateUserSyncPackage(Guid userId, int sortIndex)
        {
            return new UserSyncPackageMeta(userId, DateTime.Now)
                   {
                       SortIndex = sortIndex,
                       PackageId = string.Format("{0}${1}", userId.FormatGuid(), sortIndex)
                   };
        }

        protected static InterviewSyncPackageMeta CreateInterviewSyncPackageMetaInformation(Guid interviewId, int sortIndex, string itemType, Guid userId)
        {
            return new InterviewSyncPackageMeta(interviewId, Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 1, DateTime.Now, userId, itemType, 10, 5)
                   {
                       PackageId = string.Format("{0}${1}", interviewId.FormatGuid(), sortIndex)
                   };
        }
    }
}
