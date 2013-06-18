using System;

using Machine.Specifications;

using Moq;

using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;

using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryReaderTests
{
    internal class when_counting_repository_entities_and_views_are_being_rebuilt : RavenReadSideRepositoryReaderTestsContext
    {
        Establish context = () =>
        {
            var readLayerStatusService = Mock.Of<IReadLayerStatusService>(service
                => service.AreViewsBeingRebuiltNow() == true);

            reader = CreateRavenReadSideRepositoryReader<IReadSideRepositoryEntity>(readLayerStatusService: readLayerStatusService);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                reader.Count());

        It should_throw_maintenance_exception = () =>
            exception.ShouldBeOfType<MaintenanceException>();

        private static Exception exception;
        private static RavenReadSideRepositoryReader<IReadSideRepositoryEntity> reader;
    }
}