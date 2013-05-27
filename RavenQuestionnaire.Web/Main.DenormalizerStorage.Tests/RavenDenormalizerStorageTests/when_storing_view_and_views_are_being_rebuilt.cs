using System;

using Machine.Specifications;

using Moq;

using WB.Core.Infrastructure;

using It = Machine.Specifications.It;

namespace Main.DenormalizerStorage.Tests.RavenDenormalizerStorageTests
{
    internal class when_storing_view_and_views_are_being_rebuilt : RavenDenormalizerStorageTestsContext
    {
        Establish context = () =>
        {
            var readLayerStatusService = Mock.Of<IReadLayerStatusService>(service
                => service.AreViewsBeingRebuiltNow() == true);

            storage = CreateRavenDenormalizerStorage<object>(readLayerStatusService: readLayerStatusService);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                storage.Store(new object(), Guid.Empty));

        It should_throw_maintenance_exception = () =>
            exception.ShouldBeOfType<MaintenanceException>();

        private static Exception exception;
        private static RavenDenormalizerStorage<object> storage;
    }
}