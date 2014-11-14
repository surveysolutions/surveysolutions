﻿using System;

using Machine.Specifications;

using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryReaderTests
{
    internal class when_getting_repository_entity_by_id_and_views_are_being_rebuilt : RavenReadSideRepositoryReaderTestsContext
    {
        Establish context = () =>
        {
            var readSideStatusService = Mock.Of<IReadSideStatusService>(service
                => service.AreViewsBeingRebuiltNow() == true);

            reader = CreateRavenReadSideRepositoryReader<IReadSideRepositoryEntity>(readSideStatusService: readSideStatusService);
        };

        Because of = () =>
            exception = Catch.Exception(() => 
                reader.GetById(Guid.Empty.ToString()));

        It should_throw_maintenance_exception = () =>
            exception.ShouldBeOfExactType<MaintenanceException>();

        private static Exception exception;
        private static RavenReadSideRepositoryReader<IReadSideRepositoryEntity> reader;
    }
}