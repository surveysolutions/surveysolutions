using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_getting_interview_ids_and_device_is_not_registered : SyncManagerTestContext
    {
        Establish context = () =>
        {
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>();
            syncManager = CreateSyncManager(devices: devices);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                syncManager.GetInterviewPackageIdsWithOrder(userId, deviceId, lastSyncedPackageId: null));

        It should_throw_ArgumentException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentException>();

        It should_throw_exception_with_message_containting__device_was_not_found__ = () =>
            new[] { "device was not found" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static SyncManager syncManager;
        private static Exception exception;
        private static Guid deviceId = "Android".ToGuid();
        private static IReadSideRepositoryReader<TabletDocument> devices;
        private static readonly Guid userId = Guid.Parse("11111111111111111111111111111111");
    }
}