using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    [Ignore("Postgres")]
    internal class when_getting_more_than_5120_interview_ids_and_device_is_registered_and_last_package_id_is_empty : SyncManagerTestContext
    {
        Establish context = () =>
        {
            tabletDocument = CreateTabletDocument(deviceId, androidId);
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>(x => x.GetById(deviceId.FormatGuid()) == tabletDocument);

            interviewSyncPackageMetas = new List<InterviewSyncPackageMeta>();
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < chunckSize; j++)
                {
                    interviewSyncPackageMetas.Add(CreateInterviewSyncPackageMetaInformation(Guid.NewGuid(),
                        sortIndex: i*chunckSize + (j + 1), itemType: SyncItemType.Interview, userId: userId));
                }

                interviewSyncPackageMetas.Add(CreateInterviewSyncPackageMetaInformation(Guid.NewGuid(),
                       sortIndex: i * chunckSize + (chunckSize + 1), itemType: SyncItemType.DeleteInterview, userId: userId));
            }

            indexAccessorMock = new Mock<IReadSideRepositoryIndexAccessor>();
            //indexAccessorMock.Setup(x => x.Query<InterviewSyncPackageMeta>(interviewQueryIndexName))
            //    .Returns(QuerySyncPackage);
            syncManager = CreateSyncManager(devices: devices, indexAccessor: indexAccessorMock.Object);
        };

        private static IQueryable<InterviewSyncPackageMeta> QuerySyncPackage()
        {
            var result = interviewSyncPackageMetas.Take(chunckSize * (queryCount+1)).AsQueryable();
            queryCount++;
            return result;
        }

        Because of = () =>
         result = syncManager.GetInterviewPackageIdsWithOrder(userId, deviceId, null);


        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_return_list_with_5120_package_ids = () =>
            result.SyncPackagesMeta.Count().ShouldEqual(5120);

       It should_not_contain_delete_packages = () =>
            result.SyncPackagesMeta.ShouldNotContain(x => x.ItemType == SyncItemType.DeleteInterview);

       It should_not_contain_only_interview_packages = () =>
            result.SyncPackagesMeta.Count(x => x.ItemType == SyncItemType.Interview).ShouldEqual(result.SyncPackagesMeta.Count());

        private static SyncManager syncManager;
        private static SyncItemsMetaContainer result;

        private const string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static TabletDocument tabletDocument;
        private static IReadSideRepositoryReader<TabletDocument> devices;

        private static readonly Guid userId = Guid.Parse("11111111111111111111111111111111");

        private static Mock<IReadSideRepositoryIndexAccessor> indexAccessorMock;
        private static readonly int chunckSize=128;
        private static List<InterviewSyncPackageMeta> interviewSyncPackageMetas = new List<InterviewSyncPackageMeta>();
        private static int queryCount = 0;
    }
}
